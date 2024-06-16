using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsepMVC.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;

namespace OsepMVC.Controllers
{
    public class VoteController : Controller
    {
        private readonly OsepDbContext _context;

        public VoteController(OsepDbContext context)
        {
            _context = context;
        }

        // POST: Vote/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int eventId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var existingVote = await _context.Votes.FirstOrDefaultAsync(v => v.EventId == eventId && v.UserId == int.Parse(userId));
            if (existingVote != null)
            {
                return RedirectToAction("Details", "Event", new { id = eventId });
            }

            var vote = new Vote
            {
                EventId = eventId,
                UserId = int.Parse(userId),
                VoteDate = DateTime.Now
            };

            _context.Add(vote);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Event", new { id = eventId });
        }

        // GET: Vote/Index
        public async Task<IActionResult> Index()
        {
            var votes = await _context.Votes.Include(v => v.Event).Include(v => v.User).ToListAsync();
            return View(votes);
        }
    }
}
