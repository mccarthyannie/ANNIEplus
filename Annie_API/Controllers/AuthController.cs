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
using Annie_API.Data;
using Annie_API.UnitsOfWork.Interfaces;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

namespace Annie_API.Controllers
{

    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;
        private readonly IConfiguration _configuration;

        public AuthController(IUsersUnitOfWork usersUnitOfWork, IConfiguration configuration)
        {
            _usersUnitOfWork = usersUnitOfWork;
            _configuration = configuration;
        }

        // POST: api/login
        [Route("api/login")]
        [HttpPost]
        public async Task<ActionResult<TokenDTO>> LoginUser([FromBody] LoginRequest request)
        {
            var result = await _usersUnitOfWork.LoginAsync(request);

            if (!result.Succeeded) 
            {
                return BadRequest("Email or Password are wrong.");
            }

            var user = await _usersUnitOfWork.GetUserAsync(request.Email);
    
            return Ok(BuildToken(user));
        }


        // Create new User type
        // POST: api/login
        [Route("api/register")]
        [HttpPost]
        public async Task<ActionResult<TokenDTO>> RegisterUser(RegisterUserRequest request)
        {
            if (await EmailExists(request.Email))
            {
                return BadRequest("Email already exists.");
            }

            User user = new()
            {
                Name = request.Name,
                Email = request.Email,
                Role = UserRole.User,
                UserName = request.Email
            };

            var result = await _usersUnitOfWork.AddUserAsync(user, request.Password);


            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            await _usersUnitOfWork.AddUserToRoleASync(user, (UserRole.User).ToString());

            return Ok(BuildToken(user));
        }


        // Create new Instructor user
        // POST: api/login/instructor
        [Route("api/register/instructor")]
        [HttpPost]
        public async Task<ActionResult<TokenDTO>> RegisterInstructor(RegisterUserRequest request)
        {
            if (await EmailExists(request.Email))
            {
                return BadRequest("Email already exists.");
            }

            User user = new()
            {
                Name = request.Name,
                Email = request.Email,
                Role = UserRole.User,
                UserName = request.Email
            };

            var result = await _usersUnitOfWork.AddUserAsync(user, request.Password);


            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            await _usersUnitOfWork.AddUserToRoleASync(user, (UserRole.Instructor).ToString());

            return Ok(BuildToken(user));
        }

        private TokenDTO BuildToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("Name", user.Name)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwtKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddDays(7);
            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
                );

            return new TokenDTO
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = expiration
            };
        }

        private async Task<bool> EmailExists(string email)
        {
            return (await _usersUnitOfWork.GetUserAsync(email)) != null;
        }
    }
}
