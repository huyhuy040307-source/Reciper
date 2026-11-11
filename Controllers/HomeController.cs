using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using PagedList;
using Reciper.Models;
using Reciper.Models.ViewModel;

namespace Reciper.Controllers
{
    public class HomeController : Controller
    {
        private ReciperEntities db = new ReciperEntities();

        // Trang chủ người dùng: Khám phá công thức nấu ăn
        public ActionResult Index(string searchTerm, int? categoryId, int? page)
        {
            var model = new HomeRecipeVM();
            model.SearchTerm = searchTerm;
            model.CategoryId = categoryId;
            model.Categories = db.categories.OrderBy(c => c.name).ToList();

            var recipes = db.recipes.Include(r => r.category).Include(r => r.user).AsQueryable();

            // Lọc theo từ khóa tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                recipes = recipes.Where(r =>
                    r.title.Contains(searchTerm) ||
                    r.description.Contains(searchTerm) ||
                    r.difficulty.Contains(searchTerm) ||
                    (r.category != null && r.category.name.Contains(searchTerm)));
            }

            // Lọc theo danh mục nếu người dùng bấm chọn danh mục
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                recipes = recipes.Where(r => r.category_id == categoryId.Value);
            }

            // Top công thức nổi bật (lấy 8 món)
            model.FeaturedRecipes = db.recipes
                .Include(r => r.category)
                .OrderByDescending(r => r.id)
                .Take(8)
                .ToList();

            // Công thức mới nhất có phân trang (6 món mỗi trang)
            int pageNumber = page ?? 1;
            int pageSize = 6;
            model.NewRecipes = recipes
                .OrderByDescending(r => r.created_at)
                .ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        // Chi tiết công thức món ăn (tương tự ProductDetails trong bài mẫu)
        public ActionResult RecipeDetails(int? id, int? servings, int? page)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            recipe rec = db.recipes
                .Include(r => r.category)
                .Include(r => r.user)
                .Include(r => r.recipe_ingredients.Select(ri => ri.ingredient))
                .Include(r => r.steps)
                .FirstOrDefault(r => r.id == id);

            if (rec == null)
            {
                return HttpNotFound();
            }

            var model = new RecipeDetailsVM();
            model.Recipe = rec;
            model.Servings = (servings.HasValue && servings.Value > 0) ? servings.Value : (rec.servings > 0 ? (int)rec.servings : 2);

            // Các món ăn liên quan (cùng danh mục món ăn)
            model.RelatedRecipes = db.recipes
                .Include(r => r.category)
                .Where(r => r.category_id == rec.category_id && r.id != rec.id)
                .OrderByDescending(r => r.id)
                .Take(4)
                .ToList();

            // Các món ăn gợi ý thêm khác có phân trang
            int pageNumber = page ?? 1;
            int pageSize = 4;
            model.TopRecipes = db.recipes
                .Include(r => r.category)
                .Where(r => r.id != rec.id)
                .OrderBy(r => r.id)
                .ToPagedList(pageNumber, pageSize);

            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
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
