using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SmartStock.ViewModels
{
    public class StockOutReportViewModel
    {
        // Filters
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? ProductId { get; set; }

        public IEnumerable<SelectListItem> ProductList { get; set; }

        // Results
        public List<StockOutReportItem> Results { get; set; }
    }

    public class StockOutReportItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string Purpose { get; set; }
        public DateTime StockOutDate { get; set; }
        public string ReferenceNo { get; set; }
    }
}