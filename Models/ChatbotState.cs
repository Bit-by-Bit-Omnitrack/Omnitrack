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
            CreatingTicket_AskTask
        }

        public BotAction CurrentAction { get; set; } = BotAction.None;
        public Ticket TempTicket { get; set; } // To store partial ticket details

        public ChatbotState()
        {
            TempTicket = new Ticket(); // Initialize TempTicket when state is created
        }

        public void Reset()
        {
            CurrentAction = BotAction.None;
            TempTicket = new Ticket();
        }
    }
}