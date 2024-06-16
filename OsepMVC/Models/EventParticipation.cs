using System;

namespace OsepMVC.Models
{
    public class EventParticipation
    {
        public int EventParticipationId { get; set; }
        public int EventId { get; set; }
        public Event Event { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public DateTime ParticipationDate { get; set; }
    }
}