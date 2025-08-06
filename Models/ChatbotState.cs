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

            // New: Actions for the project assignment conversation
            AssigningProject_AskUser,
            AssigningProject_AskProject
        }

        public BotAction CurrentAction { get; set; } = BotAction.None;
        public Ticket TempTicket { get; set; } // To store partial ticket details

        // New: Properties to store temporary data for project assignment
        public string? TempProjectMember_UserId { get; set; }
        public int TempProjectMember_ProjectId { get; set; }

        public ChatbotState()
        {
            TempTicket = new Ticket(); // Initialize TempTicket when state is created
        }

        public void Reset()
        {
            CurrentAction = BotAction.None;
            TempTicket = new Ticket();

            // New: Reset the project assignment properties as well
            TempProjectMember_UserId = null;
            TempProjectMember_ProjectId = 0;
        }
    }
}