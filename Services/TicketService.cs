using UserRoles.DTOs;
using Microsoft.EntityFrameworkCore;

namespace UserRoles.Services
{
    public class TicketService
    {
        private readonly ApplicationDbContext _context;

        public TicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TicketSummaryDto>> GetTicketSummariesAsync()
        {
            var tickets = await _context.Tickets
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Tasks)
                .Select(t => new TicketSummaryDto
                {
                    Id = t.Id,
                    TicketID = t.TicketID,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status != null ? t.Status.Name : "Unknown",
                    Priority = t.Priority != null ? t.Priority.Name : "Normal",
                    AssignedTo = t.AssignedToUser != null
                        ? $"{t.AssignedToUser.FirstName} {t.AssignedToUser.LastName}"
                        : "Unassigned",
                    CreatedBy = t.CreatedByUser != null
                        ? $"{t.CreatedByUser.FirstName} {t.CreatedByUser.LastName}"
                        : "Unknown",
                    CreatedDate = t.CreatedDate,
                    DueDate = t.DueDate,
                    TaskTitle = t.Tasks != null ? t.Tasks.Title : null
                })
                .ToListAsync();

            return tickets;
        }
    }
}