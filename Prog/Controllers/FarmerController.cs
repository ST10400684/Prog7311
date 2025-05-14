using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prog.Models;
using System.Security.Cryptography;
using System.Text;
using Prog.ViewModels;


namespace Prog.Controllers
{
    [Authorize(Roles = "Employee")]
    public class FarmerController : Controller
    {
        private readonly AgriEnergyConnectContext _context;

        public FarmerController(AgriEnergyConnectContext context)
        {
            _context = context;
        }

        // GET: Farmer
        public async Task<IActionResult> Index()
        {
            var farmers = await _context.Farmers
                .Include(f => f.User)
                .ToListAsync();

            return View(farmers);
        }

        // GET: Farmer/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Farmer/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FarmerCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                if (await _context.Users.AnyAsync(u => u.Username == model.Username))
                {
                    ModelState.AddModelError("Username", "Username already exists");
                    return View(model);
                }

                // Create user account for farmer
                var salt = GenerateSalt();
                var passwordHash = HashPassword(model.Password, salt);

                var user = new User
                {
                    Username = model.Username,
                    PasswordHash = passwordHash,
                    Salt = salt,
                    Email = model.Email,
                    UserRole = "Farmer",
                    CreatedDate = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Create farmer profile
                var farmer = new Farmer
                {
                    UserId = user.UserId,
                    FarmerName = model.FarmerName,
                    FarmName = model.FarmName,
                    Location = model.Location,
                    ContactNumber = model.ContactNumber,
                    RegistrationDate = DateTime.Now
                };

                _context.Farmers.Add(farmer);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Helper methods for password hashing
        private string GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        private string HashPassword(string password, string saltString)
        {
            byte[] salt = Convert.FromBase64String(saltString);
            int iterations = 1000;

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(20);
                return $"{iterations}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
            }
        }
    }

    public class FarmerCreateViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string FarmerName { get; set; }
        public string FarmName { get; set; }
        public string Location { get; set; }
        public string ContactNumber { get; set; }
    }
}