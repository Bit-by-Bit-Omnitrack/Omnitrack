using UserRoles.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UserRoles.Services
{
    public interface ITicketService
    {
        Task<List<TicketSummaryDto>> GetTicketSummariesAsync();
    }
}