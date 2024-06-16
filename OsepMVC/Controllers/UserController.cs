using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsepMVC.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace OsepMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly OsepDbContext _context;

        public UserController(OsepDbContext context)
        {
            _context = context;
        }

        // GET: User/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: User/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("FirstName,LastName,Email,Password,Role,University")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), "Home");
            }
            return View(user);
        }

        // GET: User/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: User/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserName", user.FirstName);
            HttpContext.Session.SetString("IsAdmin", user.Role == "Admin" ? "true" : "false");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim("UserId", user.UserId.ToString()),
                new Claim("IsAdmin", user.Role == "Admin" ? "true" : "false")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction(nameof(Index), "Home");
        }

        // GET: User
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Index), "Home");
        }

        public async Task<IActionResult> MyEvents()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var events = await _context.Events
                .Include(e => e.User)
                .Include(e => e.Votes)
                .Where(e => e.UserId == int.Parse(userId))
                .ToListAsync();

            return View(events);
        }
        public async Task<IActionResult> MyParticipations()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                
                return RedirectToAction("Login", "User");
            }

            var participations = await _context.EventParticipations
                .Include(ep => ep.Event)
                .Where(ep => ep.UserId == int.Parse(userId))
                .ToListAsync();

            return View(participations);
        }
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(User user)
        {
            if (!ModelState.IsValid)
            {
                return View(user);
            }

            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId) || user.UserId != int.Parse(userId))
            {
                return Unauthorized();
            }

            var userToUpdate = await _context.Users.FindAsync(user.UserId);
            if (userToUpdate == null)
            {
                return NotFound();
            }

            userToUpdate.FirstName = user.FirstName;
            userToUpdate.LastName = user.LastName;
            userToUpdate.Email = user.Email;
            userToUpdate.Password = user.Password; 
            userToUpdate.University = user.University;
            userToUpdate.Role = user.Role;

            try
            {
                _context.Update(userToUpdate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
        }
        [HttpGet]
        public async Task<IActionResult> Delete()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == int.Parse(userId));

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed()
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login");
            }

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            HttpContext.Session.Clear(); 

            return RedirectToAction("Index", "Home");
        }
        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
