using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using UserRoles.Models;
using UserRoles.ViewModels;
using Microsoft.AspNetCore.Authorization;
using UserRoles.Data;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using System.Drawing;
using Syncfusion.Pdf.Tables;
using Syncfusion.Drawing; // Add this namespace for RectangleF and PointF

namespace UserRoles.Controllers
{
    public class TicketsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public TicketsController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Tickets1
        public async Task<IActionResult> Index(int? filterTaskId, int? filterProjectId) // Added filterTaskId parameter
        {
            // Start with all tickets, eager load related data
            var ticketsQuery = _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Tasks)
                .Include(t => t.Project) // Include Project navigation property
                .AsQueryable(); // Use AsQueryable to allow further filtering

            // Apply task filter if filterTaskId is provided
            if (filterTaskId.HasValue && filterTaskId.Value > 0)
            {
                ticketsQuery = ticketsQuery.Where(t => t.TasksId == filterTaskId.Value);
            }

            // Apply project filter if filterProjectId is provided
            if (filterProjectId.HasValue && filterProjectId.Value > 0)
            {
                ticketsQuery = ticketsQuery.Where(t => t.ProjectId == filterProjectId.Value);
            }

            // Exclude tickets with StatusID 5 (Completed) from the main dashboard view
            ticketsQuery = ticketsQuery.Where(t => t.StatusID != 5);

            var tickets = await ticketsQuery.ToListAsync();

            ViewBag.Statuses = await _context.TicketStatuses.OrderBy(s => s.Id).ToListAsync();

            // Populate ViewBag for tasks dropdown in the filter
            ViewBag.TasksFilter = new SelectList(await _context.Tasks.ToListAsync(), "Id", "Name", filterTaskId);

            // Populate ViewBag for projects dropdown in the filter
            ViewBag.ProjectsFilter = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName", filterProjectId);

            return View(tickets);
        }

        // GET: Tickets/History
        public async Task<IActionResult> History(int? statusId)
        {
            var completedTicketsQuery = _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Tasks)
                .Include(t => t.Project)
                .Where(t => t.StatusID == 5) // Assuming StatusID 5 is "Completed" or "Archived"
                .AsQueryable();

            // Apply filter if a statusId is provided
            if (statusId.HasValue)
            {
                completedTicketsQuery = completedTicketsQuery.Where(t => t.StatusID == statusId.Value);
            }

            var completedTickets = await completedTicketsQuery
                .OrderByDescending(t => t.UpdatedDate)
                .ToListAsync();

            return View(completedTickets);
        }

        // GET: Tickets1/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser) // Include CreatedByUser for details
                .Include(t => t.Status)
                .Include(t => t.Priority)// If you add a navigation property for status
                .Include(t => t.Tasks)
                .Include(t => t.Project) // Include Project navigation property
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        [Authorize(Roles = "Project Leader, System Administrator")]
        public async Task<IActionResult> Create()
        {
            // Populate ViewBag.Users for the dropdown in the Create view
            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName");
            ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name");
            ViewBag.Tasks = new SelectList(await _context.Tasks.ToListAsync(), "Id", "Name");
            ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName");
            return View();
        }

        // POST: Tickets1/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ticket ticket)
        {
            ticket.TicketID = $"TCK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            ticket.TaskID = $"TSK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            ticket.StatusID = 1;
            ticket.CreatedDate = DateTime.UtcNow;
            var currentUser = await _userManager.GetUserAsync(User);
            ticket.CreatedByID = currentUser?.Id ?? "System";

            ModelState.Remove("TicketID");
            ModelState.Remove("TaskID");
            ModelState.Remove("StatusID");
            ModelState.Remove("CreatedByID");
            ModelState.Remove("CreatedDate");
            ModelState.Remove("Priority");

            if (ModelState.IsValid)
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", ticket.AssignedToUserId);
            ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name", ticket.PriorityId);
            ViewBag.Tasks = new SelectList(await _context.Tasks.ToListAsync(), "Id", "Name", ticket.TasksId);
            ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName", ticket.ProjectId);

            return View(ticket);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateTicketStatus(int ticketId, int newStatusId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null)
            {
                return NotFound();
            }

            // Check if the newStatusId is valid (exists in your TicketStatuses table)
            var newStatus = await _context.TicketStatuses.FindAsync(newStatusId);
            if (newStatus == null)
            {
                return BadRequest("Invalid Status ID.");
            }

            ticket.StatusID = newStatusId;

            // Optional: Update metadata
            var currentUser = await _userManager.GetUserAsync(User);
            ticket.UpdatedBy = currentUser?.Id ?? "System";
            ticket.UpdatedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Ticket status updated successfully." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("Failed to update ticket status due to a concurrency issue.");
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using ILogger)
                Console.WriteLine($"Error updating ticket status: {ex.Message}");
                return StatusCode(500, "An error occurred while updating the ticket status.");
            }
        }

        // New action to archive a ticket by setting its status to 'Completed' (StatusID = 5)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveTicket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            // Assuming StatusID 5 is "Completed" or "Archived"
            ticket.StatusID = 5;

            // Update metadata
            var currentUser = await _userManager.GetUserAsync(User);
            ticket.UpdatedBy = currentUser?.Id ?? "System";
            ticket.UpdatedDate = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Ticket archived successfully." });
            }
            catch (DbUpdateConcurrencyException)
            {
                return BadRequest("Failed to archive ticket due to a concurrency issue.");
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error archiving ticket: {ex.Message}");
                return StatusCode(500, "An error occurred while archiving the ticket.");
            }
        }

        // GET: Tickets1/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }

            ViewBag.Users = new SelectList(await _userManager.Users
                .Where(u => u.IsActive).ToListAsync(), "Id", "UserName", ticket.AssignedToUserId);

            ViewBag.Statuses = new SelectList(await _context.TicketStatuses
                .ToListAsync(), "Id", "StatusName", ticket.StatusID);
            ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name", ticket.PriorityId);
            ViewBag.Tasks = new SelectList(await _context.Tasks.ToListAsync(), "Id", "Name", ticket.TasksId);
            ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName", ticket.ProjectId);

            return View(ticket);
        }

        // POST: Tickets1/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,AssignedToUserId,DueDate,StatusID,PriorityId,TasksId,ProjectId")] Ticket ticket)
        {
            if (id != ticket.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(await _userManager.Users.Where(u => u.IsActive).ToListAsync(), "Id", "UserName", ticket.AssignedToUserId);
                ViewBag.Statuses = new SelectList(await _context.TicketStatuses.ToListAsync(), "Id", "StatusName", ticket.StatusID);
                ViewBag.Priorities = new SelectList(await _context.Priorities.ToListAsync(), "Id", "Name", ticket.PriorityId);
                ViewBag.Tasks = new SelectList(await _context.Tasks.ToListAsync(), "Id", "Name", ticket.TasksId); // Re-populate with selected TaskId
                ViewBag.Projects = new SelectList(await _context.Projects.ToListAsync(), "ProjectId", "ProjectName", ticket.ProjectId);
            }

            var existingTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            if (existingTicket == null)
                return NotFound();

            try
            {
                // Only update fields the user is allowed to change
                existingTicket.Title = ticket.Title;
                existingTicket.Description = ticket.Description;
                existingTicket.AssignedToUserId = ticket.AssignedToUserId;
                existingTicket.DueDate = ticket.DueDate;
                existingTicket.StatusID = ticket.StatusID;
                existingTicket.PriorityId = ticket.PriorityId; // Added this line to update priority
                existingTicket.TasksId = ticket.TasksId; // Added this line to update tasks
                existingTicket.ProjectId = ticket.ProjectId; // Added this line to update project

                // Update metadata
                var currentUser = await _userManager.GetUserAsync(User);
                existingTicket.UpdatedBy = currentUser?.Id ?? "System";
                existingTicket.UpdatedDate = DateTime.UtcNow;


                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TicketExists(ticket.Id))
                    return NotFound();
                else
                    throw;
            }
        }


        // GET: Tickets1/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ticket = await _context.Tickets
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }
        // POST: Tickets1/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                _context.Tickets.Remove(ticket);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Tickets/ChartData
        public async Task<IActionResult> ChartData()
        {

            var allTickets = await _context.Tickets
                .Include(t => t.CreatedByUser)
                .Include(t => t.AssignedToUser)
                .ToListAsync();


            var createdTickets = allTickets
                .GroupBy(t => t.CreatedByUser?.UserName ?? "Unassigned")
                .Select(g => new { User = g.Key, Count = g.Count() })
                .ToDictionary(x => x.User, x => x.Count);


            var assignedTickets = allTickets
                .Where(t => t.AssignedToUser != null)
                .GroupBy(t => t.AssignedToUser.UserName)
                .Select(g => new { User = g.Key, Count = g.Count() })
                .ToDictionary(x => x.User, x => x.Count);


            var chartData = new
            {
                Created = createdTickets,
                Assigned = assignedTickets
            };

            return Json(chartData);
        }

        // Action to get data for the ticket status chart
        [HttpGet]
        public async Task<IActionResult> StatusChartData()
        {
            var statusCounts = await _context.Tickets
                .Include(t => t.Status)
                .GroupBy(t => t.Status.StatusName)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count);

            return Json(statusCounts);
        }

        // New action to get data for the report
        [HttpGet]
        public async Task<IActionResult> GetReportData()
        {
            var tickets = await _context.Tickets
                .Include(t => t.AssignedToUser)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Tasks)
                .Select(t => new
                {
                    t.Title,
                    t.Description,
                    DueDate = t.DueDate.HasValue ? t.DueDate.Value.ToString("yyyy-MM-dd") : string.Empty, // Format date for easy use
                    Priority = t.Priority.Name,
                    AssignedTo = t.AssignedToUser.UserName,
                    Status = t.Status.StatusName,
                    Task = t.Tasks.Name
                })
                .ToListAsync();

            return Json(tickets);
        }

        // GET: Tickets/Charts
        public IActionResult Charts()
        {
            return View();
        }

        /// <summary>
        /// Downloads a comprehensive report of all tasks and tickets as a CSV file.
        /// Includes user roles and project details.
        /// </summary>
        /// <returns>A CSV file with the report data.</returns>
        public async Task<IActionResult> DownloadCsvReport()
        {
            var reportData = await GetReportDataAsync();
            var csvBuilder = new System.Text.StringBuilder();

            // Add header row
            csvBuilder.AppendLine("Project Name,Task Name,Task Details,Task Status,Task Due Date,Assigned To (Task),Assigned To Email (Task),Created By (Task),Ticket Title,Ticket Description,Ticket Status,Ticket Priority,Ticket Due Date,Assigned To (Ticket),Created By (Ticket)");

            // Add data rows
            foreach (var row in reportData)
            {
                csvBuilder.AppendLine(
                    $"{EscapeCsv(row.ProjectName)}," +
                    $"{EscapeCsv(row.TaskName)}," +
                    $"{EscapeCsv(row.TaskDetails)}," +
                    $"{EscapeCsv(row.TaskStatus)}," +
                    $"{row.TaskDueDate?.ToString("yyyy-MM-dd") ?? ""}," +
                    $"{EscapeCsv(row.AssignedToUser)}," +
                    $"{EscapeCsv(row.AssignedToUserEmail)}," +
                    $"{EscapeCsv(row.CreatedByUser)}," +
                    $"{EscapeCsv(row.TicketTitle)}," +
                    $"{EscapeCsv(row.TicketDescription)}," +
                    $"{EscapeCsv(row.TicketStatus)}," +
                    $"{EscapeCsv(row.TicketPriority)}," +
                    $"{row.TicketDueDate?.ToString("yyyy-MM-dd") ?? ""}," +
                    $"{EscapeCsv(row.AssignedToUserTicket)}," +
                    $"{EscapeCsv(row.CreatedByTicket)}"
                );
            }
            return File(System.Text.Encoding.UTF8.GetBytes(csvBuilder.ToString()), "text/csv", $"Omnitrack_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        /// <summary>
        /// Downloads a comprehensive report of all tasks and tickets as a Word document.
        /// </summary>
        [Authorize(Roles = "System Administrator, Project Leader")]
        public async Task<IActionResult> DownloadDocReport()
        {
            var reportData = await GetReportDataAsync();
            var stream = new MemoryStream(); // Create the stream outside the using block

            // Create a new Word document
            using (WordDocument document = new WordDocument())
            {
                // Add a section and a table to the document
                IWSection section = document.AddSection();
                WTable table = (WTable)section.AddTable(); 
                table.ResetCells(reportData.Count + 1, 8); 

                // Add table header
                string[] headers = { "Project Name", "Ticket Title", "Ticket Description", "Ticket Status", "Ticket Priority", "Ticket Due Date", "Assigned To (Ticket)", "Created By (Ticket)" };
                for (int i = 0; i < headers.Length; i++)
                {
                    table[0, i].AddParagraph().AppendText(headers[i]).CharacterFormat.Bold = true;
                }

                // Add data rows
                for (int i = 0; i < reportData.Count; i++)
                {
                    var rowData = reportData[i];
                    table[i + 1, 0].AddParagraph().AppendText(rowData.ProjectName);
                    table[i + 1, 1].AddParagraph().AppendText(rowData.TicketTitle);
                    table[i + 1, 2].AddParagraph().AppendText(rowData.TicketDescription);
                    table[i + 1, 3].AddParagraph().AppendText(rowData.TicketStatus);
                    table[i + 1, 4].AddParagraph().AppendText(rowData.TicketPriority);
                    table[i + 1, 5].AddParagraph().AppendText(rowData.TicketDueDate?.ToString("yyyy-MM-dd"));
                    table[i + 1, 6].AddParagraph().AppendText(rowData.AssignedToUserTicket);
                    table[i + 1, 7].AddParagraph().AppendText(rowData.CreatedByTicket);
                }

                IWSection section1= document.AddSection();
                WTable table1= (WTable)section1.AddTable();
               table1.ResetCells(reportData.Count + 1, 7);

                string[] headers2 = { "Task Name", "Task Details", "Task Status", "Task Due Date", "Assigned To (Task)", "Assigned To Email (Task)", "Created By (Task)" };
                for (int i = 0; i < headers2.Length; i++)
                {
                   table1[0, i].AddParagraph().AppendText(headers2[i]).CharacterFormat.Bold = true;
                }

                // Add data rows
                for (int i = 0; i < reportData.Count; i++)
                {
                    var rowData = reportData[i];
                    
                   table1[i + 1, 0].AddParagraph().AppendText(rowData.TaskName);
                   table1[i + 1, 1].AddParagraph().AppendText(rowData.TaskDetails);
                   table1[i + 1, 2].AddParagraph().AppendText(rowData.TaskStatus);
                   table1[i + 1, 3].AddParagraph().AppendText(rowData.TaskDueDate?.ToString("yyyy-MM-dd"));
                   table1[i + 1, 4].AddParagraph().AppendText(rowData.AssignedToUser);
                   table1[i + 1, 5].AddParagraph().AppendText(rowData.AssignedToUserEmail);
                   table1[i + 1, 6].AddParagraph().AppendText(rowData.CreatedByUser);
                }

                // Save the document to the stream
                document.Save(stream, FormatType.Docx);
            }
            // Return the stream. The framework will handle its disposal.
            stream.Position = 0;
            return File(stream, "application/msword", $"Omnitrack_Report_{DateTime.Now:yyyyMMdd_HHmmss}.docx");
        }

        /// <summary>
        /// Downloads a comprehensive report of all tasks and tickets as a PDF file.
        /// </summary>
        [Authorize(Roles = "System Administrator, Project Leader")]
        public async Task<IActionResult> DownloadPdfReport()
        {
            var reportData = await GetReportDataAsync();
            var stream = new MemoryStream(); // Create the stream outside the using block

            // Create a new PDF document
            using (PdfDocument document = new PdfDocument())
            {
                // Add a page
                PdfPage page = document.Pages.Add();

                // Create a table and add data
                PdfLightTable pdfTable = new PdfLightTable();
                pdfTable.DataSource = reportData.Select(r => new
                {
                    r.ProjectName,
                    r.TaskName,
                    TaskDetails = r.TaskDetails?.Substring(0, Math.Min(r.TaskDetails.Length, 30)) + "...",
                    r.TaskStatus,
                    TaskDueDate = r.TaskDueDate?.ToString("yyyy-MM-dd"),
                    AssignedToUser = r.AssignedToUser,
                    CreatedByUser = r.CreatedByUser,
                    r.TicketTitle,
                    TicketDescription = r.TicketDescription?.Substring(0, Math.Min(r.TicketDescription.Length, 30)) + "...",
                    r.TicketStatus,
                    r.TicketPriority,
                    TicketDueDate = r.TicketDueDate?.ToString("yyyy-MM-dd"),
                    AssignedToUserTicket = r.AssignedToUserTicket,
                    CreatedByTicket = r.CreatedByTicket
                }).ToList();

            
                Syncfusion.Drawing.RectangleF bounds = new Syncfusion.Drawing.RectangleF(0, 0, page.Graphics.ClientSize.Width, page.Graphics.ClientSize.Height);
                pdfTable.Draw(page, bounds);

                // Save the document to the stream
                document.Save(stream);
            }
            // Return the stream. The framework will handle its disposal.
            stream.Position = 0;
            return File(stream, "application/pdf", $"Omnitrack_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        // Helper method to fetch the data
        private async Task<List<ReportViewModel>> GetReportDataAsync()
        {
            var reportData = await _context.Tasks
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .Include(t => t.Project)
                .Include(t => t.Status)
                .GroupJoin(
                    _context.Tickets.Include(ti => ti.AssignedToUser).Include(ti => ti.CreatedByUser).Include(ti => ti.Status).Include(ti => ti.Priority).Include(ti => ti.Project),
                    task => task.Id,
                    ticket => ticket.TasksId,
                    (task, tickets) => new { task, tickets = tickets.DefaultIfEmpty() }
                )
                .SelectMany(
                    x => x.tickets,
                    (x, ticket) => new
                    {
                        Task = x.task,
                        Ticket = ticket,
                    }
                )
                .ToListAsync();

            var reportRows = new List<ReportViewModel>();
            foreach (var item in reportData)
            {
                reportRows.Add(new ReportViewModel
                {
                    ProjectName = item.Task?.Project?.ProjectName ?? item.Ticket?.Project?.ProjectName,
                    TaskName = item.Task?.Name,
                    TaskDetails = item.Task?.Details,
                    TaskStatus = item.Task?.Status?.StatusName,
                    TaskDueDate = item.Task?.DueDate,
                    AssignedToUser = item.Task?.AssignedToUser?.UserName,
                    AssignedToUserEmail = item.Task?.AssignedToUser?.Email,
                    CreatedByUser = item.Task?.CreatedByUser?.UserName,
                    CreatedByUserEmail = item.Task?.CreatedByUser?.Email,

                    TicketTitle = item.Ticket?.Title,
                    TicketDescription = item.Ticket?.Description,
                    TicketStatus = item.Ticket?.Status?.StatusName,
                    TicketPriority = item.Ticket?.Priority?.Name,
                    TicketDueDate = item.Ticket?.DueDate,
                    AssignedToUserTicket = item.Ticket?.AssignedToUser?.UserName,

                    CreatedByTicket = item.Ticket?.CreatedByUser?.UserName,

                });
            }
            return reportRows;
        }
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            if (value.Contains(",") || value.Contains("\"") || value.Contains(System.Environment.NewLine))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }
            return value;
        }
        private bool TicketExists(int id)
        {
            return _context.Tickets.Any(e => e.Id == id);
        }
    }
}