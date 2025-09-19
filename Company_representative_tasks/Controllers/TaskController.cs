using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Argent_Company.Models;
using TaskModel = Argent_Company.Models.Task;
using static Argent_Company.Models.Constants;

namespace Argent_Company.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaskController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Task
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Tasks.Include(t => t.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Task/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // GET: Task/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Task/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    public async Task<IActionResult> Create([Bind("Id,Title,Description,DueDate,UserId")] TaskModel task)
        {
            if (ModelState.IsValid)
            {
                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", task.UserId);
            return View(task);
        }

        // GET: Task/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks.FindAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", task.UserId);
            return View(task);
        }

        // POST: Task/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DueDate,UserId,Status,CollectionAmount")] TaskModel task)
        {
            if (id != task.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaskExists(task.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", task.UserId);
            return View(task);
        }

        // POST: Task/AddNote
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddNote(AddNoteModel model)
        {
            if (string.IsNullOrEmpty(model.Content))
            {
                return BadRequest("المحتوى مطلوب");
            }

            var task = await _context.Tasks.FindAsync(model.TaskId);
            if (task == null)
            {
                return NotFound("المهمة غير موجودة");
            }

            var userIdNullable = HttpContext.Session.GetInt32(SessionKeyUserId);
            if (!userIdNullable.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }
            int userId = userIdNullable.Value;

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return BadRequest("المستخدم غير موجود");
            }

            var note = new Note
            {
                Content = model.Content,
                TaskId = model.TaskId,
                UserId = userId,
                User = user,
                Task = task
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            // Check if it's an AJAX request
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }
            
            // For non-AJAX requests, redirect back
            return RedirectToAction("Index", "Dashboard");
        }

        public class AddNoteModel
        {
            public int TaskId { get; set; }
            public required string Content { get; set; }
        }

        // GET: Task/GetNotes/5
        [HttpGet]
        public async Task<IActionResult> GetNotes(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
            {
                return NotFound("المهمة غير موجودة");
            }

            var notes = await _context.Notes
                .Where(n => n.TaskId == taskId)
                .Include(n => n.User)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new
                {
                    id = n.Id,
                    content = n.Content,
                    createdAt = n.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                    userName = n.User.Name
                })
                .ToListAsync();

            return Json(notes);
        }

        private bool TaskExists(int id)
        {
            return _context.Tasks.Any(e => e.Id == id);
        }
        
        // POST: Task/MarkDone
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkDone(MarkDoneModel model)
        {
            if (model.CollectionAmount == 0)
            {
                return BadRequest("يرجى إدخال مبلغ التحصيل بشكل صحيح.");
            }

            var task = await _context.Tasks.FindAsync(model.TaskId);
            if (task == null)
            {
                return NotFound("المهمة غير موجودة");
            }

            task.Status = "تم";
            task.CollectionAmount = model.CollectionAmount;

            try
            {
                _context.Update(task);
                await _context.SaveChangesAsync();
                TempData["Success"] = "تم الحفظ بنجاح";
                
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = true, message = "تم الحفظ بنجاح" });
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return BadRequest($"حدث خطأ أثناء حفظ التغييرات: {ex.Message}");
            }
        }

        public class MarkDoneModel
        {
            public int TaskId { get; set; }
            public decimal CollectionAmount { get; set; }
        }

        // GET: Task/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var task = await _context.Tasks
                .Include(t => t.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (task == null)
            {
                return NotFound();
            }

            return View(task);
        }

        // POST: Task/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
