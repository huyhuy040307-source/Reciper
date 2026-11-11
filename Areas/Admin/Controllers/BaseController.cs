using System;
using System.Web.Mvc;

namespace Reciper.Areas.Admin.Controllers
{
    // Các controller trong khu Admin kế thừa lớp này.
    // Chưa đăng nhập hoặc không phải Admin -> chuyển về trang đăng nhập.
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var role = Session["Role"] as string;
            bool isAdmin = role != null && role.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin)
            {
                filterContext.Result = new RedirectResult("~/Account/Login");
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}

