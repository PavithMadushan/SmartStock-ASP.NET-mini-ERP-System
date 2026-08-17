using System.Collections.Generic;

namespace SmartStock.ViewModels
{
    public class DashboardViewModel
    {
        // KPI Cards
        public int TotalProducts { get; set; }
        public int TotalStockInThisMonth { get; set; }
        public int TotalStockOutThisMonth { get; set; }
        public decimal CurrentStockValue { get; set; }

        // Low Stock Alert table
        public List<LowStockItem> LowStockProducts { get; set; }

        // Chart 1: Monthly Stock Movement
        public List<string> ChartMonths { get; set; }
        public List<int> MonthlyStockIn { get; set; }
        public List<int> MonthlyStockOut { get; set; }

        // Chart 2: Stock by Category
        public List<string> CategoryLabels { get; set; }
        public List<int> CategoryValues { get; set; }
    }

    public class LowStockItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
    }
}