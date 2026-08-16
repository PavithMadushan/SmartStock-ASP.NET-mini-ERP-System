using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartStock.Data;
using SmartStock.Models;

namespace SmartStock.Controllers
{
    public class SuppliersController : Controller
    {
        private SmartStockDbContext db = new SmartStockDbContext();

        // GET: Suppliers
        public ActionResult Index()
        {
            var suppliers = db.Suppliers.Where(s => s.IsActive).OrderBy(s => s.SupplierName).ToList();
            return View(suppliers);
        }

        // GET: Suppliers/Create
        public ActionResult Create()
        {
            return View(new Supplier { IsActive = true });
        }

        // POST: Suppliers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                supplier.IsActive = true;
                db.Suppliers.Add(supplier);
                db.SaveChanges();
                TempData["Success"] = "Supplier created successfully.";
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        // GET: Suppliers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var supplier = db.Suppliers.Find(id);
            if (supplier == null) return HttpNotFound();

            return View(supplier);
        }

        // POST: Suppliers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Supplier supplier)
        {
            if (ModelState.IsValid)
            {
                db.Entry(supplier).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Supplier updated successfully.";
                return RedirectToAction("Index");
            }
            return View(supplier);
        }

        // GET: Suppliers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var supplier = db.Suppliers.Find(id);
            if (supplier == null) return HttpNotFound();

            return View(supplier);
        }

        // POST: Suppliers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var supplier = db.Suppliers.Find(id);
            if (supplier == null) return HttpNotFound();

            bool hasStockIns = db.StockIns.Any(s => s.SupplierId == id);
            if (hasStockIns)
            {
                TempData["Error"] = "Supplier cannot be deleted because it has stock-in transaction history.";
                return RedirectToAction("Index");
            }

            db.Suppliers.Remove(supplier);
            db.SaveChanges();
            TempData["Success"] = "Supplier deleted successfully.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}