namespace SmartStock.ViewModels
{
    public class StockSummaryReportItem
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal StockValue { get; set; }
    }
}