using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Annie_API.Models;
using Annie_API.DTOs;
using Annie_API.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Diagnostics;
using Humanizer;

namespace Annie_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IEmailComposer _emailComposer;
        private readonly IConfiguration _configuration;

        public BookingsController(DataContext context, IEmailComposer emailComposer, IConfiguration configuration)
        {
            _context = context;
            _emailComposer = emailComposer;
            _configuration = configuration;
        }

        // GET: api/Bookings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> GetBookings()
        {
            return await _context.Bookings.Select(b => 
                                                        new BookingDTO 
                                                            { Id = b.Id,
                                                                Email = b.User.Email,
                                                                SessionId = b.SessionId,
                                                                SessionName = b.Session.Name,
                                                                BookingDate = b.BookingDate})
                                                            .ToListAsync();
        }


        // Returns a booking by its id 
        // GET: api/Bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDTO>> GetBooking(long id)
        {
            var booking = await _context.Bookings
                                            .Include(b => b.User)
                                            .Include(b => b.Session)
                                            .FirstOrDefaultAsync(b=> b.Id == id);

            if (booking == null)
            {
                return NotFound();
            }

            return Ok(new BookingDTO
            {
                Id = booking.Id,
                Email = booking.User.Email,
                SessionId = booking.SessionId,
                SessionName = booking.Session.Name,
                BookingDate = booking.BookingDate
            });
        }


        // Returns the sessions associated with the user
        // GET: api/Bookings/User/5
        [HttpGet("User")]
        [Authorize]
        public async Task<ActionResult<List<Session>>> GetBookingByUser()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (String.IsNullOrEmpty(email)) 
            {
                return Forbid();
            }

            return await _context.Bookings.Where(u => u.User.Email == email).Select(u => u.Session).ToListAsync();
        }


        // Returns the users that booked the session
        // GET: api/Bookings/Session/5
        [HttpGet("Session/{id}")]
        [Authorize(Policy = "CanChangeSessions")]
        public async Task<ActionResult<List<UserDTO>>> GetBookingBySession(long id)
        {
            return await _context.Bookings.Where(b => b.SessionId == id).Select(b => new UserDTO 
                                                                                            {Name = b.User.Name,
                                                                                                Email = b.User.Email,                                                                                
                                                                                                Role = b.User.Role}) 
                                                                                                .ToListAsync();
        }

        // POST: api/Bookings
        [HttpPost]
        [Authorize(Policy = "AnyValidUser")]
        public async Task<ActionResult<BookingDTO>> PostBooking(BookingRequest request)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (String.IsNullOrEmpty(email))
            {
                return Forbid();
            }

            if (request == null)
            {
                return BadRequest();
            }
            
            var session = await _context.Sessions.FindAsync(request.SessionId);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);


            if (user == null || session == null)
            { 
                return BadRequest("User and Session must be provided.");
            }

            var repeats = await _context.Bookings.CountAsync(b => b.SessionId == request.SessionId
                                                && b.UserId == user.Id);

            if (repeats != 0) {
                return BadRequest("Booking already exists. ");
            }

            if (session.Capacity <= 0)
            {
                return BadRequest("Session is fully booked.");
            }
            if (session.StartTime <= DateTime.UtcNow)
            {
                return BadRequest("Session has already started.");
            }

            var booking = new Booking
            {
                UserId = user.Id,
                SessionId = request.SessionId,
                BookingDate = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            session.Capacity--;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException?.Message.Contains("duplicate") == true)
                {
                    return Conflict("A conflict in the database occurred " + ex.InnerException.Message);
                }

                return BadRequest("The booking could not be processed. Error: " + ex.Message);
            }

            var response = await SendBookingConfirmationEmail(booking);
            if (!response)
            {
                Console.WriteLine("Error Sending Confirmation Email.");
            }

            return CreatedAtAction("GetBooking", 
                                    new { id = booking.Id }, 
                                    new BookingDTO { 
                                        Id = booking.Id,
                                        Email = user.Email,
                                        SessionId = booking.SessionId,
                                        SessionName = session.Name,
                                        BookingDate = booking.BookingDate});
        }

        // DELETE: api/Bookings/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteBooking(long id)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (String.IsNullOrEmpty(email))
            {
                return Forbid();
            }
            
            var booking = await _context.Bookings.Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id);
            if (booking == null)
            {
                return NotFound();
            }

            if (booking.User.Email != email) 
            {
                return Forbid();
            }

            var session = await _context.Sessions.FindAsync(booking.SessionId);
            if (session != null)
                session.Capacity++;

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> SendBookingConfirmationEmail(Booking booking)
        {
            string messageBody = $@"
                <div style=""background: linear-gradient(135deg, #ffffff 0%, #f0f9ff 100%); padding: 40px 20px; font-family: 'Segoe UI', Roboto, sans-serif; color: #1e293b; text-align: center;"">
                    <div style=""max-width: 500px; margin: 0 auto; background: rgba(255, 255, 255, 0.8); border: 1px solid rgba(255, 255, 255, 0.6); border-radius: 30px; padding: 40px; box-shadow: 0 20px 40px rgba(0, 0, 0, 0.02);"">
        
                        <div style=""width: 60px; height: 60px; background: radial-gradient(circle, #ffffff 0%, #e0f2fe 70%, transparent 100%); border-radius: 50%; margin: 0 auto 16px;""></div>

                        <h1 style=""font-weight: 700; font-size: 22px; color: #0f172a; margin-bottom: 24px;"">Annie+ Booking Confirmation</h1>
                        <p style=""font-size: 16px; margin-bottom: 16px;"">Your session has been booked!</p>";

            if (!String.IsNullOrEmpty(booking.Session!.Location))
            {
                messageBody += $@"<p style=""background: #f8fafc; padding: 12px; border-radius: 12px; color: #475569;"">The session will occur <b>{booking.Session.StartTime.Humanize()}</b> at <b>{booking.Session.Location}</b></p>";
            }
            else
            {
                messageBody += $@"<p style=""background: #f8fafc; padding: 12px; border-radius: 12px; color: #475569;"">The session will occur <b>{booking.Session.StartTime.Humanize()}</b></p>";
            }

            string uniqueId = Guid.NewGuid().ToString().Substring(0, 8);
            messageBody += $@"
                        <p style=""margin-top: 24px; font-weight: 500; color: #0ea5e9;"">We are excited to see you there!</p>
                        <div style=""margin-top: 32px;"">
                            <a href=""{_configuration["Frontend Url"]}"" style=""text-decoration: none; color: #64748b; font-size: 14px; border-bottom: 1px solid #bae6fd; padding-bottom: 2px;"">Click Here to see your Bookings</a>
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
                booking.User!.Name,
                booking.User.Email!,
                "Annie+ Booking Confirmation",
                messageBody
                );
        }
    }
}
