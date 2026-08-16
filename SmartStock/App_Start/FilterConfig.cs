using System.Web;
using System.Web.Mvc;

namespace SmartStock
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeAttribute()); // Require login for ALL actions by default
        }
    }
}
