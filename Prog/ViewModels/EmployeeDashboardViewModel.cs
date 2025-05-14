using System.Collections.Generic;
using Prog.Models;

namespace Prog.ViewModels
{
    public class EmployeeDashboardViewModel
    {
        public int FarmerCount { get; set; }
        public int ProductCount { get; set; }
        public int CategoryCount { get; set; }
        public List<Farmer> Farmers { get; set; }
    }
}
