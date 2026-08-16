using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartStock.Data;
using SmartStock.Models;
using SmartStock.ViewModels;

namespace SmartStock.Controllers
{
    public class ProductsController : Controller
    {
        private SmartStockDbContext db = new SmartStockDbContext();

        // GET: Products
        public ActionResult Index(string searchTerm, int? categoryId)
        {
            var products = db.Products.Include(p => p.Category).Where(p => p.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                products = products.Where(p =>
                    p.ProductName.Contains(searchTerm) ||
                    p.ProductCode.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == categoryId.Value);
            }

            ViewBag.Categories = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryId", "CategoryName", categoryId);
            ViewBag.SearchTerm = searchTerm;

            return View(products.OrderBy(p => p.ProductName).ToList());
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            var model = new ProductViewModel
            {
                CategoryList = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryId", "CategoryName")
            };
            return View(model);
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductViewModel model)
        {
            if (db.Products.Any(p => p.ProductCode == model.ProductCode))
            {
                ModelState.AddModelError("ProductCode", "This product code already exists.");
            }

            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    ProductCode = model.ProductCode,
                    ProductName = model.ProductName,
                    CategoryId = model.CategoryId,
                    UnitPrice = model.UnitPrice,
                    ReorderLevel = model.ReorderLevel,
                    CurrentStock = 0, // New products ALWAYS start at zero stock - never taken from the form
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Product created successfully.";
                return RedirectToAction("Index");
            }

            model.CategoryList = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryId", "CategoryName", model.CategoryId);
            return View(model);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            var model = new ProductViewModel
            {
                ProductId = product.ProductId,
                ProductCode = product.ProductCode,
                ProductName = product.ProductName,
                CategoryId = product.CategoryId,
                UnitPrice = product.UnitPrice,
                ReorderLevel = product.ReorderLevel,
                CurrentStock = product.CurrentStock,
                CategoryList = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryId", "CategoryName", product.CategoryId)
            };

            return View(model);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductViewModel model)
        {
            if (db.Products.Any(p => p.ProductCode == model.ProductCode && p.ProductId != model.ProductId))
            {
                ModelState.AddModelError("ProductCode", "This product code already exists.");
            }

            if (ModelState.IsValid)
            {
                var product = db.Products.Find(model.ProductId);
                if (product == null) return HttpNotFound();

                // Only editable fields are updated here.
                // CurrentStock is intentionally NEVER set from this form -
                // it is only ever changed by StockIn/StockOut transactions (Phase 4).
                product.ProductCode = model.ProductCode;
                product.ProductName = model.ProductName;
                product.CategoryId = model.CategoryId;
                product.UnitPrice = model.UnitPrice;
                product.ReorderLevel = model.ReorderLevel;

                db.SaveChanges();
                TempData["Success"] = "Product updated successfully.";
                return RedirectToAction("Index");
            }

            model.CategoryList = new SelectList(db.Categories.Where(c => c.IsActive), "CategoryId", "CategoryName", model.CategoryId);
            return View(model);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductId == id);
            if (product == null) return HttpNotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            bool hasTransactions = db.StockIns.Any(s => s.ProductId == id) || db.StockOuts.Any(s => s.ProductId == id);

            if (hasTransactions)
            {
                // Business rule: products with transaction history can't be hard-deleted
                // (it would corrupt StockIn/StockOut records). Deactivate instead.
                product.IsActive = false;
                db.SaveChanges();
                TempData["Success"] = "Product cannot be deleted because it has transaction history, so it was deactivated instead.";
                return RedirectToAction("Index");
            }

            db.Products.Remove(product);
            db.SaveChanges();
            TempData["Success"] = "Product deleted successfully.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}