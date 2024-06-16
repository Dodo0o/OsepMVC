using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsepMVC.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;

namespace OsepMVC.Controllers
{
    public class EventController : Controller
    {
        private readonly OsepDbContext _context;
        private readonly ILogger<EventController> _logger;

        public EventController(OsepDbContext context, ILogger<EventController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Event
        public async Task<IActionResult> Index(string category)
        {
            IQueryable<Event> events = _context.Events
                .Include(e => e.User)
                .Include(e => e.Votes);

            if (!string.IsNullOrEmpty(category) && category != "All")
            {
                events = events.Where(e => e.Type == category);
            }

            var eventList = await events
                .OrderByDescending(e => e.Votes.Count)
                .ToListAsync();

            ViewBag.Category = category;

            return View(eventList);
        }

        // GET: Event/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventEntity = await _context.Events
                .Include(e => e.User)
                .Include(e => e.Votes)
                .Include(e => e.Comments)
                .ThenInclude(c => c.User)
                .Include(e => e.Participations)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (eventEntity == null)
            {
                return NotFound();
            }

            ViewBag.UserId = HttpContext.Session.GetString("UserId") ?? string.Empty;
            ViewBag.IsAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

            return View(eventEntity);
        }

        // GET: Event/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("UserId") == null)
            {
                return RedirectToAction("Login", "User");
            }

            return View();
        }

        // POST: Event/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EventViewModel model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            _logger.LogInformation("Session UserId: {UserId}", userId);

            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            ModelState.Remove("ImagePath");

            if (ModelState.IsValid)
            {
                var eventEntity = new Event
                {
                    Title = model.Title,
                    Description = model.Description,
                    Date = model.Date,
                    Type = model.Type,
                    UserId = int.Parse(userId),
                    Price = model.Price,
                    Address = model.Address,
                    User = await _context.Users.FindAsync(int.Parse(userId))
                };

                if (model.Image != null && model.Image.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.Image.CopyToAsync(fileStream);
                    }

                    eventEntity.ImagePath = "/images/" + uniqueFileName;
                }

                _context.Add(eventEntity);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Event");
            }

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    _logger.LogError("ModelState Error in {Key}: {ErrorMessage}", state.Key, error.ErrorMessage);
                }
            }

            return View(model);
        }

        // GET: Event/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (userId == null || (eventEntity.UserId != int.Parse(userId) && isAdmin != "true"))
            {
                return Unauthorized();
            }

            var model = new EventViewModel
            {
                EventId = eventEntity.EventId,
                Title = eventEntity.Title,
                Description = eventEntity.Description,
                Date = eventEntity.Date,
                Type = eventEntity.Type,
                ImagePath = eventEntity.ImagePath,
                Price = eventEntity.Price,
                Address = eventEntity.Address
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventViewModel model)
        {
            if (id != model.EventId)
            {
                return NotFound();
            }

            ModelState.Remove("ImagePath");

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        _logger.LogError("ModelState Error in {Key}: {ErrorMessage}", state.Key, error.ErrorMessage);
                    }
                }
                return View(model);
            }

            var eventEntity = await _context.Events.FindAsync(id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (userId == null || (eventEntity.UserId != int.Parse(userId) && isAdmin != "true"))
            {
                return Unauthorized();
            }

            eventEntity.Title = model.Title;
            eventEntity.Description = model.Description;
            eventEntity.Date = model.Date;
            eventEntity.Type = model.Type;
            eventEntity.Price = model.Price;
            eventEntity.Address = model.Address;

            if (model.Image != null && model.Image.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + model.Image.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Image.CopyToAsync(fileStream);
                }

                eventEntity.ImagePath = "/images/" + uniqueFileName;
            }
            
            else
            {
                model.ImagePath = eventEntity.ImagePath;
            }

            _context.Update(eventEntity);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyEvents", "User");
        }

        // GET: Event/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventEntity = await _context.Events
                .Include(e => e.User)
                .Include(e => e.Votes)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (userId == null || (eventEntity.UserId != int.Parse(userId) && isAdmin != "true"))
            {
                return Unauthorized();
            }

            return View(eventEntity);
        }

        // POST: Event/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventEntity = await _context.Events
                .Include(e => e.Votes)
                .FirstOrDefaultAsync(e => e.EventId == id);
            if (eventEntity == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin");

            if (userId == null || (eventEntity.UserId != int.Parse(userId) && isAdmin != "true"))
            {
                return Unauthorized();
            }

            _context.Votes.RemoveRange(eventEntity.Votes);
            _context.Events.Remove(eventEntity);
            await _context.SaveChangesAsync();

            return RedirectToAction("MyEvents", "User");
        }

        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }

        // POST: Event/Vote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Vote(int eventId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            var existingVote = await _context.Votes.FirstOrDefaultAsync(v => v.EventId == eventId && v.UserId == int.Parse(userId));
            if (existingVote != null)
            {
                return RedirectToAction("Index"); 
            }

            var vote = new Vote
            {
                EventId = eventId,
                UserId = int.Parse(userId),
                VoteDate = DateTime.Now
            };

            _context.Add(vote);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index"); 
        }

        [HttpPost]
        public IActionResult AddComment(int eventId, string content)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            var comment = new Comment
            {
                EventId = eventId,
                UserId = int.Parse(userId),
                Content = content,
                Date = DateTime.Now
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = eventId });
        }

        [HttpPost]
        public IActionResult DeleteComment(int commentId)
        {
            var comment = _context.Comments.Find(commentId);

            if (comment == null)
            {
                return NotFound();
            }

            var userId = HttpContext.Session.GetString("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

            if (comment.UserId == int.Parse(userId) || isAdmin)
            {
                _context.Comments.Remove(comment);
                _context.SaveChanges();
                return RedirectToAction("Details", new { id = comment.EventId });
            }

            return Forbid();
        }

        [HttpPost]
        public IActionResult Participate(int eventId)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            var eventEntity = _context.Events.Find(eventId);
            if (eventEntity == null)
            {
                return NotFound();
            }

            bool isPaidEvent = eventEntity.Price.HasValue && eventEntity.Price.Value > 0;
            if (isPaidEvent)
            {
               
                return RedirectToAction("ConfirmParticipation", new { eventId });
            }
            else
            {
                var participation = new EventParticipation
                {
                    EventId = eventId,
                    UserId = int.Parse(userId),
                    ParticipationDate = DateTime.Now
                };

                _context.EventParticipations.Add(participation);
                _context.SaveChanges();
                return RedirectToAction("Details", new { id = eventId });
            }
        }

       
        [HttpGet]
        public IActionResult ConfirmParticipation(int eventId)
        {
            var eventEntity = _context.Events.Find(eventId);
            if (eventEntity == null)
            {
                return NotFound();
            }

            return View(eventEntity);
        }

        
        [HttpPost]
        public IActionResult ConfirmParticipation(int eventId, bool confirmed)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            if (!confirmed)
            {
                return RedirectToAction("Details", new { id = eventId });
            }

            var participation = new EventParticipation
            {
                EventId = eventId,
                UserId = int.Parse(userId),
                ParticipationDate = DateTime.Now
            };

            _context.EventParticipations.Add(participation);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Tebrikler! Başarılı şekilde etkinliğe katıldınız. Etkinlik yetkilisi tarafından ödeme için kısa zaman içerisinde e-mailinize gerekli talimatlar gönderilecektir. İyi eğlenceler!";
            return RedirectToAction("Details", new { id = eventId });
        }

        [HttpPost]
        public IActionResult CancelParticipation(int eventId)
        {
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            var participation = _context.EventParticipations
                .FirstOrDefault(ep => ep.EventId == eventId && ep.UserId == int.Parse(userId));

            if (participation != null)
            {
                _context.EventParticipations.Remove(participation);
                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = eventId });
        }

        // GET: Event/EventParticipations/5
        [HttpGet]
        public async Task<IActionResult> EventParticipations(int eventId)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var isAdmin = HttpContext.Session.GetString("IsAdmin") == "true";

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "User");
            }

            var eventEntity = await _context.Events
                .Include(e => e.Participations)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventEntity == null)
            {
                return NotFound();
            }

            if (eventEntity.UserId != int.Parse(userId) && !isAdmin)
            {
                return Unauthorized(); 
            }

            return View(eventEntity);
        }
    }
}
