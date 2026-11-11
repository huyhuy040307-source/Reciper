using System;
using System.Collections.Generic;
using PagedList;
using Reciper.Models;

namespace Reciper.Models.ViewModel
{
    public class HomeRecipeVM
    {
        // Tiêu chí tìm kiếm
        public string SearchTerm { get; set; }
        public int? CategoryId { get; set; }

        // Danh sách danh mục để người dùng lọc theo loại món ăn
        public List<category> Categories { get; set; }

        // Danh sách công thức nổi bật (slider / hàng ngang)
        public List<recipe> FeaturedRecipes { get; set; }

        // Danh sách công thức mới nhất có phân trang
        public IPagedList<recipe> NewRecipes { get; set; }
    }
}

