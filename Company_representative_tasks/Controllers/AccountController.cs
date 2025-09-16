using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Argent_Company.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace Argent_Company.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string SessionKeyUserId = "UserId";

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        private bool IsAuthenticated()
        {
            return HttpContext.Session.GetInt32(SessionKeyUserId) != null;
        }

        private int? GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32(SessionKeyUserId);
        }

        private IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(string name, string email, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "All fields are required.");
                return View();
            }
            if (await _context.Users.AnyAsync(a => a.Email == email))
            {
                ModelState.AddModelError(string.Empty, "Email already exists.");
                return View();
            }
            var user = new User { Name = name, Email = email, PasswordHash = password, Role = "Agent" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            // Log registration
            var log = new UserLog
            {
                AgentId = user.Id,
                EventType = "Register",
                EventTime = DateTime.Now,
                UserEmail = user.Email
            };
            _context.UserLogs.Add(log);
            await _context.SaveChangesAsync();
            // Auto-login after registration
            HttpContext.Session.SetInt32(SessionKeyUserId, user.Id);
            return RedirectToAction("Index", "Dashboard");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Email and password are required.");
                return View();
            }
            var user = await _context.Users.FirstOrDefaultAsync(a => a.Email == email && a.PasswordHash == password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                return View();
            }
            // Set session
            HttpContext.Session.SetInt32(SessionKeyUserId, user.Id);
            // Log the login event
            var log = new UserLog
            {
                AgentId = user.Id,
                EventType = "Login",
                EventTime = DateTime.Now,
                UserEmail = user.Email
            };
            _context.UserLogs.Add(log);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Dashboard");
        }

        // All other actions: require authentication
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated()) return RedirectToLogin();
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToLogin();
            // Only show the logged-in user's info
            var user = await _context.Users.FirstOrDefaultAsync(a => a.Id == userId);
            return View(new List<User> { user });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (!IsAuthenticated()) return RedirectToLogin();
            if (id == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        public IActionResult Create()
        {
            if (!IsAuthenticated()) return RedirectToLogin();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Email,PasswordHash")] User user)
        {
            if (!IsAuthenticated()) return RedirectToLogin();
            if (ModelState.IsValid)
            {
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAuthenticated()) return RedirectToLogin();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,PasswordHash")] User user)
        {
            if (!IsAuthenticated()) return RedirectToLogin();
            if (id != user.Id)
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
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (!IsAuthenticated()) return RedirectToLogin();
            if (id == null)
            {
                return NotFound();
            }
            var user = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAuthenticated()) return RedirectToLogin();
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
