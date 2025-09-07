using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Company_representative_tasks.Models;
using System.Linq;

namespace Company_representative_tasks.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(a => a.Tasks)
                .Include(a => a.Invoices)
                .Include(a => a.Notes)
                .ToListAsync();
            return View(users);
        }
    }
}