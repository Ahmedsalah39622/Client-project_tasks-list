using System.Collections.Generic;

namespace Company_representative_tasks.Models
{
    public class DashboardViewModel
    {
        public IEnumerable<Task>? Tasks { get; set; }
        public IEnumerable<Invoice>? Invoices { get; set; }
        public IEnumerable<Note>? Notes { get; set; }
    }
}
