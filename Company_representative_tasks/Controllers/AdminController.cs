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
            string ngrokPath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "ngrok.exe");
            string outputFile = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "ngrok_output.txt");
            try
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = ngrokPath;
                process.StartInfo.Arguments = "http 5050 --config ngrok.yml"; // Force use of local config
                process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
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