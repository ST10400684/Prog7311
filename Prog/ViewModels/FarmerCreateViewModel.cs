using System.ComponentModel.DataAnnotations;

namespace Prog.ViewModels
{
    public class FarmerCreateViewModel
    {
        // Account info
        [Required]
        [Display(Name = "Username")]
        [StringLength(50, MinimumLength = 4)]
        public string Username { get; set; }

        [Required]
        [Display(Name = "Password")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$",
            ErrorMessage = "Password must include uppercase, lowercase, number and special character")]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Farmer profile
        [Required]
        [Display(Name = "Farmer Name")]
        public string FarmerName { get; set; }

        [Required]
        [Display(Name = "Farm Name")]
        public string FarmName { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        [Display(Name = "Contact Number")]
        public string ContactNumber { get; set; }
    }
}
