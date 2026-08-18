using System;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using SmartStock.Data;
using SmartStock.Helpers;
using SmartStock.Models;
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

            // Build a Forms Authentication ticket that carries the user's Role
            // in UserData. This is what lets [Authorize(Roles="Admin")] work later,
            // since the role needs to travel inside the encrypted cookie itself.
            var ticket = new FormsAuthenticationTicket(
                1,                              // version
                user.Username,                  // name (becomes User.Identity.Name)
                DateTime.Now,                   // issue date
                DateTime.Now.AddMinutes(60),    // expiry - matches Web.config timeout
                false,                          // persistent cookie
                user.Role                       // <-- custom data: the user's role
            );

            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket)
            {
                HttpOnly = true // JavaScript can't read this cookie - reduces XSS risk
            };
            Response.Cookies.Add(authCookie);

            // FullName is still convenient to keep in Session for display purposes
            Session["FullName"] = user.FullName;
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

        // GET: /Account/Register  (Admin only - creates "User" accounts)
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Register()
        {
            var model = new RegisterViewModel
            {
                Role = "User",
                RoleList = BuildRoleList()
            };
            return View(model);
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [CustomAuthorize(Roles = "Admin")]
        public ActionResult Register(RegisterViewModel model)
        {
            if (db.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "This username is already taken.");
            }

            if (ModelState.IsValid)
            {
                var newUser = new User
                {
                    Username = model.Username,
                    Password = PasswordHelper.HashPassword(model.Password),
                    FullName = model.FullName,
                    // Role is ALWAYS hardcoded to "User" here, regardless of what
                    // was posted from the browser. Never trust the client for
                    // anything security-sensitive like role assignment.
                    Role = "User",
                    IsActive = true,
                    CreatedDate = DateTime.Now
                };

                db.Users.Add(newUser);
                db.SaveChanges();

                TempData["Success"] = "User account \"" + newUser.Username + "\" created successfully. Share the username and password with them securely.";
                return RedirectToAction("Register");
            }

            model.RoleList = BuildRoleList();
            return View(model);
        }

        // GET: /Account/AccessDenied
        [AllowAnonymous]
        public ActionResult AccessDenied()
        {
            return View();
        }

        private System.Web.Mvc.SelectList BuildRoleList()
        {
            // Only "User" can ever be created through Sign-Up. There is
            // intentionally only ever one Admin (the seeded account) - creating
            // more Admins through a web form would be a privilege-escalation risk.
            return new System.Web.Mvc.SelectList(new[] { "User" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}