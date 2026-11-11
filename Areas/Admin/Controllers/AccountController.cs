using System.Web.Mvc;

namespace Reciper.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        // Điều hướng toàn bộ về trang đăng nhập chung duy nhất của hệ thống
        public ActionResult Login()
        {
            return RedirectToAction("Login", "Account", new { area = "" });
        }

        public ActionResult Logout()
        {
            return RedirectToAction("Logout", "Account", new { area = "" });
        }

        public ActionResult Register()
        {
            return RedirectToAction("Register", "Account", new { area = "" });
        }
    }
}
