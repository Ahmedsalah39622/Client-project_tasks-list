using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Argent_Company.Models;

namespace Argent_Company.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Dashboard
        public async Task<IActionResult> Index()
        {
            // Require login for dashboard
            var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null) return RedirectToAction("Login", "Account");
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login", "Account");
            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Admin");
            }
            var viewModel = new DashboardViewModel
            {
                Tasks = await _context.Tasks.Include(t => t.User).Where(t => t.UserId == userId).ToListAsync(),
                Invoices = await _context.Invoices.Include(i => i.User).Where(i => i.UserId == userId).ToListAsync(),
                Notes = await _context.Notes.Include(n => n.User).Where(n => n.UserId == userId).ToListAsync(),
                UserName = user.Name
            };
            return View(viewModel);
        }

        // POST: Dashboard/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
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
    }
}
