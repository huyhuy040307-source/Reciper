using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Reciper.Models;

namespace Reciper.Areas.Admin.Controllers
{
    public class UsersController : BaseController
    {
        private ReciperEntities db = new ReciperEntities();

        private void PopulateRoles(string selectedRole = "User")
        {
            var roles = new List<string> { "User", "Admin" };
            ViewBag.RoleList = new SelectList(roles, selectedRole);
        }

        // GET: Admin/Users
        public ActionResult Index(string searchTerm)
        {
            var users = db.users.AsQueryable();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                users = users.Where(u => u.username.Contains(searchTerm) ||
                                         u.full_name.Contains(searchTerm) ||
                                         u.email.Contains(searchTerm));
                ViewBag.SearchTerm = searchTerm;
            }
            return View(users.OrderByDescending(u => u.id).ToList());
        }

        // GET: Admin/Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Admin/Users/Create
        public ActionResult Create()
        {
            PopulateRoles("User");
            return View(new user { is_active = true, role = "User" });
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,username,email,password_hash,full_name,role,is_active")] user user)
        {
            if (ModelState.IsValid)
            {
                if (db.users.Any(u => u.username.ToLower() == user.username.ToLower()))
                {
                    ModelState.AddModelError("username", "Tên đăng nhập đã tồn tại!");
                    PopulateRoles(user.role);
                    return View(user);
                }

                user.created_at = DateTime.Now;
                db.users.Add(user);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            PopulateRoles(user.role);
            return View(user);
        }

        // GET: Admin/Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            PopulateRoles(user.role);
            return View(user);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,username,email,password_hash,full_name,role,is_active,created_at")] user user)
        {
            if (ModelState.IsValid)
            {
                if (db.users.Any(u => u.username.ToLower() == user.username.ToLower() && u.id != user.id))
                {
                    ModelState.AddModelError("username", "Tên đăng nhập đã tồn tại!");
                    PopulateRoles(user.role);
                    return View(user);
                }

                if (user.created_at == default(DateTime))
                {
                    user.created_at = DateTime.Now;
                }
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            PopulateRoles(user.role);
            return View(user);
        }

        // GET: Admin/Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            user user = db.users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            int currentUserId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (user.id == currentUserId)
            {
                TempData["Error"] = "Bạn không thể xóa chính tài khoản đang đăng nhập!";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            int currentUserId = Session["UserId"] != null ? (int)Session["UserId"] : 0;
            if (id == currentUserId)
            {
                TempData["Error"] = "Bạn không thể xóa chính tài khoản đang đăng nhập!";
                return RedirectToAction("Index");
            }

            user user = db.users.Find(id);
            if (user != null)
            {
                // Tháo liên kết recipe của user để không bị lỗi khóa ngoại
                var recipes = db.recipes.Where(r => r.user_id == id).ToList();
                foreach (var r in recipes)
                {
                    r.user_id = null;
                }
                db.users.Remove(user);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
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
