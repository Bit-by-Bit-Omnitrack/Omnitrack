using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserRoles.Models
{
   
        public class ChatMessage
        {
            public string Sender { get; set; }
            public string Message { get; set; }
            public DateTime TimeSent { get; set; } = DateTime.Now;
        }
    }


