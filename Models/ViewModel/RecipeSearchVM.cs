using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Reciper.Models.ViewModel
{
    public class RecipeSearchVM
    {
        // Tiêu chí để search theo tên, mô tả hoặc danh mục công thức
        public string SearchTerm { get; set; }

        //Các tiêu chí để search theo thời gian nấu (phút)
        public int? MinCookMinutes { get; set; }
        public int? MaxCookMinutes { get; set; }

        // Thứ tự sắp xếp
        public string SortOrder { get; set; }

        //Các thuộc tính hỗ trợ phân trang
        public int PageNumber { get; set; } //Trang hiện tại
        public int PageSize { get; set; } = 7; //Số bản ghi mỗi trang
        public int TotalPages { get; set; }

        //Danh sách công thức thỏa điều kiện tìm kiếm
        public List<recipe> Recipes { get; set; }
    }
}

