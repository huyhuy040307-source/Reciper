using Reciper.Models;
using Reciper.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Reciper.Areas.Admin.Controllers
{
    public class RecipesController : BaseController
    {
        private ReciperEntities db = new ReciperEntities();

        // GET: Admin/Recipes
        public ActionResult Index(string searchTerm, int? minCookMinutes, int? maxCookMinutes, string sortOrder, int? page)
        {
            var model = new RecipeSearchVM();
            var recipes = db.recipes.Include(r => r.category).Include(r => r.user).AsQueryable();

            // Tìm kiếm công thức dựa trên từ khóa
            if (!string.IsNullOrEmpty(searchTerm))
            {
                model.SearchTerm = searchTerm;
                recipes = recipes.Where(r => r.title.Contains(searchTerm) ||
                                             r.description.Contains(searchTerm) ||
                                             r.category.name.Contains(searchTerm) ||
                                             r.difficulty.Contains(searchTerm));
            }

            // Tìm kiếm theo thời gian nấu tối thiểu
            if (minCookMinutes.HasValue)
            {
                model.MinCookMinutes = minCookMinutes.Value;
                recipes = recipes.Where(r => r.cook_minutes >= minCookMinutes.Value);
            }

            // Tìm kiếm theo thời gian nấu tối đa
            if (maxCookMinutes.HasValue)
            {
                model.MaxCookMinutes = maxCookMinutes.Value;
                recipes = recipes.Where(r => r.cook_minutes <= maxCookMinutes.Value);
            }

            // Sắp xếp
            switch (sortOrder)
            {
                case "name_asc":
                    recipes = recipes.OrderBy(r => r.title);
                    break;
                case "name_desc":
                    recipes = recipes.OrderByDescending(r => r.title);
                    break;
                case "time_asc":
                    recipes = recipes.OrderBy(r => r.cook_minutes);
                    break;
                case "time_desc":
                    recipes = recipes.OrderByDescending(r => r.cook_minutes);
                    break;
                default:
                    recipes = recipes.OrderByDescending(r => r.id);
                    break;
            }

            model.SortOrder = sortOrder;

            // Phân trang
            int pageNumber = page ?? 1;
            int pageSize = 7;
            int totalItems = recipes.Count();

            model.PageNumber = pageNumber;
            model.PageSize = pageSize;
            model.TotalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            model.Recipes = recipes.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return View(model);
        }

        // GET: Admin/Recipes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            recipe recipe = db.recipes
                .Include(r => r.category)
                .Include(r => r.user)
                .Include(r => r.recipe_ingredients.Select(ri => ri.ingredient))
                .Include(r => r.steps)
                .FirstOrDefault(r => r.id == id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        // GET: Admin/Recipes/Create
        public ActionResult Create()
        {
            ViewBag.category_id = new SelectList(db.categories, "id", "name");
            ViewBag.user_id = new SelectList(db.users, "id", "full_name");
            var model = new recipe
            {
                servings = 4,
                prep_minutes = 15,
                cook_minutes = 30,
                difficulty = "Trung bình",
                status = "published"
            };
            return View(model);
        }

        // POST: Admin/Recipes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "id,category_id,title,description,servings,prep_minutes,cook_minutes,difficulty,image_url,user_id,status")] recipe recipe,
            string[] ing_name, string[] ing_qty, string[] ing_unit, string[] ing_note,
            string[] step_instruction)
        {
            // 1. Tự động thiết lập giá trị hệ thống trước khi kiểm tra
            recipe.created_at = DateTime.Now;
            if (string.IsNullOrEmpty(recipe.status))
            {
                recipe.status = "published";
            }

            // 2. Bỏ qua kiểm tra trường created_at trong ModelState
            ModelState.Remove("created_at");

            // 3. Kiểm tra logic bổ sung (chặn số âm, rỗng)
            if (string.IsNullOrWhiteSpace(recipe.title))
            {
                ModelState.AddModelError("title", "Tên công thức không được để trống.");
            }
            if (recipe.servings <= 0)
            {
                ModelState.AddModelError("servings", "Khẩu phần phải lớn hơn 0.");
            }
            if (recipe.prep_minutes < 0)
            {
                ModelState.AddModelError("prep_minutes", "Thời gian chuẩn bị không được âm.");
            }
            if (recipe.cook_minutes < 0)
            {
                ModelState.AddModelError("cook_minutes", "Thời gian nấu không được âm.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.recipes.Add(recipe);
                    db.SaveChanges();

                    // Lưu nguyên liệu và các bước nấu
                    SaveIngredientsAndSteps(recipe.id, ing_name, ing_qty, ing_unit, ing_note, step_instruction);

                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    var errors = ex.EntityValidationErrors
                                   .SelectMany(x => x.ValidationErrors)
                                   .Select(x => x.PropertyName + ": " + x.ErrorMessage);
                    ViewBag.Error = "Lỗi CSDL: " + string.Join("; ", errors);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Lỗi hệ thống: " + (ex.InnerException?.InnerException?.Message ?? ex.Message);
                }
            }

            ViewBag.category_id = new SelectList(db.categories, "id", "name", recipe.category_id);
            ViewBag.user_id = new SelectList(db.users, "id", "full_name", recipe.user_id);
            return View(recipe);
        }

        // GET: Admin/Recipes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            recipe recipe = db.recipes
                .Include(r => r.category)
                .Include(r => r.user)
                .Include(r => r.recipe_ingredients.Select(ri => ri.ingredient))
                .Include(r => r.steps)
                .FirstOrDefault(r => r.id == id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            ViewBag.category_id = new SelectList(db.categories, "id", "name", recipe.category_id);
            ViewBag.user_id = new SelectList(db.users, "id", "full_name", recipe.user_id);
            return View(recipe);
        }

        // POST: Admin/Recipes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "id,category_id,title,description,servings,prep_minutes,cook_minutes,difficulty,image_url,user_id,status,created_at")] recipe recipe,
            string[] ing_name, string[] ing_qty, string[] ing_unit, string[] ing_note,
            string[] step_instruction)
        {
            ModelState.Remove("created_at");

            if (string.IsNullOrWhiteSpace(recipe.title))
            {
                ModelState.AddModelError("title", "Tên công thức không được để trống.");
            }
            if (recipe.servings <= 0)
            {
                ModelState.AddModelError("servings", "Khẩu phần phải lớn hơn 0.");
            }
            if (recipe.prep_minutes < 0)
            {
                ModelState.AddModelError("prep_minutes", "Thời gian chuẩn bị không được âm.");
            }
            if (recipe.cook_minutes < 0)
            {
                ModelState.AddModelError("cook_minutes", "Thời gian nấu không được âm.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(recipe).State = EntityState.Modified;

                    // Giữ nguyên ngày tạo gốc ban đầu, không cho phép bị ghi đè thành giá trị rỗng
                    if (recipe.created_at == default(DateTime))
                    {
                        db.Entry(recipe).Property(x => x.created_at).IsModified = false;
                    }

                    db.SaveChanges();

                    // Cập nhật nguyên liệu và các bước nấu
                    SaveIngredientsAndSteps(recipe.id, ing_name, ing_qty, ing_unit, ing_note, step_instruction);

                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    var errors = ex.EntityValidationErrors
                                   .SelectMany(x => x.ValidationErrors)
                                   .Select(x => x.PropertyName + ": " + x.ErrorMessage);
                    ViewBag.Error = "Lỗi CSDL: " + string.Join("; ", errors);
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Lỗi hệ thống: " + (ex.InnerException?.InnerException?.Message ?? ex.Message);
                }
            }

            ViewBag.category_id = new SelectList(db.categories, "id", "name", recipe.category_id);
            ViewBag.user_id = new SelectList(db.users, "id", "full_name", recipe.user_id);
            return View(recipe);
        }

        // GET: Admin/Recipes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            recipe recipe = db.recipes
                .Include(r => r.category)
                .Include(r => r.user)
                .Include(r => r.recipe_ingredients.Select(ri => ri.ingredient))
                .Include(r => r.steps)
                .FirstOrDefault(r => r.id == id);
            if (recipe == null)
            {
                return HttpNotFound();
            }
            return View(recipe);
        }

        // POST: Admin/Recipes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var oldIngredients = db.recipe_ingredients.Where(ri => ri.recipe_id == id).ToList();
            db.recipe_ingredients.RemoveRange(oldIngredients);

            var oldSteps = db.steps.Where(s => s.recipe_id == id).ToList();
            db.steps.RemoveRange(oldSteps);

            recipe recipe = db.recipes.Find(id);
            if (recipe != null)
            {
                db.recipes.Remove(recipe);
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // Hàm tiện ích: Lưu danh sách nguyên liệu và các bước làm
        private void SaveIngredientsAndSteps(int recipeId, string[] ing_name, string[] ing_qty, string[] ing_unit, string[] ing_note, string[] step_instruction)
        {
            // 1. Cập nhật nguyên liệu
            var oldIngredients = db.recipe_ingredients.Where(ri => ri.recipe_id == recipeId).ToList();
            db.recipe_ingredients.RemoveRange(oldIngredients);

            if (ing_name != null)
            {
                for (int i = 0; i < ing_name.Length; i++)
                {
                    var rawName = ing_name[i];
                    if (string.IsNullOrWhiteSpace(rawName)) continue;
                    var name = rawName.Trim();

                    var ing = db.ingredients.FirstOrDefault(x => x.name.ToLower() == name.ToLower());
                    if (ing == null)
                    {
                        ing = new ingredient { name = name };
                        db.ingredients.Add(ing);
                        db.SaveChanges();
                    }

                    decimal? qty = null;
                    if (ing_qty != null && i < ing_qty.Length && !string.IsNullOrWhiteSpace(ing_qty[i]))
                    {
                        string cleanQty = ing_qty[i].Trim().Replace(',', '.');
                        if (decimal.TryParse(cleanQty, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal parsedQty))
                        {
                            qty = parsedQty;
                        }
                    }

                    string unit = (ing_unit != null && i < ing_unit.Length) ? ing_unit[i]?.Trim() : null;
                    string note = (ing_note != null && i < ing_note.Length) ? ing_note[i]?.Trim() : null;

                    var existingRI = db.recipe_ingredients.Local.FirstOrDefault(ri => ri.recipe_id == recipeId && ri.ingredient_id == ing.id);
                    if (existingRI == null)
                    {
                        db.recipe_ingredients.Add(new recipe_ingredients
                        {
                            recipe_id = recipeId,
                            ingredient_id = ing.id,
                            quantity = qty,
                            unit = unit,
                            note = note
                        });
                    }
                }
            }

            // 2. Cập nhật các bước nấu ăn
            var oldSteps = db.steps.Where(s => s.recipe_id == recipeId).ToList();
            db.steps.RemoveRange(oldSteps);

            if (step_instruction != null)
            {
                byte stepNum = 1;
                for (int i = 0; i < step_instruction.Length; i++)
                {
                    var instr = step_instruction[i];
                    if (string.IsNullOrWhiteSpace(instr)) continue;

                    db.steps.Add(new step
                    {
                        recipe_id = recipeId,
                        step_number = stepNum++,
                        instruction = instr.Trim()
                    });
                }
            }

            db.SaveChanges();
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