namespace UserRoles.Models
{
    public class ChatbotState
    {
        public enum BotAction
        {
            None,
            CreatingTicket_AskTitle,
            CreatingTicket_AskDescription,
            CreatingTicket_AskAssignedUser,
            CreatingTicket_AskDueDate,
            CreatingTicket_AskPriority,
            CreatingTicket_AskTask,

            AssigningProject_AskUser,
            AssigningProject_AskProject,

            // New States for Task Creation
            CreatingTask_AskName,
            CreatingTask_AskDetails,
            CreatingTask_AskAssignedUser,
            CreatingTask_AskDueDate,
            CreatingTask_AskProject,
            CreatingTask_AskStatus,

            CountingTickets_AskUser
        }

        public BotAction CurrentAction { get; set; } = BotAction.None;
        public Ticket TempTicket { get; set; } = new Ticket();
        public string? TempProjectMember_UserId { get; set; }
        public int TempProjectMember_ProjectId { get; set; }

        // New: Property to store partial task details
        public Tasks TempTask { get; set; } = new Tasks();

        public void Reset()
        {
            CurrentAction = BotAction.None;
            TempTicket = new Ticket();
            TempProjectMember_UserId = null;
            TempProjectMember_ProjectId = 0;
            TempTask = new Tasks(); // Reset the new temporary task object
        }
    }
}