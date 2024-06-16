using System;
using System.ComponentModel.DataAnnotations;

namespace OsepMVC.Models
{
    public class Vote
    {
        public int VoteId { get; set; }

        [Required]
        public int UserId { get; set; }
        public User User { get; set; }

        [Required]
        public int EventId { get; set; }
        public Event Event { get; set; }

        public DateTime VoteDate { get; set; }
    }
}
