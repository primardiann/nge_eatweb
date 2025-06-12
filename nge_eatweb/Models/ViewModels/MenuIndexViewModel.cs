using System.Collections.Generic;

namespace nge_eatweb.ViewModels
{
    public class MenuIndexViewModel
    {
        public IEnumerable<MenuFormViewModel> Menus { get; set; } // GANTI KE MenuFormViewModel biar cocok sama isi controller
        public MenuFormViewModel FormModel { get; set; }

        // Pagination
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
    }
}
