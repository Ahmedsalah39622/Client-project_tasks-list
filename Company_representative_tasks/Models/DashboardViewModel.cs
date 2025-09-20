using System.Collections.Generic;

namespace Argent_Company.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<Task>? Tasks { get; set; }
        public IEnumerable<Invoice>? Invoices { get; set; }
        public IEnumerable<Note>? Notes { get; set; }
        public string? UserName { get; set; }
        public string? NgrokUrl { get; set; }

        // Today's Collection Total
        public decimal TodayTotalCollections { get; set; }
    }
}
