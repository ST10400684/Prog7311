// ReportController.cs (new controller)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prog.Models;
using System.Globalization;
using System.Text;
using Prog.ViewModels;


namespace Prog.Controllers
{
    [Authorize(Roles = "Employee")]
    public class ReportController : Controller
    {
        private readonly AgriEnergyConnectContext _context;

        public ReportController(AgriEnergyConnectContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get product statistics by category
            var productsByCategory = await _context.Products
                .GroupBy(p => p.CategoryId)
                .Select(g => new CategoryReportViewModel
                {
                    CategoryId = g.Key,
                    CategoryName = g.FirstOrDefault().Category.CategoryName,
                    ProductCount = g.Count()
                })
                .ToListAsync();

            // Get product statistics by farmer
            var productsByFarmer = await _context.Products
                .GroupBy(p => p.FarmerId)
                .Select(g => new FarmerReportViewModel
                {
                    FarmerId = g.Key,
                    FarmerName = g.FirstOrDefault().Farmer.FarmerName,
                    FarmName = g.FirstOrDefault().Farmer.FarmName,
                    ProductCount = g.Count()
                })
                .ToListAsync();

            var viewModel = new ReportIndexViewModel
            {
                ProductsByCategory = productsByCategory,
                ProductsByFarmer = productsByFarmer
            };

            return View(viewModel);
        }

        public async Task<IActionResult> ExportProductsCsv(int? farmerId = null, int? categoryId = null)
        {
            var query = _context.ProductFullViews.AsQueryable();

            if (farmerId.HasValue)
            {
                query = query.Where(p => p.FarmerId == farmerId.Value);
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query.ToListAsync();

            var builder = new StringBuilder();
            builder.AppendLine("ProductId,ProductName,Description,ProductionDate,FarmerName,FarmName,CategoryName");

            foreach (var product in products)
            {
                builder.AppendLine($"{product.ProductId},\"{product.ProductName}\",\"{product.ProductDescription}\",{product.ProductionDate},\"{product.FarmerName}\",\"{product.FarmName}\",\"{product.CategoryName}\"");
            }

            return File(Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", "products_report.csv");
        }
    }

    public class CategoryReportViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
    }

    public class FarmerReportViewModel
    {
        public int FarmerId { get; set; }
        public string FarmerName { get; set; }
        public string FarmName { get; set; }
        public int ProductCount { get; set; }
    }

    public class ReportIndexViewModel
    {
        public List<CategoryReportViewModel> ProductsByCategory { get; set; }
        public List<FarmerReportViewModel> ProductsByFarmer { get; set; }
    }
}