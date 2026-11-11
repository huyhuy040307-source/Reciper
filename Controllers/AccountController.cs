using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity.Validation;
using Reciper.Models;

namespace Reciper.Controllers
{
    // Trang đăng nhập / đăng ký DUY NHẤT cho cả Admin và User
    public class AccountController : Controller
    {
        private ReciperEntities db = new ReciperEntities();

        // Kiểm tra role có phải Admin không (không phân biệt hoa thường, bỏ khoảng trắng)
        private static bool IsAdmin(string role)
        {
            return role != null && role.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        // Chuyển hướng theo quyền: Admin -> trang Admin, User -> trang người dùng
        private ActionResult RedirectByRole(string role)
        {
            if (IsAdmin(role))
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            // Đã đăng nhập rồi thì chuyển luôn về đúng trang
            if (Session["Username"] != null)
            {
                return RedirectByRole(Session["Role"] as string);
            }
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Vui lòng nhập tên đăng nhập và mật khẩu";
                return View();
            }

            // Tạo sẵn tài khoản admin/123456 nếu bảng users đang trống
            if (!db.users.Any())
            {
                db.users.Add(new user
                {
                    username = "admin",
                    password_hash = "123456",
                    full_name = "Quản trị viên",
                    email = "admin@gmail.com",
                    role = "Admin",
                    is_active = true,
                    created_at = DateTime.Now
                });
                db.SaveChanges();
            }

            var acc = db.users.FirstOrDefault(u => u.username == username && u.password_hash == password);
            if (acc == null)
            {
                ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
                return View();
            }

            if (!acc.is_active)
            {
                ViewBag.Error = "Tài khoản đã bị khóa!";
                return View();
            }

            // Lưu thông tin đăng nhập vào Session
            Session["UserId"] = acc.id;
            Session["Username"] = acc.username;
            Session["Role"] = acc.role;

            return RedirectByRole(acc.role);
        }

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "username,full_name,email,password_hash")] user model)
        {
            if (ModelState.IsValid)
            {
                if (db.users.Any(u => u.username == model.username))
                {
                    ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                    return View(model);
                }

                // Tài khoản đăng ký mới luôn là User thường
                model.role = "User";
                model.is_active = true;
                model.created_at = DateTime.Now;

                try
                {
                    db.users.Add(model);
                    db.SaveChanges();
                    return RedirectToAction("Login");
                }
                catch (DbEntityValidationException ex)
                {
                    var errors = ex.EntityValidationErrors
                                   .SelectMany(x => x.ValidationErrors)
                                   .Select(x => x.PropertyName + ": " + x.ErrorMessage);
                    ViewBag.Error = "Lỗi nhập liệu: " + string.Join("; ", errors);
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
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
