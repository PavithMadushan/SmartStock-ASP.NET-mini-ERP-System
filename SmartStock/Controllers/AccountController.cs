using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using SmartStock.Data;
using SmartStock.Helpers;
using SmartStock.ViewModels;

namespace SmartStock.Controllers
{
    public class AccountController : Controller
    {
        private SmartStockDbContext db = new SmartStockDbContext();

        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            // If already logged in, skip straight to dashboard
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Index", "Dashboard");
            }
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = db.Users.FirstOrDefault(u => u.Username == model.Username && u.IsActive);

            if (user == null || !PasswordHelper.VerifyPassword(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // Store username in the Forms Authentication cookie
            FormsAuthentication.SetAuthCookie(user.Username, false);

            // Store extra info (FullName, Role) in Session for easy access across the app
            Session["FullName"] = user.FullName;
            Session["Role"] = user.Role;
            Session["UserId"] = user.UserId;

            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: /Account/Logout
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Account");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}