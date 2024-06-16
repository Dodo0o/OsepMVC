using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OsepMVC.Models
{
    public class Event
    {
        public Event()
        {
            Votes = new HashSet<Vote>();
        }

        public int EventId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public string Type { get; set; } 

        public int UserId { get; set; }
        public User User { get; set; }

        public string ImagePath { get; set; } 
        public decimal? Price { get; set; }
        public string Address { get; set; }

        public ICollection<Vote> Votes { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<EventParticipation> Participations { get; set; }

    }
}
