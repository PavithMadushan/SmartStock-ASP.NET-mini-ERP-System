using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SmartStock.ViewModels
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product code is required")]
        [StringLength(50)]
        [Display(Name = "Product Code")]
        public string ProductCode { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(150)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        [Required(ErrorMessage = "Category is required")]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Reorder level cannot be negative")]
        [Display(Name = "Reorder Level")]
        public int ReorderLevel { get; set; }

        // Shown as read-only info on Edit; never comes FROM the submitted form
        public int CurrentStock { get; set; }

        public IEnumerable<SelectListItem> CategoryList { get; set; }
    }
}