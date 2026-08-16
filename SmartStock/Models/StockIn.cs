using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStock.Models
{
    public class StockIn
    {
        public int StockInId { get; set; }

        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Supplier is required")]
        public int SupplierId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public int Quantity { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be greater than 0")]
        [Column(TypeName = "decimal")]
        public decimal UnitCost { get; set; }

        [Column(TypeName = "decimal")]
        public decimal TotalCost { get; set; }

        public DateTime StockInDate { get; set; }

        [StringLength(50)]
        public string ReferenceNo { get; set; }

        [StringLength(255)]
        public string Remarks { get; set; }

        public virtual Product Product { get; set; }
        public virtual Supplier Supplier { get; set; }
    }
}