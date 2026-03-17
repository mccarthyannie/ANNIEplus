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
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            return await _context.Bookings.ToListAsync();
        }

        // GET: api/Bookings/User/5
        [HttpGet("User/{id}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookingByUser(long id)
        {
            var bookings = await _context.Bookings.Where(u => u.UserId == id).ToListAsync();

            return bookings;
        }

        // GET: api/Bookings/Session/5
        [HttpGet("Session/{id}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookingBySession(long id)
        {
            var bookings = await _context.Bookings.Where(u => u.SessionId == id).ToListAsync();

            return bookings;
        }

        // PUT: api/Bookings/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBooking(long id, Booking booking)
        {
            if (id != booking.Id)
            {
                return BadRequest();
            }

            _context.Entry(booking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // TODO : use authorize and claim to validate user and prevent booking to other users 
        // POST: api/Bookings
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Booking>> PostBooking(BookingRequest request)
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

            var booking = new Booking
            {
                UserId = request.UserId,
                SessionId = request.SessionId,
                BookingDate = DateTime.UtcNow
            };

            var count = await _context.Bookings.CountAsync(b => b.SessionId == booking.SessionId);
            if (count >= booking.Session.Capacity) { 
                return BadRequest("Session is fully booked.");
            }
            if (booking.Session.StartTime <= DateTime.UtcNow) { 
                return BadRequest("Session has already started.");
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBooking", new { id = booking.Id }, booking);
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

        private bool BookingExists(long id)
        {
            return _context.Bookings.Any(e => e.Id == id);
        }
    }
}
