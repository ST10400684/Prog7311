using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prog.Models;
using System.Security.Claims;
using Prog.ViewModels;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace Prog.Controllers
{
    public class AccountController : Controller
    {
        private readonly AgriEnergyConnectContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(AgriEnergyConnectContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Account/Login
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == model.Username);

                if (user != null && VerifyPasswordTemporary(model.Password, user.PasswordHash))
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.UserRole),
                        new Claim("UserId", user.UserId.ToString())
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.RememberMe
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    // Update last login time
                    user.LastLogin = DateTime.Now;
                    await _context.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }

        // GET: Account/Profile
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue("UserId"));
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var viewModel = new ViewModels.UserProfileViewModel
            {
                Username = user.Username,
                Email = user.Email,
                UserRole = user.UserRole,
                CreatedDate = user.CreatedDate ?? DateTime.Now,
                LastLogin = user.LastLogin
            };

            return View(viewModel);
        }

        // New Code
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null)
                {
                    // In a real application, you would:
                    // 1. Generate a reset token
                    // 2. Store it in the database with an expiration
                    // 3. Send an email with a reset link

                    // For this prototype, just show a success message
                    TempData["SuccessMessage"] = "If the email exists in our system, a password reset link will be sent.";
                    return RedirectToAction("Login");
                }

                // Don't reveal that the user does not exist
                TempData["SuccessMessage"] = "If the email exists in our system, a password reset link will be sent.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        public class ForgotPasswordViewModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        // GET: Account/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // TEMPORARY solution that works with the current plain text passwords in the database
        private bool VerifyPasswordTemporary(string password, string storedPassword)
        {
            _logger.LogWarning("Using insecure plain text password verification");
            return password == storedPassword;
        }

        // The original secure method - keep this for future implementation
        private bool VerifyPasswordSecure(string password, string storedHash, string storedSalt)
        {
            try
            {
                // Only attempt to decode if the stored salt has a valid format
                byte[] saltBytes;

                try
                {
                    saltBytes = Convert.FromBase64String(storedSalt);
                }
                catch (FormatException)
                {
                    _logger.LogError("Invalid salt format in database: {Salt}", storedSalt);
                    return false;
                }

                var parts = storedHash.Split(':');

                if (parts.Length != 3)
                {
                    _logger.LogError("Invalid hash format in database");
                    return false;
                }

                if (!int.TryParse(parts[0], out int iterations))
                {
                    _logger.LogError("Invalid iteration count in hash");
                    return false;
                }

                byte[] salt;
                byte[] hash;

                try
                {
                    salt = Convert.FromBase64String(parts[1]);
                    hash = Convert.FromBase64String(parts[2]);
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Invalid Base64 format in hash components");
                    return false;
                }

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    byte[] testHash = pbkdf2.GetBytes(hash.Length);
                    return testHash.SequenceEqual(hash);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in password verification");
                return false;
            }
        }

        // Helper method to hash a password for future use when migrating to secure storage
        public static (string hash, string salt) HashPassword(string password)
        {
            const int keySize = 32; // 256 bits
            const int iterations = 10000;

            // Generate a random salt
            byte[] saltBytes = RandomNumberGenerator.GetBytes(keySize);
            string saltBase64 = Convert.ToBase64String(saltBytes);

            // Hash the password with PBKDF2
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, iterations, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(keySize);

                // Format as iterations:salt:hash
                string hashBase64 = Convert.ToBase64String(hashBytes);
                string saltPart = Convert.ToBase64String(saltBytes);
                string completeHash = $"{iterations}:{saltPart}:{hashBase64}";

                return (completeHash, saltBase64);
            }
        }

        [HttpGet]
        public IActionResult AccessDenied(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(); // this looks for AccessDenied.cshtml by default
        }



        // Method to help migrate your database to secure password storage
        [HttpGet]
        [Route("/Admin/MigratePasswords")]
        [Authorize(Roles = "Admin")] // Make sure to secure this endpoint!
        public async Task<IActionResult> MigratePasswords()
        {
            // This should be run once to convert plaintext passwords to hashed ones
            var users = await _context.Users.ToListAsync();
            int count = 0;

            foreach (var user in users)
            {
                // Only update if it looks like a plain text password
                if (!user.PasswordHash.Contains(":"))
                {
                    var plaintextPassword = user.PasswordHash;
                    var (hash, salt) = HashPassword(plaintextPassword);

                    user.PasswordHash = hash;
                    user.Salt = salt;
                    count++;
                }
            }

            if (count > 0)
            {
                await _context.SaveChangesAsync();
                return Content($"Successfully migrated {count} passwords");
            }

            return Content("No passwords needed migration");
        }
    }
}