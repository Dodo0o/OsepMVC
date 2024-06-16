using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OsepMVC.Models
{
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        // Foreign Key
        [ForeignKey("Event")]
        public int EventId { get; set; }
        public Event Event { get; set; }

        // Foreign Key
        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
