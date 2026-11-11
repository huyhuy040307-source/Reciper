using System;
using System.Collections.Generic;
using PagedList;
using Reciper.Models;

namespace Reciper.Models.ViewModel
{
    public class RecipeDetailsVM
    {
        // Công thức chi tiết (kèm category, user, ingredients, steps)
        public recipe Recipe { get; set; }

        // Khẩu phần muốn tính toán (mặc định bằng khẩu phần gốc của món)
        public int Servings { get; set; } = 1;

        // Các công thức cùng danh mục (món liên quan)
        public List<recipe> RelatedRecipes { get; set; }

        // Các công thức nổi bật / gợi ý thêm
        public IPagedList<recipe> TopRecipes { get; set; }
    }
}

