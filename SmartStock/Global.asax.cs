using System;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace SmartStock
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        // Runs on every request. Reads the Forms Authentication cookie (if present),
        // pulls the Role we stored in UserData during Login, and attaches it to
        // HttpContext.User so that [Authorize(Roles = "Admin")] / User.IsInRole(...)
        // work correctly throughout the app.
        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie != null)
            {
                FormsAuthenticationTicket authTicket = null;
                try
                {
                    authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                }
                catch
                {
                    // Corrupted/tampered cookie - treat as not authenticated
                    return;
                }

                if (authTicket != null && !authTicket.Expired)
                {
                    string[] roles = authTicket.UserData.Split(',');
                    FormsIdentity identity = new FormsIdentity(authTicket);
                    GenericPrincipal principal = new GenericPrincipal(identity, roles);
                    Context.User = principal;
                }
            }
        }
    }
}