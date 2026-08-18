using System;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SmartStock.Data;
using SmartStock.ViewModels;
using SmartStock.Helpers;

namespace SmartStock.Controllers
{
    [CustomAuthorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private SmartStockDbContext db = new SmartStockDbContext();

        // GET: Reports (menu page)
        public ActionResult Index()
        {
            return View();
        }

        //  STOCK SUMMARY REPORT
        public ActionResult StockSummary()
        {
            var results = GetStockSummaryData();
            return View(results);
        }

        public ActionResult ExportStockSummary()
        {
            var results = GetStockSummaryData();

            var sb = new StringBuilder();
            sb.Append("<table border='1'>");
            sb.Append("<tr><th>Product Code</th><th>Product Name</th><th>Category</th><th>Current Stock</th><th>Reorder Level</th><th>Unit Price</th><th>Stock Value</th></tr>");

            foreach (var item in results)
            {
                sb.Append("<tr>");
                sb.Append("<td>" + item.ProductCode + "</td>");
                sb.Append("<td>" + item.ProductName + "</td>");
                sb.Append("<td>" + item.CategoryName + "</td>");
                sb.Append("<td>" + item.CurrentStock + "</td>");
                sb.Append("<td>" + item.ReorderLevel + "</td>");
                sb.Append("<td>" + item.UnitPrice.ToString("N2") + "</td>");
                sb.Append("<td>" + item.StockValue.ToString("N2") + "</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            return ExportAsExcel(sb.ToString(), "StockSummaryReport.xls");
        }

        private System.Collections.Generic.List<StockSummaryReportItem> GetStockSummaryData()
        {
            return db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .OrderBy(p => p.ProductName)
                .Select(p => new StockSummaryReportItem
                {
                    ProductCode = p.ProductCode,
                    ProductName = p.ProductName,
                    CategoryName = p.Category.CategoryName,
                    CurrentStock = p.CurrentStock,
                    ReorderLevel = p.ReorderLevel,
                    UnitPrice = p.UnitPrice,
                    StockValue = p.CurrentStock * p.UnitPrice
                })
                .ToList();
        }

        //  STOCK IN REPORT 

        public ActionResult StockInReport(DateTime? fromDate, DateTime? toDate, int? productId, int? supplierId)
        {
            var model = BuildStockInReport(fromDate, toDate, productId, supplierId);
            return View(model);
        }

        public ActionResult ExportStockInReport(DateTime? fromDate, DateTime? toDate, int? productId, int? supplierId)
        {
            var model = BuildStockInReport(fromDate, toDate, productId, supplierId);

            var sb = new StringBuilder();
            sb.Append("<table border='1'>");
            sb.Append("<tr><th>Product</th><th>Supplier</th><th>Quantity</th><th>Unit Cost</th><th>Total Cost</th><th>Date</th></tr>");

            foreach (var item in model.Results)
            {
                sb.Append("<tr>");
                sb.Append("<td>" + item.ProductName + "</td>");
                sb.Append("<td>" + item.SupplierName + "</td>");
                sb.Append("<td>" + item.Quantity + "</td>");
                sb.Append("<td>" + item.UnitCost.ToString("N2") + "</td>");
                sb.Append("<td>" + item.TotalCost.ToString("N2") + "</td>");
                sb.Append("<td>" + item.StockInDate.ToString("yyyy-MM-dd") + "</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            return ExportAsExcel(sb.ToString(), "StockInReport.xls");
        }

        private StockInReportViewModel BuildStockInReport(DateTime? fromDate, DateTime? toDate, int? productId, int? supplierId)
        {
            var query = db.StockIns.Include(s => s.Product).Include(s => s.Supplier).AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(s => s.StockInDate >= fromDate.Value);

            if (toDate.HasValue)
            {
                // Include the entire "to" day
                DateTime toDateEnd = toDate.Value.Date.AddDays(1);
                query = query.Where(s => s.StockInDate < toDateEnd);
            }

            if (productId.HasValue)
                query = query.Where(s => s.ProductId == productId.Value);

            if (supplierId.HasValue)
                query = query.Where(s => s.SupplierId == supplierId.Value);

            var results = query
                .OrderByDescending(s => s.StockInDate)
                .Select(s => new StockInReportItem
                {
                    ProductName = s.Product.ProductName,
                    SupplierName = s.Supplier.SupplierName,
                    Quantity = s.Quantity,
                    UnitCost = s.UnitCost,
                    TotalCost = s.TotalCost,
                    StockInDate = s.StockInDate
                })
                .ToList();

            return new StockInReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                ProductId = productId,
                SupplierId = supplierId,
                ProductList = new SelectList(db.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName), "ProductId", "ProductName", productId),
                SupplierList = new SelectList(db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.SupplierName), "SupplierId", "SupplierName", supplierId),
                Results = results,
                TotalCost = results.Sum(r => r.TotalCost)
            };
        }

        // STOCK OUT REPORT

        public ActionResult StockOutReport(DateTime? fromDate, DateTime? toDate, int? productId)
        {
            var model = BuildStockOutReport(fromDate, toDate, productId);
            return View(model);
        }

        public ActionResult ExportStockOutReport(DateTime? fromDate, DateTime? toDate, int? productId)
        {
            var model = BuildStockOutReport(fromDate, toDate, productId);

            var sb = new StringBuilder();
            sb.Append("<table border='1'>");
            sb.Append("<tr><th>Product</th><th>Quantity</th><th>Purpose</th><th>Date</th><th>Reference</th></tr>");

            foreach (var item in model.Results)
            {
                sb.Append("<tr>");
                sb.Append("<td>" + item.ProductName + "</td>");
                sb.Append("<td>" + item.Quantity + "</td>");
                sb.Append("<td>" + item.Purpose + "</td>");
                sb.Append("<td>" + item.StockOutDate.ToString("yyyy-MM-dd") + "</td>");
                sb.Append("<td>" + item.ReferenceNo + "</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            return ExportAsExcel(sb.ToString(), "StockOutReport.xls");
        }

        private StockOutReportViewModel BuildStockOutReport(DateTime? fromDate, DateTime? toDate, int? productId)
        {
            var query = db.StockOuts.Include(s => s.Product).AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(s => s.StockOutDate >= fromDate.Value);

            if (toDate.HasValue)
            {
                DateTime toDateEnd = toDate.Value.Date.AddDays(1);
                query = query.Where(s => s.StockOutDate < toDateEnd);
            }

            if (productId.HasValue)
                query = query.Where(s => s.ProductId == productId.Value);

            var results = query
                .OrderByDescending(s => s.StockOutDate)
                .Select(s => new StockOutReportItem
                {
                    ProductName = s.Product.ProductName,
                    Quantity = s.Quantity,
                    Purpose = s.Purpose,
                    StockOutDate = s.StockOutDate,
                    ReferenceNo = s.ReferenceNo
                })
                .ToList();

            return new StockOutReportViewModel
            {
                FromDate = fromDate,
                ToDate = toDate,
                ProductId = productId,
                ProductList = new SelectList(db.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName), "ProductId", "ProductName", productId),
                Results = results
            };
        }

        // LOW STOCK REPORT 

        public ActionResult LowStockReport()
        {
            var results = GetLowStockData();
            return View(results);
        }

        public ActionResult ExportLowStockReport()
        {
            var results = GetLowStockData();

            var sb = new StringBuilder();
            sb.Append("<table border='1'>");
            sb.Append("<tr><th>Product</th><th>Category</th><th>Current Stock</th><th>Reorder Level</th></tr>");

            foreach (var item in results)
            {
                sb.Append("<tr>");
                sb.Append("<td>" + item.ProductName + "</td>");
                sb.Append("<td>" + item.CategoryName + "</td>");
                sb.Append("<td>" + item.CurrentStock + "</td>");
                sb.Append("<td>" + item.ReorderLevel + "</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");

            return ExportAsExcel(sb.ToString(), "LowStockReport.xls");
        }

        private System.Collections.Generic.List<LowStockItem> GetLowStockData()
        {
            return db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.CurrentStock <= p.ReorderLevel)
                .OrderBy(p => p.CurrentStock)
                .Select(p => new LowStockItem
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    CategoryName = p.Category.CategoryName,
                    CurrentStock = p.CurrentStock,
                    ReorderLevel = p.ReorderLevel
                })
                .ToList();
        }

        // SHARED EXCEL EXPORT HELPER 

        private FileContentResult ExportAsExcel(string htmlTable, string fileName)
        {
            string fullHtml = "<html><head><meta charset='utf-8' /></head><body>" + htmlTable + "</body></html>";
            byte[] bytes = Encoding.UTF8.GetBytes(fullHtml);
            return File(bytes, "application/vnd.ms-excel", fileName);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}