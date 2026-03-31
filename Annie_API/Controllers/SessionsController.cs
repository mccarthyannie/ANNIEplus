using Annie_API.Data;
using Annie_API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Annie_API.Controllers
{
    [Route("api/Sessions")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IEmailComposer _emailComposer;
        private readonly IConfiguration _configuration;

        // Dependency injection of the DataContext to interact with the database
        public SessionsController(DataContext context, IEmailComposer emailComposer, IConfiguration configuration)
        {
            _context = context;
            _emailComposer = emailComposer;
            _configuration = configuration;
        }

        // GET: api/Sessions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Session>>> GetSessions()
        {
            return await _context.Sessions.Where(s => s.Status == SessionStatus.Available).ToListAsync();
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
                             s.StartTime < nextMonthStart &&
                             s.Status == SessionStatus.Available))
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
        [HttpPut("{id}")]
        [Authorize(Policy = "CanChangeSessions")]
        public async Task<ActionResult<Session>> PutSession(long id, Session session)
        {
            string validationError = isValidSession(session);
            if (!String.IsNullOrEmpty(validationError))
            {
                return BadRequest(validationError);
            }

            var currentSession = await _context.Sessions
                .Include(s => s.Bookings)
                .ThenInclude(b => b.User)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (currentSession == null)
            {
                return NotFound();
            }

            bool cancelled  = (currentSession.Status != SessionStatus.Cancelled && session.Status == SessionStatus.Cancelled);
            bool modified = (currentSession.Location != session.Location
                || currentSession.StartTime != session.StartTime
                || cancelled)
                && currentSession.Bookings!.Count != 0;
            // conditions to notify users of session changes: location change, time change, or cancellation (but not uncancellation)
            if (modified)
            {
                var usersinSession = currentSession.Bookings
                                    .Select(b => b.User)
                                    .ToList();

                var result = true;
                foreach (var user in usersinSession)
                {
                    result = result && await SendSessionEmail(user, session, cancelled);
                }
                if (!result)
                {
                    Console.WriteLine("Error Sending Session Cancellation Email.");
                }
                else Console.WriteLine("Session change confirmation Emails sent.");
            }

            _context.Entry(currentSession).CurrentValues.SetValues(session);
            _context.Entry(currentSession).State = EntityState.Modified;

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

            return Ok(session);
        }

        // POST: api/Sessions
        [HttpPost]
        [Authorize(Policy = "CanChangeSessions")]
        public async Task<ActionResult<Session>> PostSession(Session session)
        {
            string validationError = isValidSession(session);
            if (!String.IsNullOrEmpty(validationError)) 
            {
                return BadRequest(validationError);
            }

            _context.Sessions.Add(session);
           
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return BadRequest($"Database Exception with message: {ex.Message}");
            }


            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }


        // DELETE: api/Sessions/5
        [HttpDelete("{id}")]
        [Authorize(Policy = "CanChangeSessions")]
        public async Task<IActionResult> DeleteSession(long id)
        {
            var session = await _context.Sessions
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == id);


            if (session == null)
            {
                return NotFound();
            }

            if(session.Bookings != null)
            {
                var usersinSession = session.Bookings
                                   .Select(b => b.User!)
                                   .ToList();

                if (usersinSession.Count != 0)
                {
                    var result = true;
                    foreach (var user in usersinSession)
                    {
                        result = result && await SendSessionEmail(user, session, true);
                    }
                    if (!result)
                    {
                        Console.WriteLine("Error Sending Session Cancellation Email.");
                    }
                    _context.Bookings.RemoveRange(session.Bookings);
                }
            }
           

            session.Status = SessionStatus.Cancelled;
           

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
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
        }

        // ideally, this would be done in the background to increase performance
        private async Task<bool> SendSessionEmail(User user, Session session, bool cancelled)
        {
            var link = _configuration["Frontend Url"]!.TrimEnd('/');
            
            string title, subject, notice, sessionInfo = string.Empty;
            
            if (cancelled)
            {
                subject = "Annie + Session Cancelled";
                title = "Annie+ Session Cancellation";
                notice = "Unfortunately one of your sessions has been cancelled:(";
                sessionInfo = $"The session, {session.Name}, on {session.StartTime} will no longer occur.";

            }
            else           
            { 
                subject = "Annie + Session Updated";
                title = "Annie+ Session Update";
                notice = "One of your sessions has been updated. Please check the details below.";
                sessionInfo = $"The session, {session.Name}, has been updated. It will now occur on {session.StartTime} at {session.Location}.";
            }

            var emailBody = $@"
                    <div style=""background: linear-gradient(135deg, #fdfeff 0%, #e0f2fe 100%); padding: 40px 20px; font-family: 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; color: #1e293b; text-align: center;"">
                        <div style=""max-width: 500px; margin: 0 auto; background: rgba(255, 255, 255, 0.7); border: 1px solid rgba(255, 255, 255, 0.4); border-radius: 24px; padding: 40px; box-shadow: 0 10px 30px rgba(14, 165, 233, 0.05);"">
        
                            <div style=""width: 80px; height: 80px; background: radial-gradient(circle, #ffffff 0%, #bae6fd 60%, transparent 100%); border-radius: 50%; margin: 0 auto 20px;""></div>

                            <h1 style=""font-weight: 600; font-size: 24px; letter-spacing: -0.02em; margin-bottom: 8px;"">{title}</h1>
                            <p style=""color: #64748b; font-size: 16px; line-height: 1.5;"">Hello, {user.Name}</p>
                            <p style=""color: #64748b; font-size: 16px; line-height: 1.5;"">{notice}</p>
                            
                            <div style=""border-top: 1px solid rgba(226, 232, 240, 0.8); margin-top: 32px; padding-top: 24px;"">
                                <p style=""color: #94a3b8; font-size: 13px; margin-bottom: 8px;"">{sessionInfo}</p>
                            </div>

                            <div style=""border-top: 1px solid rgba(226, 232, 240, 0.8); margin-top: 32px; padding-top: 24px;"">
                                <p style=""color: #94a3b8; font-size: 13px; margin-bottom: 8px;""></p>
                                <a href=""{link}"" target=""_blank"" rel=""noopener noreferrer"" style=""background: #0ea5e9; color: #ffffff; padding: 14px 32px; border-radius: 12px; text-decoration: none; font-weight: 500; display: inline-block; box-shadow: 0 4px 15px rgba(14, 165, 233, 0.2);"">Create new bookings here!</a>
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
                        </div>
                    </div>";

            return _emailComposer.ComposeEmail(
                user.Name,
                user.Email!,
                subject,
                emailBody
                );
        }

        private bool SessionExists(long id)
        {
            return _context.Sessions.Any(e => e.Id == id);
        }

        private string isValidSession(Session session) 
        {
            if (session == null)
            {
                return "Session data is null.";
            }
            if (session.StartTime >= session.EndTime)
            {
                return "Session start time must be before end time.";
            }
            if (session.Capacity < 0)
            {
                return "Session capacity cannot be negative.";
            }
            if (session.StartTime < DateTime.Today)
            {
                return "Session start time cannot be in the past.";
            }

            return string.Empty;
        }
    }
}
