using Annie_API.Authorization;
using Annie_API.Data;
using Annie_API.DTOs;
using Annie_API.Models;
using Annie_API.UnitsOfWork.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Annie_API.Controllers
{

    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUsersUnitOfWork _usersUnitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IEmailComposer _emailComposer;
        private readonly DataContext _context;

        public AuthController(IUsersUnitOfWork usersUnitOfWork, IConfiguration configuration, IEmailComposer emailComposer, DataContext context)
        {
            _usersUnitOfWork = usersUnitOfWork;
            _configuration = configuration;
            _emailComposer = emailComposer;
            _context = context;
        }

        // POST: api/login
        [Route("api/login")]
        [HttpPost]
        public async Task<ActionResult<TokenDTO>> LoginUser([FromBody] LoginRequest request)
        {
            var result = await _usersUnitOfWork.LoginAsync(request);

            if (result.Succeeded)
            {
                var user = await _usersUnitOfWork.GetUserAsync(request.Email);

                return Ok(BuildToken(user));
            }

            if (result.IsLockedOut)
            { 
                return BadRequest("Account is locked. Please try again later.");
            }

            if (result.IsNotAllowed)
            { 
                return BadRequest("Email not confirmed. Please check your email for confirmation link.");
            }

            return BadRequest("Email or Password are wrong.");
        }


        // Create new User type
        // POST: api/login
        [Route("api/register")]
        [HttpPost]
        public async Task<ActionResult> RegisterUser(RegisterUserRequest request)
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
            var response = await SendConfirmationEmailAsnyc(user);
            if (response) 
            {
                return NoContent();
            }
            else 
            {
                return BadRequest("Failed to send confirmation email.");
            }
        }


        // Create new Instructor user
        // POST: api/login/instructor
        [Route("api/register/instructor")]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> RegisterInstructor(RegisterUserRequest request)
        {
            if (await EmailExists(request.Email))
            {
                return BadRequest("Email already exists.");
            }

            User user = new()
            {
                Name = request.Name,
                Email = request.Email,
                Role = UserRole.Instructor,
                UserName = request.Email
            };

            var result = await _usersUnitOfWork.AddUserAsync(user, request.Password);


            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.Select(e => e.Description));
            }

            await _usersUnitOfWork.AddUserToRoleASync(user, UserRole.Instructor.ToString());
            var response = await SendConfirmationEmailAsnyc(user);
            if (response)
            {
                return NoContent();
            }
            else
            {
                return BadRequest("Failed to send confirmation email.");
            }
        }


        [HttpGet("api/auth/ConfirmEmail")]
        public async Task<ActionResult> ConfirmEmail(string userId, string token)
        {
            token = token.Replace(' ', '+');

            var user = await _context.Users.FindAsync(userId);

            if (user == null) 
            {
                return BadRequest("Invalid user.");
            }

            var result = await _usersUnitOfWork.ConfirmEmailAsync(user, token);
            if (!result.Succeeded) 
            {
                return BadRequest(result.Errors.FirstOrDefault());
            }

            return Ok("Email confirmed successfully.");
        }


        [HttpPost("api/resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation([FromBody] EmailDTO email)
        {
            var user = await _usersUnitOfWork.GetUserAsync(email.email);
           
            if (user == null) 
            {
                return NotFound();
            }

            if (user.EmailConfirmed)
            {
                return BadRequest("User has already confirmed their Email!");
            }

            var response = await SendConfirmationEmailAsnyc(user);
            if (response)
            {
                return NoContent();
            }
            else
            {
                return BadRequest("Failed to send confirmation email.");
            }
        }

        [HttpPost("api/auth/ResetPassword")]
        public async Task<IActionResult> ResetPasswordAsnyc([FromBody] EmailDTO email)
        {
            var user = _usersUnitOfWork.GetUserAsync(email.email);
            if (user.Result == null)
            {
                return NotFound();
            }

            var resetToken = await _usersUnitOfWork.CreateResetPasswordToken(user.Result);
            var frontendBase = _configuration["Frontend Url"]!.TrimEnd('/');
            var encodedToken = WebUtility.UrlEncode(resetToken);
            var link = $"{frontendBase}/api/auth/ResetConfirmation?Token={encodedToken}";

            string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var emailBody = $@"
                    <div style=""background: linear-gradient(135deg, #fdfeff 0%, #e0f2fe 100%); padding: 40px 20px; font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #1e293b; text-align: center;"">
                        <div style=""max-width: 500px; margin: 0 auto; background: rgba(255, 255, 255, 0.7); border: 1px solid rgba(255, 255, 255, 0.4); border-radius: 24px; padding: 40px; box-shadow: 0 10px 30px rgba(14, 165, 233, 0.05);"">
        
                            <div style=""width: 80px; height: 80px; background: radial-gradient(circle, #ffffff 0%, #bae6fd 60%, transparent 100%); border-radius: 50%; margin: 0 auto 20px;""></div>

                            <h1 style=""font-weight: 600; font-size: 24px; letter-spacing: -0.02em; margin-bottom: 8px;"">Annie+ Password Reset</h1>
                            <p style=""color: #64748b; font-size: 16px; line-height: 1.5;"">Please click the link below to reset your password.</p>
        
                            <div style=""margin: 32px 0;"">
                                <a href=""{link}"" target=""_blank"" rel=""noopener noreferrer"" style=""background: #0ea5e9; color: #ffffff; padding: 14px 32px; border-radius: 12px; text-decoration: none; font-weight: 500; display: inline-block; box-shadow: 0 4px 15px rgba(14, 165, 233, 0.2);"">Reset Password</a>
                            </div>

                            <div style=""border-top: 1px solid rgba(226, 232, 240, 0.8); margin-top: 32px; padding-top: 24px;"">
                                <p style=""color: #94a3b8; font-size: 13px; margin-bottom: 8px;"">If the button does not work, copy and paste the URL below into your browser:</p>
                                <p style=""color: #0ea5e9; font-size: 12px; word-break: break-all; opacity: 0.8;"">{link}</p>
                            </div>
                        </div>

                        <div style=""max-width: 500px; margin: 24px auto 0; text-align: center;"">
                            <hr style=""border: 0; border-top: 1px solid rgba(14, 165, 233, 0.1); margin-bottom: 20px;"">
                            <p style=""color: #94a3b8; font-size: 11px; line-height: 1.6; margin-bottom: 4px;"">
                                This email can't receive replies.
                            </p>
                            <p style=""color: #94a3b8; font-size: 11px;"">
                                 © Annie+, 240 Prince Phillip Drive, 40 Arctic Ave, St. John's, NL A1B 3X7
                            </p>
                            <p style=""display:none !important; font-size:1px; color:#ffffff; line-height:1px; max-height:0px; opacity:0; overflow:hidden;"">Ref: {uniqueId}</p>
                        </div>
                    </div>";

            var emailResult =  _emailComposer.ComposeEmail(
                user.Result.Name,
                user.Result.Email!,
                "Annie+ User Confirmation",
                emailBody
                );

            if (emailResult)
            {
                return NoContent();
            }

            return BadRequest("Error sending Reset Password Email");
        }


        [HttpPost("api/auth/ResetConfirmation")]
        public async Task<IActionResult> PasswordResetConfirmationAsync([FromBody] PasswordResetDTO passwordReset)
        {
            var user = _usersUnitOfWork.GetUserAsync(passwordReset.Email);
            if (user.Result == null)
            {
                return NotFound();
            }

            var result = await _usersUnitOfWork.ResetPasswordAsync(user.Result, passwordReset.Token, passwordReset.NewPassword);
            if (result.Succeeded)
            {
                return NoContent();
            }

            return BadRequest(result.Errors.FirstOrDefault()?.Description);
        }


        private async Task<bool> SendConfirmationEmailAsnyc(User user)
        {

            var confirmationToken = await _usersUnitOfWork.CreateConfirmationToken(user);

            var frontendBase = _configuration["Frontend Url"]!.TrimEnd('/');
            var encodedToken = WebUtility.UrlEncode(confirmationToken);
            var encodedId = WebUtility.UrlEncode(user.Id);
            var link = $"{frontendBase}/api/auth/ConfirmEmail?UserId={encodedId}&Token={encodedToken}";

            string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            var emailBody = $@"
                    <div style=""background: linear-gradient(135deg, #fdfeff 0%, #e0f2fe 100%); padding: 40px 20px; font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #1e293b; text-align: center;"">
                        <div style=""max-width: 500px; margin: 0 auto; background: rgba(255, 255, 255, 0.7); border: 1px solid rgba(255, 255, 255, 0.4); border-radius: 24px; padding: 40px; box-shadow: 0 10px 30px rgba(14, 165, 233, 0.05);"">
        
                            <div style=""width: 80px; height: 80px; background: radial-gradient(circle, #ffffff 0%, #bae6fd 60%, transparent 100%); border-radius: 50%; margin: 0 auto 20px;""></div>

                            <h1 style=""font-weight: 600; font-size: 24px; letter-spacing: -0.02em; margin-bottom: 8px;"">Annie+ User Confirmation</h1>
                            <p style=""color: #64748b; font-size: 16px; line-height: 1.5;"">Please click the link below to confirm your e-mail and use Annie+!</p>
        
                            <div style=""margin: 32px 0;"">
                                <a href=""{link}"" target=""_blank"" rel=""noopener noreferrer"" style=""background: #0ea5e9; color: #ffffff; padding: 14px 32px; border-radius: 12px; text-decoration: none; font-weight: 500; display: inline-block; box-shadow: 0 4px 15px rgba(14, 165, 233, 0.2);"">Confirm Email</a>
                            </div>

                            <div style=""border-top: 1px solid rgba(226, 232, 240, 0.8); margin-top: 32px; padding-top: 24px;"">
                                <p style=""color: #94a3b8; font-size: 13px; margin-bottom: 8px;"">If the button does not work, copy and paste the URL below into your browser:</p>
                                <p style=""color: #0ea5e9; font-size: 12px; word-break: break-all; opacity: 0.8;"">{link}</p>
                            </div>
                        </div>

                        <div style=""max-width: 500px; margin: 24px auto 0; text-align: center;"">
                            <hr style=""border: 0; border-top: 1px solid rgba(14, 165, 233, 0.1); margin-bottom: 20px;"">
                            <p style=""color: #94a3b8; font-size: 11px; line-height: 1.6; margin-bottom: 4px;"">
                                This email can't receive replies.
                            </p>
                            <p style=""color: #94a3b8; font-size: 11px;"">
                                 © Annie+, 240 Prince Phillip Drive, 40 Arctic Ave, St. John's, NL A1B 3X7
                            </p>
                            <p style=""display:none !important; font-size:1px; color:#ffffff; line-height:1px; max-height:0px; opacity:0; overflow:hidden;"">Ref: {uniqueId}</p>
                        </div>
                    </div>";

            return _emailComposer.ComposeEmail(
                user.Name,
                user.Email!,
                "Annie+ User Confirmation",
                emailBody
                );
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
