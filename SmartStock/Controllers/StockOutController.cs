using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using SmartStock.Data;
using SmartStock.Models;
using SmartStock.ViewModels;

namespace SmartStock.Controllers
{
    public class StockOutController : Controller
    {
        private SmartStockDbContext db = new SmartStockDbContext();

        // GET: StockOut
        public ActionResult Index()
        {
            var records = db.StockOuts
                .Include(s => s.Product)
                .OrderByDescending(s => s.StockOutDate)
                .ToList();

            return View(records);
        }

        // GET: StockOut/Create
        public ActionResult Create()
        {
            var model = new StockOutViewModel
            {
                ProductList = BuildProductList()
            };
            return View(model);
        }

        // POST: StockOut/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StockOutViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        // Always re-fetch the product from the DB to get the true, current
                        // stock level - never trust a value that might have been posted from the client.
                        var product = db.Products.Find(model.ProductId);
                        if (product == null)
                        {
                            ModelState.AddModelError("", "Selected product no longer exists.");
                            model.ProductList = BuildProductList();
                            return View(model);
                        }

                        // Core business rule: Requested Quantity must not exceed Current Stock
                        if (model.Quantity > product.CurrentStock)
                        {
                            ModelState.AddModelError("Quantity", "Insufficient stock available.");
                            model.ProductList = BuildProductList();
                            ViewBag.AvailableStock = product.CurrentStock;
                            return View(model);
                        }

                        var stockOut = new StockOut
                        {
                            ProductId = model.ProductId,
                            Quantity = model.Quantity,
                            StockOutDate = DateTime.Now,
                            Purpose = model.Purpose,
                            ReferenceNo = model.ReferenceNo,
                            Remarks = model.Remarks
                        };

                        db.StockOuts.Add(stockOut);

                        // Core business rule: New Stock = Current Stock - Quantity
                        product.CurrentStock -= model.Quantity;

                        db.SaveChanges();
                        transaction.Commit();

                        TempData["Success"] = "Stock Out transaction saved successfully.";
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
            return View(model);
        }

        // GET: StockOut/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) return HttpNotFound();

            var record = db.StockOuts
                .Include(s => s.Product)
                .FirstOrDefault(s => s.StockOutId == id);

            if (record == null) return HttpNotFound();

            return View(record);
        }

        // GET: StockOut/GetAvailableStock?productId=5
        // AJAX endpoint used by the Create view to show live available stock
        [HttpGet]
        public JsonResult GetAvailableStock(int productId)
        {
            var product = db.Products.Find(productId);
            int stock = product != null ? product.CurrentStock : 0;
            return Json(new { availableStock = stock }, JsonRequestBehavior.AllowGet);
        }

        private SelectList BuildProductList()
        {
            return new SelectList(db.Products.Where(p => p.IsActive).OrderBy(p => p.ProductName), "ProductId", "ProductName");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}