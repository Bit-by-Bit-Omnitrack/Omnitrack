using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using UserRoles.Models;
using Microsoft.AspNetCore.Authorization;
using UserRoles.Models.Dtos;

namespace UserRoles.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CalendarEventsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public CalendarEventsController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetEventsByDateRange(string startDate, string endDate)
        {
            
            if (!DateTimeOffset.TryParse(startDate, out var startOffset) ||
                !DateTimeOffset.TryParse(endDate, out var endOffset))
            {
                return BadRequest("Invalid date format.");
            }

          
            var start = startOffset.DateTime;
            var end = endOffset.DateTime;

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var userId = user.Id;

            var customEvents = await _context.CalendarEvents
                .Where(e => e.UserId == userId && e.ScheduledDate >= start && e.ScheduledDate <= end)
                .Select(e => new CalendarEventDto
                {
                   Id = e.Id,
                    Title = e.Title,
                    Description = e.Description,
                    ScheduledDate = e.ScheduledDate,
                    EndDate = e.EndDate,
                    EventType = e.EventType,
                    TicketId = e.TicketId,
                    TasksId = e.TasksId,
                    ProjectId = e.ProjectId
                })
                .ToListAsync(); 

            var ticketEvents = await _context.Tickets
                .Where(t => t.DueDate != null && t.DueDate.Value.Date >= start.Date && t.DueDate.Value.Date <= end.Date)
                .Select(t => new CalendarEventDto
                {
                    Id = -t.Id,
                    Title = "🎫 Ticket: " + t.Title,
                    Description = "Ticket ID: " + t.TicketID,
                    ScheduledDate = t.DueDate,
                    EndDate = null,
                    EventType = "TicketDue",
                    TicketId = t.Id
                })
                .ToListAsync();

            var taskEvents = await _context.Tasks
                .Where(t => t.DueDate != null && t.DueDate.Value.Date >= start.Date && t.DueDate.Value.Date <= end.Date)
                .Select(t => new CalendarEventDto
                {
                    Id = -1000 - t.Id,
                    Title = "🎯 Task: " + t.Name,
                    Description = "Task ID: " + t.Id,
                    ScheduledDate = t.DueDate,
                    EndDate = null,
                    EventType = "TaskDue",
                    TasksId = t.Id
                })
                .ToListAsync();

            var allEvents = customEvents.Concat(ticketEvents).Concat(taskEvents).ToList();
            return Ok(allEvents);
        }

        // POST: api/CalendarEvents
        [HttpPost]
        public async Task<ActionResult<CalendarEvent>> PostCalendarEvent(CalendarEvent calendarEvent)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            calendarEvent.UserId = user.Id;
            calendarEvent.Id = 0; 

            _context.CalendarEvents.Add(calendarEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCalendarEvent", new { id = calendarEvent.Id }, calendarEvent);
        }

        // GET: api/CalendarEvents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CalendarEvent>> GetCalendarEvent(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var calendarEvent = await _context.CalendarEvents.FindAsync(id);

            if (calendarEvent == null || calendarEvent.UserId != user.Id)
            {
                return NotFound();
            }

            return calendarEvent;
        }

        // PUT: api/CalendarEvents/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCalendarEvent(int id, CalendarEvent calendarEvent)
        {
            if (id != calendarEvent.Id)
            {
                return BadRequest();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var existingEvent = await _context.CalendarEvents.FindAsync(id);

            if (existingEvent == null || existingEvent.UserId != user.Id)
            {
                return NotFound();
            }

            // Update allowed properties
            existingEvent.Title = calendarEvent.Title;
            existingEvent.Description = calendarEvent.Description;
            existingEvent.ScheduledDate = calendarEvent.ScheduledDate;
            existingEvent.EndDate = calendarEvent.EndDate;
            existingEvent.EventType = calendarEvent.EventType;
            existingEvent.TicketId = calendarEvent.TicketId;
            existingEvent.TasksId = calendarEvent.TasksId;
            existingEvent.ProjectId = calendarEvent.ProjectId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CalendarEventExists(id))
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

        // DELETE: api/CalendarEvents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCalendarEvent(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var calendarEvent = await _context.CalendarEvents.FindAsync(id);
            if (calendarEvent == null || calendarEvent.UserId != user.Id)
            {
                return NotFound();
            }

            _context.CalendarEvents.Remove(calendarEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CalendarEventExists(int id)
        {
            return _context.CalendarEvents.Any(e => e.Id == id);
        }
    }
}
