using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SmartStock.Models
{
    public class Supplier
    {
        public int SupplierId { get; set; }

        [Required(ErrorMessage = "Supplier name is required")]
        [StringLength(150)]
        public string SupplierName { get; set; }

        [StringLength(100)]
        public string ContactPerson { get; set; }

        [StringLength(30)]
        public string Phone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string Email { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        public bool IsActive { get; set; }

        public virtual ICollection<StockIn> StockIns { get; set; }
    }
}