using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Annie_API.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Authorization;

namespace Annie_API.Controllers
{
    [Route("api/Sessions")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly DataContext _context;

        // Dependency injection of the DataContext to interact with the database
        public SessionsController(DataContext context)
        {
            _context = context;
        }

        // GET: api/Sessions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Session>>> GetSessions()
        {
            return await _context.Sessions.ToListAsync();
        }

        // GET: api/Sessions/month/november
        [HttpGet("month/{month}")]
        public async Task<ActionResult<IEnumerable<Session> >> GetSession(int month, int year=0)
        {
            if (month < 1 || month > 12)
            {
                return BadRequest("Invalid month. Please provide a value between 1 and 12.");
            }
            
            if(year == 0) year = DateTime.Now.Year;
            var monthStart = new DateTime(year, month, 1);
            var nextMonthStart = monthStart.AddMonths(1);

            var query = _context.Sessions
                        .Where(s =>
                            (s.StartTime >= monthStart &&
                             s.StartTime < nextMonthStart))
                        .OrderBy(s => s.StartTime);

            return await query.ToListAsync();
        }

        // GET: api/Sessions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Session>> GetSession(long id)
        {
            var session = await _context.Sessions.FindAsync(id);

            if (session == null)
            {
                return NotFound();
            }

            return session;
        }

        // PUT: api/Sessions/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        //[Authorize(Roles="Admin,Instructor")]
        public async Task<IActionResult> PutSession(long id, Session session)
        {
            if (id != session.Id)
            {
                return BadRequest();
            }

            _context.Entry(session).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SessionExists(id))
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

        // POST: api/Sessions
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        //[Authorize(Roles="Admin,Instructor")]
        public async Task<ActionResult<Session>> PostSession(Session session)
        {
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }

        // DELETE: api/Sessions/5
        [HttpDelete("{id}")]
        //[Authorize(Roles="Admin,Instructor")]
        public async Task<IActionResult> DeleteSession(long id)
        {
            var session = await _context.Sessions.FindAsync(id);
            if (session == null)
            {
                return NotFound();
            }

            _context.Sessions.Remove(session);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SessionExists(long id)
        {
            return _context.Sessions.Any(e => e.Id == id);
        }
    }
}
