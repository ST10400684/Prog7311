using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Prog.Models;
using System.Security.Claims;
using Prog.ViewModels;


namespace Prog.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly AgriEnergyConnectContext _context;

        public ProductController(AgriEnergyConnectContext context)
        {
            _context = context;
        }

        // GET: Product/Create - For farmers to add products
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> Create()
        {
            // Get categories for dropdown
            ViewData["Categories"] = new SelectList(await _context.ProductCategories.ToListAsync(), "CategoryId", "CategoryName");

            return View();
        }

        // POST: Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Get the current farmer's ID
                var userId = int.Parse(User.FindFirstValue("UserId"));
                var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.UserId == userId);

                if (farmer == null)
                {
                    return NotFound("Farmer profile not found.");
                }

                var product = new Product
                {
                    FarmerId = farmer.FarmerId,
                    CategoryId = model.CategoryId,
                    ProductName = model.ProductName,
                    Description = model.Description,
                    ProductionDate = DateOnly.FromDateTime(model.ProductionDate),
                    CreatedDate = DateTime.Now
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyProducts));
            }

            ViewData["Categories"] = new SelectList(await _context.ProductCategories.ToListAsync(), "CategoryId", "CategoryName");
            return View(model);
        }

        // new methods
        [HttpGet]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.UserId == userId);

            if (farmer == null)
            {
                return NotFound("Farmer profile not found.");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.FarmerId == farmer.FarmerId);

            if (product == null)
            {
                return NotFound("Product not found or you don't have permission to edit it.");
            }

            var viewModel = new ProductEditViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Description = product.Description,
                CategoryId = product.CategoryId,
                ProductionDate = product.ProductionDate.ToDateTime(new TimeOnly())
            };

            ViewData["Categories"] = new SelectList(await _context.ProductCategories.ToListAsync(), "CategoryId", "CategoryName", product.CategoryId);

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> Edit(ProductEditViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = int.Parse(User.FindFirstValue("UserId"));
                var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.UserId == userId);

                if (farmer == null)
                {
                    return NotFound("Farmer profile not found.");
                }

                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.ProductId == model.ProductId && p.FarmerId == farmer.FarmerId);

                if (product == null)
                {
                    return NotFound("Product not found or you don't have permission to edit it.");
                }

                // Update product properties
                product.ProductName = model.ProductName;
                product.Description = model.Description;
                product.CategoryId = model.CategoryId;
                product.ProductionDate = DateOnly.FromDateTime(model.ProductionDate);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(MyProducts));
            }

            ViewData["Categories"] = new SelectList(await _context.ProductCategories.ToListAsync(), "CategoryId", "CategoryName", model.CategoryId);
            return View(model);
        }

        // GET: Product/MyProducts - For farmers to view their own products
        [Authorize(Roles = "Farmer")]
        public async Task<IActionResult> MyProducts()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var farmer = await _context.Farmers.FirstOrDefaultAsync(f => f.UserId == userId);

            if (farmer == null)
            {
                return NotFound("Farmer profile not found.");
            }

            var products = await _context.ProductFullViews
                .Where(p => p.FarmerId == farmer.FarmerId)
                .ToListAsync();

            return View(products);
        }

        // GET: Product/List/{farmerId} - For employees to view products of a specific farmer
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> List(int farmerId, DateTime? startDate = null, DateTime? endDate = null, int? categoryId = null)
        {
            var farmer = await _context.Farmers.FindAsync(farmerId);
            if (farmer == null)
            {
                return NotFound();
            }

            ViewData["FarmerName"] = farmer.FarmerName;
            ViewData["FarmerId"] = farmerId;
            ViewData["Categories"] = new SelectList(await _context.ProductCategories.ToListAsync(), "CategoryId", "CategoryName");

            // Apply filters
            var query = _context.ProductFullViews.Where(p => p.FarmerId == farmerId);

            if (startDate.HasValue)
            {
                var startDateOnly = DateOnly.FromDateTime(startDate.Value);
                query = query.Where(p => p.ProductionDate >= startDateOnly);
                ViewData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
            }

            if (endDate.HasValue)
            {
                var endDateOnly = DateOnly.FromDateTime(endDate.Value);
                query = query.Where(p => p.ProductionDate <= endDateOnly);
                ViewData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId);
                ViewData["SelectedCategoryId"] = categoryId;
            }

            var products = await query.ToListAsync();
            return View(products);
        }
    }

    public class ProductCreateViewModel
    {
        public string ProductName { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
        public DateTime ProductionDate { get; set; }
    }
}