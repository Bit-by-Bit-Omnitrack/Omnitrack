namespace UserRoles.Models
{
    public class TaskItem
    {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }

            // Foreign key
            public int PriorityId { get; set; }
            public Priority Priority { get; set; }
        }
    }

