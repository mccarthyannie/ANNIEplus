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

namespace Annie_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly DataContext _context;

        public BookingsController(DataContext context)
        {
            _context = context;
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
                                                                                                Email = b.User.Id,                                                                                
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

            var count = await _context.Bookings.CountAsync(b => b.SessionId == request.SessionId);
            if (count >= session.Capacity)
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
            Console.WriteLine("AAAAAAAAA\nAAAAAAAAAAAAAA\nAAAAAAAAAAAA\n");
            Console.WriteLine("before email from claims");
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            Console.WriteLine("before email empty check");
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

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
