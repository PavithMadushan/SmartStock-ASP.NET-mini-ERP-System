using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SmartStock.Data;
using SmartStock.ViewModels;

namespace SmartStock.Controllers
{
    public class DashboardController : Controller
    {
        private SmartStockDbContext db = new SmartStockDbContext();

        public ActionResult Index()
        {
            var model = new DashboardViewModel
            {
                TotalProducts = GetTotalProducts(),
                TotalStockInThisMonth = GetTotalStockInThisMonth(),
                TotalStockOutThisMonth = GetTotalStockOutThisMonth(),
                CurrentStockValue = GetCurrentStockValue(),
                LowStockProducts = GetLowStockProducts()
            };

            BuildMonthlyMovementChart(model);
            BuildCategoryChart(model);

            return View(model);
        }

        // --- KPI Queries ---
        // Each of these uses Count()/Sum()/GroupBy() so aggregation happens
        // on the SQL Server side, not by loading full tables into memory.

        private int GetTotalProducts()
        {
            return db.Products.Count(p => p.IsActive);
        }

        private int GetTotalStockInThisMonth()
        {
            DateTime monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime monthEnd = monthStart.AddMonths(1);

            return db.StockIns
                .Where(s => s.StockInDate >= monthStart && s.StockInDate < monthEnd)
                .Select(s => (int?)s.Quantity)
                .Sum() ?? 0;
        }

        private int GetTotalStockOutThisMonth()
        {
            DateTime monthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime monthEnd = monthStart.AddMonths(1);

            return db.StockOuts
                .Where(s => s.StockOutDate >= monthStart && s.StockOutDate < monthEnd)
                .Select(s => (int?)s.Quantity)
                .Sum() ?? 0;
        }

        private decimal GetCurrentStockValue()
        {
            // Stock Value = sum of (CurrentStock x UnitPrice) across all active products
            return db.Products
                .Where(p => p.IsActive)
                .Select(p => (decimal?)(p.CurrentStock * p.UnitPrice))
                .Sum() ?? 0;
        }

        private System.Collections.Generic.List<LowStockItem> GetLowStockProducts()
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

        // --- Chart 1: Monthly Stock Movement (last 6 months, including current) ---
        private void BuildMonthlyMovementChart(DashboardViewModel model)
        {
            model.ChartMonths = new System.Collections.Generic.List<string>();
            model.MonthlyStockIn = new System.Collections.Generic.List<int>();
            model.MonthlyStockOut = new System.Collections.Generic.List<int>();

            DateTime today = DateTime.Now;

            for (int i = 5; i >= 0; i--)
            {
                DateTime monthDate = today.AddMonths(-i);
                DateTime monthStart = new DateTime(monthDate.Year, monthDate.Month, 1);
                DateTime monthEnd = monthStart.AddMonths(1);

                int stockIn = db.StockIns
                    .Where(s => s.StockInDate >= monthStart && s.StockInDate < monthEnd)
                    .Select(s => (int?)s.Quantity)
                    .Sum() ?? 0;

                int stockOut = db.StockOuts
                    .Where(s => s.StockOutDate >= monthStart && s.StockOutDate < monthEnd)
                    .Select(s => (int?)s.Quantity)
                    .Sum() ?? 0;

                model.ChartMonths.Add(monthStart.ToString("MMM"));
                model.MonthlyStockIn.Add(stockIn);
                model.MonthlyStockOut.Add(stockOut);
            }
        }

        // --- Chart 2: Stock by Category (based on current stock quantity per category) ---
        private void BuildCategoryChart(DashboardViewModel model)
        {
            var categoryData = db.Products
                .Where(p => p.IsActive)
                .GroupBy(p => p.Category.CategoryName)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    TotalStock = g.Sum(p => p.CurrentStock)
                })
                .Where(x => x.TotalStock > 0)
                .OrderByDescending(x => x.TotalStock)
                .ToList();

            model.CategoryLabels = categoryData.Select(x => x.CategoryName).ToList();
            model.CategoryValues = categoryData.Select(x => x.TotalStock).ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}