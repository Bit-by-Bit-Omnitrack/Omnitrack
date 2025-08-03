using UserRoles.DTOs;
using Microsoft.EntityFrameworkCore;
using UserRoles.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserRoles.Services
{
    public class TicketService : ITicketService
    {
        private readonly AppDbContext _context;

        public TicketService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TicketSummaryDto>> GetTicketSummariesAsync()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Select(t => new TicketSummaryDto
                {
                    Id = t.Id,
                    TicketID = t.TicketID,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status != null ? t.Status.StatusName : "Unknown",
                    Priority = t.Priority != null ? t.Priority.Name : "Normal",
                    AssignedToUserId = t.AssignedToUserId,
                    CreatedByID = t.CreatedByID,
                    CreatedDate = t.CreatedDate,
                    DueDate = t.DueDate,
                    TaskId = t.TasksId
                })
                .ToListAsync();

            return tickets;
        }
    }
}