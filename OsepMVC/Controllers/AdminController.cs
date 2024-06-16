using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsepMVC.Models;
using System.Threading.Tasks;

namespace OsepMVC.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly OsepDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(OsepDbContext context)
        {
            _context = context;
        }

       
        public IActionResult Index()
        {
            return View();
        }

       
        public async Task<IActionResult> Users()
        {
            return View(await _context.Users.ToListAsync());
        }

       
        public async Task<IActionResult> Events()
        {
            var events = await _context.Events.Include(e => e.User).Include(e => e.Votes).ToListAsync();
            return View(events);
        }
        // GET: Admin/CreateAdmin
        public IActionResult CreateAdmin()
        {
            return View();
        }

        // POST: Admin/CreateAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAdmin([Bind("FirstName,LastName,Email,Password,Role,University")] User user)
        {
            if (ModelState.IsValid)
            {
                if (user.Role == "Admin")
                {
                    user.IsAdmin = true;
                }
                else
                {
                    user.IsAdmin = false;
                }
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Admin/EditUser/5
        public async Task<IActionResult> EditUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, [Bind("UserId,FirstName,LastName,Email,Password,Role,University")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Users));
            }
            return View(user);
        }

        // GET: Admin/DeleteUser/5
        public async Task<IActionResult> DeleteUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/DeleteUser/5
        [HttpPost, ActionName("DeleteUser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUserConfirmed(int id)
        {
            var user = await _context.Users
                .Include(u => u.Votes)
                .Include(u => u.Comments)
                .Include(u => u.Participations)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

           
            _context.Votes.RemoveRange(user.Votes);

           
            _context.Comments.RemoveRange(user.Comments);

          
            _context.EventParticipations.RemoveRange(user.Participations);

           
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
        public async Task<IActionResult> EventParticipations(int eventId)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Participations)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventEntity == null)
            {
                return NotFound();
            }

            return View(eventEntity);
        }
    }
}
