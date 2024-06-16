using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OsepMVC.Models;

namespace OsepMVC.Services
{
    public class EventResetService
    {
        private readonly OsepDbContext _context;

        public EventResetService(OsepDbContext context)
        {
            _context = context;
        }

        public async Task ResetEventsAsync()
        {
           
            var participations = await _context.EventParticipations.ToListAsync();
            _context.EventParticipations.RemoveRange(participations);

          
            var votes = await _context.Votes.ToListAsync();
            _context.Votes.RemoveRange(votes);

         
            var comments = await _context.Comments.ToListAsync();
            _context.Comments.RemoveRange(comments);

          
            var events = await _context.Events.ToListAsync();
            _context.Events.RemoveRange(events);

           
            await _context.SaveChangesAsync();
        }
    }
}
