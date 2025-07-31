using Microsoft.AspNetCore.Mvc;
using UserRoles.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UserRoles.Data; // Ensure this is correctly referenced for AppDbContext
using Microsoft.EntityFrameworkCore; // For ToListAsync, FirstOrDefaultAsync etc.

namespace UserRoles.Controllers
{
    public class ChatbotController : Controller
    {
        private static List<ChatMessage> chatHistory = new();
        private static ChatbotState chatbotState = new(); // Static for simplicity in this example
                                                          // In a production app, manage state per user (e.g., using session, database)

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
        public async Task<IActionResult> SendMessage(string message) // Made async
        {
            var userMsg = new ChatMessage
            {
                Sender = "You",
                Message = message
            };

            chatHistory.Add(userMsg);

            var botResponse = await GenerateBotResponse(message); // Await the async method

            chatHistory.Add(new ChatMessage
            {
                Sender = "OmniBot",
                Message = botResponse
            });

            return RedirectToAction("Index");
        }

        private async Task<string> GenerateBotResponse(string input) // Made async
        {
            input = input.ToLower().Trim();

            // --- Handle ongoing ticket creation conversation ---
            if (chatbotState.CurrentAction != ChatbotState.BotAction.None)
            {
                return await HandleTicketCreationConversation(input);
            }

            // --- Initial commands ---
            if (input.Contains("hello") || input.Contains("hi"))
                return "Hello! How can I assist you today?";

            if (input.Contains("help"))
                return "Sure, I'm here to help. You can ask me to 'create ticket' or 'view tickets'.";

            if (input.Contains("view ticket"))
                return "You can view your open tickets on the dashboard.";

            if (input.Contains("create ticket"))
            {
                chatbotState.Reset(); // Reset any previous state
                chatbotState.CurrentAction = ChatbotState.BotAction.CreatingTicket_AskTitle;
                return "Okay, let's create a new ticket. What is the title of the ticket?";
            }

            return "Sorry, I didn’t understand that. Can you rephrase? You can ask me to 'create ticket' or 'view tickets'.";
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
                        // Allow skipping task assignment if user enters "skip" or an invalid task name
                        if (input != "skip")
                        {
                            var tasksAgain = await _context.Tasks.ToListAsync();
                            var taskListAgain = string.Join(", ", tasksAgain.Select(t => t.Name));
                            return $"Task '{taskName}' not found. Please try again or type 'skip' to not assign a task. Available tasks: {taskListAgain}";
                        }
                    }

                    // All details gathered, attempt to create the ticket
                    return await CreateTicketFromChatbot();

                default:
                    return "Something went wrong in the ticket creation process. Please try again by typing 'create ticket'.";
            }
        }

        private async Task<string> CreateTicketFromChatbot()
        {
            var newTicket = chatbotState.TempTicket;

            // Set auto-generated and default values
            newTicket.TicketID = $"TCK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
            newTicket.TaskID = $"TSK-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}"; // Assuming TaskID is for the ticket itself, not the related Task entity
            newTicket.StatusID = 1; // Default status: 1 = To Do
            newTicket.CreatedDate = DateTime.UtcNow;

            var currentUser = await _userManager.GetUserAsync(User); // Get the current logged-in user
            if (currentUser != null)
            {
                newTicket.CreatedByID = currentUser.Id;
            }
            else
            {
                newTicket.CreatedByID = "ChatbotUser"; // Fallback if no user is logged in (unlikely in this context)
            }

            try
            {
                _context.Add(newTicket);
                await _context.SaveChangesAsync();

                chatbotState.Reset(); // Reset chatbot state after successful creation
                return $"Ticket '{newTicket.Title}' has been successfully created with ID: {newTicket.TicketID}.";
            }
            catch (Exception ex)
            {
                // Log the exception (e.g., using ILogger)
                Console.WriteLine($"Error creating ticket from chatbot: {ex.Message}");
                chatbotState.Reset(); // Reset state on error
                return "I encountered an error while trying to create the ticket. Please try again later or contact support.";
            }
        }
    }
}