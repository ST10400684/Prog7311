using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prog.Models;
using System.Security.Claims;
using Prog.ViewModels;


namespace Prog.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AgriEnergyConnectContext _context;

        public HomeController(ILogger<HomeController> logger, AgriEnergyConnectContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Farmer"))
                {
                    return RedirectToAction("Dashboard", "Farmer");
                }
                else if (User.IsInRole("Employee"))
                {
                    return RedirectToAction("Dashboard", "Employee");
                }
            }

            return View();
        }

        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var farmer = await _context.Farmers
                .FirstOrDefaultAsync(f => f.UserId == userId);

            if (farmer == null)
            {
                return NotFound("Farmer profile not found.");
            }

            // Get count of products
            var productCount = await _context.Products
                .CountAsync(p => p.FarmerId == farmer.FarmerId);

            var viewModel = new FarmerDashboardViewModel
            {
                Farmer = farmer,
                ProductCount = productCount
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Employee")]
        [HttpGet("Employee/Dashboard")]
        public async Task<IActionResult> EmployeeDashboard()
        {
            // Get counts
            var farmerCount = await _context.Farmers.CountAsync();
            var productCount = await _context.Products.CountAsync();
            var categoryCount = await _context.ProductCategories.CountAsync();

            // Use the ViewModel from the ViewModels namespace
            var viewModel = new Prog.ViewModels.EmployeeDashboardViewModel
            {
                FarmerCount = farmerCount,
                ProductCount = productCount,
                CategoryCount = categoryCount,
                Farmers = await _context.Farmers.ToListAsync()
            };

            return View("EmployeeDashboard", viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}