using System.Collections.Generic;
using Prog.Models;

namespace Prog.ViewModels
{
    public class FarmerDashboardViewModel
    {
        public Farmer Farmer { get; set; }
        public int ProductCount { get; set; }
        public List<ProductFullView>? RecentProducts { get; set; } // Optional if you plan to show recent products
    }
}
