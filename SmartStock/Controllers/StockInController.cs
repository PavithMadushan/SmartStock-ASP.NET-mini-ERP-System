using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SmartStock.Data;
using SmartStock.Models;
using SmartStock.ViewModels;

namespace SmartStock.Controllers
{
    public class StockInController : Controller
    {
        private SmartStockDbContext db = new SmartStockDbContext();

        // GET: StockIn
        public ActionResult Index()
        {
            var records = db.StockIns
                .Include(s => s.Product)
                .Include(s => s.Supplier)
                .OrderByDescending(s => s.StockInDate)
                .ToList();

            return View(records);
        }

        // GET: StockIn/Create
        public ActionResult Create()
        {
            var model = new StockInViewModel
            {
                ProductList = BuildProductList(),
                SupplierList = BuildSupplierList()
            };
            return View(model);
        }

        // POST: StockIn/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StockInViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Use a database transaction so the StockIn record and the Product.CurrentStock update either BOTH succeed or BOTH fail.
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var product = db.Products.Find(model.ProductId);
                        if (product == null)
                        {
                            ModelState.AddModelError("", "Selected product no longer exists.");
                            model.ProductList = BuildProductList();
                            model.SupplierList = BuildSupplierList();
                            return View(model);
                        }

                        var stockIn = new StockIn
                        {
                            ProductId = model.ProductId,
                            SupplierId = model.SupplierId,
                            Quantity = model.Quantity,
                            UnitCost = model.UnitCost,
                            TotalCost = model.Quantity * model.UnitCost, // TotalCost = Quantity x UnitCost
                            StockInDate = DateTime.Now,
                            ReferenceNo = model.ReferenceNo,
                            Remarks = model.Remarks
                        };

                        db.StockIns.Add(stockIn);

                        // Core business rule: New Stock = Current Stock + Quantity
                        product.CurrentStock += model.Quantity;

                        db.SaveChanges();
                        transaction.Commit();

                        TempData["Success"] = "Stock In transaction saved successfully.";
                        return RedirectToAction("Index");
                    }
                    catch
                    {
                        transaction.Rollback();
                        TempData["Error"] = "Something went wrong. Please try again.";
                    }
                }
            }

            model.ProductList = BuildProductList();
            model.SupplierList = BuildSupplierList();
            return View(model);
        }

        // GET: StockIn/Details/5 
        public ActionResult Details(int? id)
        {
            if (id == null) return HttpNotFound();

            var record = db.StockIns
                .Include(s => s.Product)
                .Include(s => s.Supplier)
                .FirstOrDefault(s => s.StockInId == id);

            if (record == null) return HttpNotFound();

            return View(record);
        }

        private SelectList BuildProductList()
        {
            return new SelectList(db.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName), "ProductId", "ProductName");
        }

        private SelectList BuildSupplierList()
        {
            return new SelectList(db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.SupplierName), "SupplierId", "SupplierName");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}