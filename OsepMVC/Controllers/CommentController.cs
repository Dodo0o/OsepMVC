using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsepMVC.Models;
using System.Linq;
using System.Threading.Tasks;

namespace OsepMVC.Controllers
{
    public class CommentController : Controller
    {
        private readonly OsepDbContext _context;

        public CommentController(OsepDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int eventId, string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest("Comment content cannot be empty");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            var comment = new Comment
            {
                Content = content,
                EventId = eventId,
                UserId = user.UserId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Event", new { id = eventId });
        }


    }
}
