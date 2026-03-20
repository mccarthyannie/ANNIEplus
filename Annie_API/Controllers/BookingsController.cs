using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Annie_API.Models;
using Annie_API.DTOs;

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
            return await _context.Bookings.Select(b => new BookingDTO 
                                                            { Id = b.Id,
                                                                UserId = b.UserId,
                                                                UserName = b.User.Name,
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
                UserId = booking.UserId,
                UserName = booking.User.Name,
                SessionId = booking.SessionId,
                SessionName = booking.Session?.Name,
                BookingDate = booking.BookingDate
            });
        }

        // Returns the sessions associated with the user
        // GET: api/Bookings/User/5
        [HttpGet("User/{id}")]
        public async Task<ActionResult<List<Session>>> GetBookingByUser(long id)
        {
            return await _context.Bookings.Where(u => u.UserId == id).Select(u => u.Session).ToListAsync();
        }

        // Returns the users that booked the session
        // GET: api/Bookings/Session/5
        [HttpGet("Session/{id}")]
        public async Task<ActionResult<List<UserDTO>>> GetBookingBySession(long id)
        {
            return await _context.Bookings.Where(b => b.SessionId == id).Select(b => new UserDTO 
                                                                                            { Id = b.User.Id,                                                                                Role = b.User.Role})
                                                                                            .ToListAsync();
        }

        // TODO : use authorize and claim to validate user and prevent booking to other users 
        // POST: api/Bookings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<BookingDTO>> PostBooking(BookingRequest request)
        {
            if (request == null)
            {
                return BadRequest();
            }
            
            var session = await _context.Sessions.FindAsync(request.SessionId);
            var user = await _context.Users.FindAsync(request.UserId);

            if (user == null || session == null)
            { 
                return BadRequest("User and Session must be provided.");
            }

            var count = await _context.Bookings.CountAsync(b => b.SessionId == request.SessionId);
            if (count >= session.Capacity) { 
                return BadRequest("Session is fully booked.");
            }
            if (session.StartTime <= DateTime.UtcNow) { 
                return BadRequest("Session has already started.");
            }

            var booking = new Booking
            {
                UserId = request.UserId,
                SessionId = request.SessionId,
                BookingDate = DateTime.UtcNow
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBooking", 
                                    new { id = booking.Id }, 
                                    new BookingDTO { 
                                        Id = booking.Id,
                                        UserId = booking.UserId,
                                        UserName = user.Name,
                                        SessionId = booking.SessionId,
                                        SessionName = session.Name,
                                        BookingDate = booking.BookingDate});
        }

        // DELETE: api/Bookings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBooking(long id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
        private bool SessionExists(long id)
        {
            return _context.Sessions.Any(e => e.Id == id);
        }
        private bool BookingExists(long id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}
