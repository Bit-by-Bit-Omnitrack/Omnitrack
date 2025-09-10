using Microsoft.AspNetCore.Mvc;
using UserRoles.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UserRoles.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace UserRoles.Controllers
{
    public class ChatbotController : Controller
    {
        private static List<ChatMessage> chatHistory = new();
        private static ChatbotState chatbotState = new();

        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public ChatbotController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View(chatHistory);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string message)
        {
            var userMsg = new ChatMessage
            {
                Sender = "You",
                Message = message
            };

            chatHistory.Add(userMsg);

            var botResponse = await GenerateBotResponse(message);

            chatHistory.Add(new ChatMessage
            {
                Sender = "OmniBot",
                Message = botResponse
            });

            return RedirectToAction("Index");
        }

        private async Task<string> GenerateBotResponse(string input)
        {
            input = input.ToLower().Trim();

            // --- Handle ongoing conversations ---
            if (chatbotState.CurrentAction != ChatbotState.BotAction.None)
            {
                // Check if the current conversation is for creating a ticket
                if (chatbotState.CurrentAction >= ChatbotState.BotAction.CreatingTicket_AskTitle &&
                    chatbotState.CurrentAction <= ChatbotState.BotAction.CreatingTicket_AskTask)
                {
                    return await HandleTicketCreationConversation(input);
                }
                // Check if the current conversation is for assigning a project
                else if (chatbotState.CurrentAction >= ChatbotState.BotAction.AssigningProject_AskUser &&
                        chatbotState.CurrentAction <= ChatbotState.BotAction.AssigningProject_AskProject)
                {
                    return await HandleProjectAssignmentConversation(input);
                }
                // Check if the current conversation is for creating a task
                else if (chatbotState.CurrentAction >= ChatbotState.BotAction.CreatingTask_AskName &&
                        chatbotState.CurrentAction <= ChatbotState.BotAction.CreatingTask_AskStatus)
                {
                    return await HandleTaskCreationConversation(input);
                }
                // New: Check if the current conversation is for counting tickets
                else if (chatbotState.CurrentAction == ChatbotState.BotAction.CountingTickets_AskUser)
                {
                    return await HandleTicketCountingConversation(input);
                }
            }

            // --- Initial commands ---
            if (input.Contains("hello") || input.Contains("hi"))
                return "Hello! How can I assist you today?";

            if (input.Contains("help"))
                return "Sure, I'm here to help. You can ask me to 'create ticket', 'assign project', or 'create task'.";

            if (input.Contains("view ticket"))
                return "You can view your open tickets on the dashboard.";

            if (input.Contains("create ticket"))
            {
                chatbotState.Reset();
                chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTicket_AskTitle;
                return "Okay, let's create a new ticket. What is the title of the ticket?";
            }

            if (input.Contains("tickets assigned to"))
            {
                chatbotState.Reset();
                chatbotState.CurrentAction = ChatbotState.BotAction.CountingTickets_AskUser;
                var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                var userList = string.Join(", ", users.Select(u => u.UserName));
                if (!users.Any()) return "No active users found. I can't count tickets right now.";
                return $"Whose assigned tickets do you want to count? Available users: {userList}";
            }

            if (input.Contains("assign project"))
            {
                chatbotState.Reset();
                chatbotState.CurrentAction = ChatbotState.BotAction.AssigningProject_AskUser;
                var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                var userList = string.Join(", ", users.Select(u => u.UserName));
                if (!users.Any()) return "No active users found. I can't assign a project right now.";

                return $"Okay, I can help with that. Who do you want to assign to a project? Available users: {userList}";
            }

            // New: Handle initial task creation command
            if (input.Contains("create task"))
            {
                chatbotState.Reset();
                chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTask_AskName;
                return "Okay, let's create a new task. What is the name of the task?";
            }

            return "Sorry, I didn’t understand that. Can you rephrase? You can ask me to 'create ticket', 'assign project', 'create task', or 'count tickets assigned to'.";
        }
        
       
        private async Task<string> HandleTicketCountingConversation(string input)
        {
            switch (chatbotState.CurrentAction)
            {
                case ChatbotState.BotAction.CountingTickets_AskUser:
                    var userName = input;
                    var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower());

                    if (user != null)
                    {
                        var assignedTicketsCount = await _context.Tickets
                            .Where(t => t.AssignedToUserId == user.Id)
                            .CountAsync();

                        chatbotState.Reset();
                        return $"'{user.UserName}' has {assignedTicketsCount} tickets assigned.";
                    }
                    else
                    {
                        var usersAgain = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                        var userListAgain = string.Join(", ", usersAgain.Select(u => u.UserName));
                        return $"User '{userName}' not found. Please try again. Available users: {userListAgain}";
                    }
                default:
                    chatbotState.Reset();
                    return "An error occurred during the ticket count process. Please type 'tickets assigned to' to start over.";
            }
        }



        private async Task<string> HandleTicketCreationConversation(string input)
        {
            switch (chatbotState.CurrentAction)
            {
                case ChatbotState.BotAction.CreatingTicket_AskTitle:
                    chatbotState.TempTicket.Title = input;
                    chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTicket_AskDescription;
                    return "Got it. Please provide a description for the ticket.";

                case ChatbotState.BotAction.CreatingTicket_AskDescription:
                    chatbotState.TempTicket.Description = input;
                    chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTicket_AskAssignedUser;
                    var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                    var userList = string.Join(", ", users.Select(u => u.UserName));
                    if (!users.Any()) return "No active users found to assign. Please assign later. What is the **due date** (YYYY-MM-DD)?";
                    return $"Who should this ticket be assigned to? (e.g., '{users.First().UserName}') Available users: {userList}";

                case ChatbotState.BotAction.CreatingTicket_AskAssignedUser:
                    var assignedUserName = input;
                    var assignedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == assignedUserName.ToLower());
                    if (assignedUser != null)
                    {
                        chatbotState.TempTicket.AssignedToUserId = assignedUser.Id;
                        chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTicket_AskDueDate;
                        return "Ticket assigned. What is the due date for this ticket? (Please use YYYY-MM-DD format)";
                    }
                    else
                    {
                        var usersAgain = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                        var userListAgain = string.Join(", ", usersAgain.Select(u => u.UserName));
                        return $"User '{assignedUserName}' not found. Please try again or type 'skip' to assign later. Available users: {userListAgain}";
                    }
                case ChatbotState.BotAction.CreatingTicket_AskDueDate:
                    if (DateTime.TryParse(input, out DateTime dueDate))
                    {
                        chatbotState.TempTicket.DueDate = dueDate;
                        chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTicket_AskPriority;
                        var priorities = await _context.Priorities.ToListAsync();
                        var priorityList = string.Join(", ", priorities.Select(p => p.Name));
                        if (!priorities.Any()) return "No priorities found. What is the **task** this ticket belongs to?";
                        return $"What is the priority of this ticket? (e.g., '{priorities.First().Name}') Available priorities: {priorityList}";
                    }
                    else
                    {
                        return "Invalid date format. Please use YYYY-MM-DD. What is the due date?";
                    }
                case ChatbotState.BotAction.CreatingTicket_AskPriority:
                    var priorityName = input;
                    var priority = await _context.Priorities.FirstOrDefaultAsync(p => p.Name.ToLower() == priorityName.ToLower());
                    if (priority != null)
                    {
                        chatbotState.TempTicket.PriorityId = priority.Id;
                        chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTicket_AskTask;
                        var tasks = await _context.Tasks.ToListAsync();
                        var taskList = string.Join(", ", tasks.Select(t => t.Name));
                        if (!tasks.Any()) return "No tasks found. Creating ticket now without a specific task. Say 'confirm' to create.";
                        return $"Which task does this ticket belong to? (e.g., '{tasks.First().Name}') Available tasks: {taskList}";
                    }
                    else
                    {
                        var prioritiesAgain = await _context.Priorities.ToListAsync();
                        var priorityListAgain = string.Join(", ", prioritiesAgain.Select(p => p.Name));
                        return $"Priority '{priorityName}' not found. Please try again. Available priorities: {priorityListAgain}";
                    }

                case ChatbotState.BotAction.CreatingTicket_AskTask:
                    var taskName = input;
                    var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Name.ToLower() == taskName.ToLower());

                    if (task != null)
                    {
                        chatbotState.TempTicket.TasksId = task.Id;
                    }
                    else
                    {
                        if (input != "skip")
                        {
                            var tasksAgain = await _context.Tasks.ToListAsync();
                            var taskListAgain = string.Join(", ", tasksAgain.Select(t => t.Name));
                            return $"Task '{taskName}' not found. Please try again or type 'skip' to not assign a task. Available tasks: {taskListAgain}";
                        }
                    }
                    return await CreateTicketFromChatbot();
                default:
                    return "Something went wrong in the ticket creation process. Please try again by typing 'create ticket'.";
            }
        }

     

        private async Task<string> HandleProjectAssignmentConversation(string input)
        {
            switch (chatbotState.CurrentAction)
            {
                case ChatbotState.BotAction.AssigningProject_AskUser:
                    var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == input.ToLower());
                    if (user != null)
                    {
                        chatbotState.TempProjectMember_UserId = user.Id;
                        chatbotState.CurrentAction = ChatbotState.BotAction.AssigningProject_AskProject;
                        var projects = await _context.Projects.ToListAsync();
                        var projectList = string.Join(", ", projects.Select(p => p.ProjectName));
                        if (!projects.Any()) return "No projects found. I can't assign a project right now.";
                        return $"Got it. Now, which project should '{user.UserName}' be assigned to? Available projects: {projectList}";
                    }
                    else
                    {
                        var usersAgain = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                        var userListAgain = string.Join(", ", usersAgain.Select(u => u.UserName));
                        return $"User '{input}' not found. Please try again. Available users: {userListAgain}";
                    }

                case ChatbotState.BotAction.AssigningProject_AskProject:
                    var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectName.ToLower() == input.ToLower());
                    if (project != null)
                    {
                        chatbotState.TempProjectMember_ProjectId = project.ProjectId;
                        var existingAssignment = await _context.ProjectMembers
                            .AnyAsync(pm => pm.UserId == chatbotState.TempProjectMember_UserId && pm.ProjectId == chatbotState.TempProjectMember_ProjectId);
                        if (existingAssignment)
                        {
                            chatbotState.Reset();
                            return "This user is already a member of this project. Project assignment cancelled.";
                        }
                        var projectMember = new ProjectMember
                        {
                            UserId = chatbotState.TempProjectMember_UserId,
                            ProjectId = chatbotState.TempProjectMember_ProjectId,
                            JoinedDate = DateTime.UtcNow
                        };
                        try
                        {
                            _context.ProjectMembers.Add(projectMember);
                            await _context.SaveChangesAsync();
                            chatbotState.Reset();
                            return "Project assignment successful!";
                        }
                        catch (Exception)
                        {
                            chatbotState.Reset();
                            return "There was an error assigning the project. Please try again.";
                        }
                    }
                    else
                    {
                        var projectsAgain = await _context.Projects.ToListAsync();
                        var projectListAgain = string.Join(", ", projectsAgain.Select(p => p.ProjectName));
                        return $"Project '{input}' not found. Please try again. Available projects: {projectListAgain}";
                    }

                default:
                    return "An error occurred during project assignment. Please type 'assign project' to start over.";
            }
        }

      

        private async Task<string> HandleTaskCreationConversation(string input)
        {
            switch (chatbotState.CurrentAction)
            {
                case ChatbotState.BotAction.CreatingTask_AskName:
                    chatbotState.TempTask.Name = input;
                    chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTask_AskDetails;
                    return "What are the details of the task?";

                case ChatbotState.BotAction.CreatingTask_AskDetails:
                    chatbotState.TempTask.Details = input;
                    chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTask_AskAssignedUser;
                    var users = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                    var userList = string.Join(", ", users.Select(u => u.UserName));
                    if (!users.Any()) return "No active users found. What is the due date for the task? (YYYY-MM-DD)";
                    return $"Who should this task be assigned to? (e.g., '{users.First().UserName}') Available users: {userList}";

                case ChatbotState.BotAction.CreatingTask_AskAssignedUser:
                    var assignedUserName = input;
                    var assignedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == assignedUserName.ToLower());
                    if (assignedUser != null)
                    {
                        chatbotState.TempTask.AssignedToUserId = assignedUser.Id;
                        chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTask_AskDueDate;
                        return "Task assigned. What is the due date for this task? (YYYY-MM-DD)";
                    }
                    else
                    {
                        var usersAgain = await _userManager.Users.Where(u => u.IsActive).ToListAsync();
                        var userListAgain = string.Join(", ", usersAgain.Select(u => u.UserName));
                        return $"User '{assignedUserName}' not found. Please try again or type 'skip' to assign later. Available users: {userListAgain}";
                    }

                case ChatbotState.BotAction.CreatingTask_AskDueDate:
                    if (DateTime.TryParse(input, out DateTime dueDate))
                    {
                        chatbotState.TempTask.DueDate = dueDate;
                        chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTask_AskProject;
                        var projects = await _context.Projects.ToListAsync();
                        var projectList = string.Join(", ", projects.Select(p => p.ProjectName));
                        if (!projects.Any()) return "No projects found. Creating task now. Say 'confirm' to create.";
                        return $"Which project does this task belong to? (e.g., '{projects.First().ProjectName}') Available projects: {projectList}";
                    }
                    else
                    {
                        return "Invalid date format. Please use YYYY-MM-DD. What is the due date?";
                    }

                case ChatbotState.BotAction.CreatingTask_AskProject:
                    var projectName = input;
                    var project = await _context.Projects.FirstOrDefaultAsync(p => p.ProjectName.ToLower() == projectName.ToLower());
                    if (project != null)
                    {
                        chatbotState.TempTask.ProjectId = project.ProjectId;
                    }
                    else
                    {
                        if (input != "skip")
                        {
                            var projectsAgain = await _context.Projects.ToListAsync();
                            var projectListAgain = string.Join(", ", projectsAgain.Select(p => p.ProjectName));
                            return $"Project '{projectName}' not found. Please try again or type 'skip' to not assign a project. Available projects: {projectListAgain}";
                        }
                    }
                    chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTask_AskStatus;
                    var statuses = await _context.TaskStatuses.ToListAsync();
                    var statusList = string.Join(", ", statuses.Select(s => s.StatusName));
                    if (!statuses.Any()) return await CreateTaskFromChatbot();
                    return $"What is the initial status of this task? (e.g., '{statuses.First().StatusName}') Available statuses: {statusList}";

                case ChatbotState.BotAction.CreatingTask_AskStatus:
                    var statusName = input;
                    var status = await _context.TaskStatuses.FirstOrDefaultAsync(s => s.StatusName.ToLower() == statusName.ToLower());
                    if (status != null)
                    {
                        chatbotState.TempTask.StatusID = status.Id;
                    }
                    else
                    {
                        if (input != "skip")
                        {
                            var statusesAgain = await _context.TaskStatuses.ToListAsync();
                            var statusListAgain = string.Join(", ", statusesAgain.Select(s => s.StatusName));
                            return $"Status '{statusName}' not found. Please try again or type 'skip' to use the default status. Available statuses: {statusListAgain}";
                        }
                    }
                    return await CreateTaskFromChatbot();

                default:
                    return "Something went wrong in the task creation process. Please try again by typing 'create task'.";
            }
        }

     

        private async Task<string> CreateTaskFromChatbot()
        {
            var newTask = chatbotState.TempTask;
            // newTask.CreatedDate = DateTime.UtcNow;

            // Set a default status if none was provided
            if (newTask.StatusID == 0)
            {
                var defaultStatus = await _context.TaskStatuses.FirstOrDefaultAsync(s => s.StatusName.ToLower() == "to do");
                newTask.StatusID = defaultStatus?.Id ?? 1; // Default to 'To Do' or 1 if not found
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                newTask.CreatedById = currentUser.Id;
            }
            else
            {
                newTask.CreatedById = "ChatbotUser";
            }

            try
            {
                _context.Add(newTask);
                await _context.SaveChangesAsync();
                chatbotState.Reset();
                return $"Task '{newTask.Name}' has been successfully created.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating task from chatbot: {ex.Message}");
                chatbotState.Reset();
                return "I encountered an error while trying to create the task. Please try again later or contact support.";
            }
        }



        private async Task<string> CreateTicketFromChatbot()
        {
            var newTicket = chatbotState.TempTicket;
            newTicket.TicketID = $"TCK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            newTicket.TaskID = $"TSK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            newTicket.StatusID = 1;
            newTicket.CreatedDate = DateTime.UtcNow;

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                newTicket.CreatedByID = currentUser.Id;
            }
            else
            {
                newTicket.CreatedByID = "ChatbotUser";
            }

            try
            {
                _context.Add(newTicket);
                await _context.SaveChangesAsync();
                chatbotState.Reset();
                return $"Ticket '{newTicket.Title}' has been successfully created with ID: {newTicket.TicketID}.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating ticket from chatbot: {ex.Message}");
                chatbotState.Reset();
                return "I encountered an error while trying to create the ticket. Please try again later or contact support.";
            }
        }
    }
}