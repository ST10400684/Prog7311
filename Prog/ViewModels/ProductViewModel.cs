using System;
using System.ComponentModel.DataAnnotations;

namespace Prog.ViewModels
{
    public class ProductEditViewModel
    {
        public int ProductId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        [Display(Name = "Or Enter New Category")]
        public string? CustomCategoryName { get; set; }

        [Required]
        [Display(Name = "Production Date")]
        [DataType(DataType.Date)]
        public DateTime ProductionDate { get; set; }
    }
}
