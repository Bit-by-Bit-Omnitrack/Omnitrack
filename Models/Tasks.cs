using System;
using System.ComponentModel.DataAnnotations;

namespace UserRoles.Models
{
    public class Tasks
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string AssignedTo { get; set; }


        public string CreatedBy { get; set; }


        public string Details { get; set; }


        public DateTime DueDate { get; set; }


        // public TaskStatus Status { get; set; } // Enum to track task phase
    }

    /*  public enum TaskStatus
      {
          Pending,
          InProgress,
          Complete,
          Blocked
      } */
}