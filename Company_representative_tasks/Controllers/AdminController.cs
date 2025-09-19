using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Argent_Company.Models;
using System.Linq;

namespace Argent_Company.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddTask(int AgentId, string Title, string? Description, DateTime DueDate)
        {
            var agent = await _context.Users.FirstOrDefaultAsync(u => u.Id == AgentId && u.Role == "Agent");
            if (agent == null)
            {
                TempData["NgrokOutput"] = "Agent not found.";
                return RedirectToAction("Index");
            }
            var task = new Argent_Company.Models.Task
            {
                Title = Title,
                Description = Description,
                DueDate = DueDate,
                UserId = agent.Id,
                User = agent
            };
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            TempData["NgrokOutput"] = "Task added successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveTask(int TaskId)
        {
            var task = await _context.Tasks.FindAsync(TaskId);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                TempData["NgrokOutput"] = "Task removed successfully.";
            }
            else
            {
                TempData["NgrokOutput"] = "Task not found.";
            }
            return RedirectToAction("Index");
        }

        // GET: Admin/TaskNotes/5
        public async Task<IActionResult> TaskNotes(int id)
        {
            var task = await _context.Tasks
                .Include(t => t.User)
                .Include(t => t.Notes)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (task == null)
            {
                return NotFound();
            }

            return View(task);

            if (task == null)
            {
                TempData["Error"] = "المهمة غير موجودة";
                return RedirectToAction(nameof(Index));
            }

            return View(task);
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(a => a.Tasks)
                .Include(a => a.Invoices)
                .Include(a => a.Notes)
                .ToListAsync();

            // Get current admin name from session
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var admin = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
                if (admin != null)
                {
                    ViewBag.AdminName = admin.Name;
                }
            }

            // Check if ngrok is running
            bool isNgrokRunning = System.Diagnostics.Process.GetProcessesByName("ngrok").Any();
            ViewBag.NgrokStatus = isNgrokRunning ? "Agent Public Link is running" : "Agent Public Link is not running";

            // Try to get the public ngrok URL using the ngrok API
            try
            {
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    var response = await httpClient.GetAsync("http://127.0.0.1:4040/api/tunnels");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        dynamic obj = System.Text.Json.JsonDocument.Parse(json);
                        foreach (var tunnel in obj.RootElement.GetProperty("tunnels").EnumerateArray())
                        {
                            System.Text.Json.JsonElement urlProp;
                            if (tunnel.TryGetProperty("public_url", out urlProp))
                            {
                                string? url = urlProp.GetString();
                                if (!string.IsNullOrEmpty(url) && url.StartsWith("https://"))
                                {
                                    ViewBag.NgrokLink = url;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch { /* Ignore errors if ngrok API is not available */ }

            return View(users);
        }

        [HttpPost]
        public IActionResult GenerateNgrokLink()
        {
            string ngrokPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "tools", "ngrok", "ngrok.exe");
            string ngrokConfig = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "tools", "ngrok", "ngrok.yml");
            string outputFile = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "ngrok_output.txt");
            try
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = ngrokPath;
                process.StartInfo.Arguments = $"http 5000 --config \"{ngrokConfig}\"";
                process.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(ngrokPath);
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                // Write output to file asynchronously
                System.Threading.Tasks.Task.Run(() => {
                    using (var writer = new System.IO.StreamWriter(outputFile, false))
                    {
                        while (!process.HasExited)
                        {
                            string? line = process.StandardOutput.ReadLine();
                            if (!string.IsNullOrEmpty(line))
                                writer.WriteLine(line);
                        }
                        writer.WriteLine(process.StandardOutput.ReadToEnd());
                        writer.WriteLine(process.StandardError.ReadToEnd());
                    }
                });
            }
            catch (Exception ex)
            {
                TempData["NgrokOutput"] = "Error: " + ex.Message;
                return RedirectToAction("Index");
            }
            TempData["NgrokOutput"] = "ngrok started. Waiting for link...";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult StopNgrok()
        {
            string output = "";
            try
            {
                foreach (var process in System.Diagnostics.Process.GetProcessesByName("ngrok"))
                {
                    process.Kill();
                }
                output = "Links stopped.";
            }
            catch (Exception ex)
            {
                output = "Error: " + ex.Message;
            }
            TempData["NgrokOutput"] = output;
            return RedirectToAction("Index");
        }
    }
}