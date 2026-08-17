using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStock.ViewModels
{
    public class StockInReportViewModel
    {
        // Filters
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? ProductId { get; set; }
        public int? SupplierId { get; set; }

        public IEnumerable<SelectListItem> ProductList { get; set; }
        public IEnumerable<SelectListItem> SupplierList { get; set; }

        // Results
        public List<StockInReportItem> Results { get; set; }
        public decimal TotalCost { get; set; }
    }

    public class StockInReportItem
    {
        public string ProductName { get; set; }
        public string SupplierName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime StockInDate { get; set; }
    }
}