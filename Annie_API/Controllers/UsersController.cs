using Annie_API.Authorization;
using Annie_API.Data;
using Annie_API.DTOs;
using Annie_API.Models;
using Annie_API.UnitsOfWork.Implementations;
using Annie_API.UnitsOfWork.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace Annie_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUsersUnitOfWork _usersUnitOfWork;
        private readonly Authorizator _authorizator = new Authorizator();

        public UsersController(DataContext context, IUsersUnitOfWork usersUnitOfWork)
        {
            _context = context;
            _usersUnitOfWork = usersUnitOfWork;
        }


        // GET: api/Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            return await _context.Users
                .Select(u => new UserDTO
            {
                Name = u.Name,
                Email = u.Email,
                Role = u.Role
            }).ToListAsync();
        }


        // GET: api/Users/Instructors
        [HttpGet("Instructors")]
        [Authorize(Policy = "CanChangeSessions")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetInstructors()
        {
            return await _context.Users
                .Where(u => u.Role == UserRole.Instructor)
                .Select( u => new UserDTO
                {
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                }).ToListAsync();
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutUser(User newUser)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (String.IsNullOrEmpty(email))
            {
                return Forbid();
            }

            var user = await _usersUnitOfWork.GetUserAsync(email);

            if (newUser.Id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).CurrentValues.SetValues(newUser);
            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                    throw;
            }

            return NoContent();
        }

        private bool UserExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
