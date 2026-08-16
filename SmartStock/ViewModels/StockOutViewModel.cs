using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace SmartStock.ViewModels
{
    public class StockOutViewModel
    {
        public int StockOutId { get; set; }

        [Required(ErrorMessage = "Product is required")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Purpose is required")]
        [StringLength(100)]
        public string Purpose { get; set; }

        [Display(Name = "Reference No")]
        [StringLength(50)]
        public string ReferenceNo { get; set; }

        [StringLength(255)]
        public string Remarks { get; set; }

        public IEnumerable<SelectListItem> ProductList { get; set; }
    }
}