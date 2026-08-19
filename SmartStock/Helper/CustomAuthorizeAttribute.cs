using System.Web.Mvc;

namespace SmartStock.Helpers
{
    /// <summary>
    /// Behaves exactly like [Authorize], except that a user who IS logged in
    /// but doesn't have the required role is redirected to a friendly
    /// "Access Denied" page instead of being bounced back to the Login page
    /// </summary>
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                filterContext.Result = new RedirectResult("~/Account/AccessDenied");
            }
            else
            {
                base.HandleUnauthorizedRequest(filterContext);
            }
        }
    }
}