using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Annie_API.Models;
using Annie_API.DTOs;
using Annie_API.Authorization;

namespace Annie_API.Controllers
{

    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly Authorizator authorizator = new Authorizator();

        public AuthController(DataContext context)
        {
            _context = context;
        }

        // POST: api/login
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Route("api/login")]
        [HttpPost]
        public async Task<ActionResult<UserDTO>> LoginUser(LoginRequest request)
        {
            var user =  _context.Users
                .FirstOrDefault(u => u.Email == request.Email);

            if (user == null) 
            {
                return Unauthorized("Invalid Email");
            }

            if (!authorizator.VerifyPassword(request.Password, user.Password))
            {
                return Unauthorized("Invalid Password");
            }

            return Ok(new UserDTO 
            { 
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
            });
        }

        // POST: api/login
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Route("api/register")]
        [HttpPost]
        public async Task<ActionResult<UserDTO>> RegisterUser(RegisterUserRequest request)
        {
            if (EmailExists(request.Email))
            {
                return BadRequest("Email already exists");
            }

            User user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = authorizator.HashPassword(request.Password),
                Role = UserRole.User
            };

            _context.Users.Add(user);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (EmailExists(user.Email))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
            });
        }

        private bool EmailExists(string email)
        {
            return _context.Users.Any(e => e.Email == email);
        }
    }
}
