using System;
using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models
{
    public class StockOut
    {
        public int StockOutId { get; set; }

        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        public DateTime StockOutDate { get; set; }

        [StringLength(100)]
        public string Purpose { get; set; }

        [StringLength(50)]
        public string ReferenceNo { get; set; }

        [StringLength(255)]
        public string Remarks { get; set; }

        public virtual Product Product { get; set; }
    }
}