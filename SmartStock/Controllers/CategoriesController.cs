using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SmartStock.Data;
using SmartStock.Models;

namespace SmartStock.Controllers
{
    public class CategoriesController : Controller
    {
        private SmartStockDbContext db = new SmartStockDbContext();

        // GET: Categories
        public ActionResult Index()
        {
            var categories = db.Categories.Where(c => c.IsActive).OrderBy(c => c.CategoryName).ToList();
            return View(categories);
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            return View(new Category { IsActive = true });
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                category.IsActive = true;
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Category updated successfully.";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var category = db.Categories.Find(id);
            if (category == null) return HttpNotFound();

            bool hasProducts = db.Products.Any(p => p.CategoryId == id);
            if (hasProducts)
            {
                TempData["Error"] = "Category cannot be deleted because it has products assigned to it.";
                return RedirectToAction("Index");
            }

            db.Categories.Remove(category);
            db.SaveChanges();
            TempData["Success"] = "Category deleted successfully.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}