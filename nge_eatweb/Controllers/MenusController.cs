using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using nge_eatweb.ViewModels;

namespace nge_eatweb.Controllers
{
    public class MenusController : Controller
    {
        private readonly IConfiguration _configuration;

        public MenusController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 10;
            List<MenuFormViewModel> allMenus = new List<MenuFormViewModel>();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                string query = "SELECT id_item, nama_item, harga, kategori FROM items ORDER BY id_item";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        allMenus.Add(new MenuFormViewModel
                        {
                            Id = reader.GetInt32(0),
                            NamaItem = reader.GetString(1),
                            Harga = reader.GetDecimal(2),
                            Kategori = reader.GetString(3)
                        });
                    }
                }
            }

            int totalCount = allMenus.Count;
            var menus = allMenus
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var viewModel = new MenuIndexViewModel
            {
                Menus = menus,
                FormModel = new MenuFormViewModel(),
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(MenuFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                string query = @"INSERT INTO items (nama_item, harga, kategori)
                                 VALUES (@NamaItem, @Harga, @Kategori)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@NamaItem", model.NamaItem);
                    cmd.Parameters.AddWithValue("@Harga", model.Harga);
                    cmd.Parameters.AddWithValue("@Kategori", model.Kategori);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                string query = "DELETE FROM items WHERE id_item = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

        public IActionResult EditModal(int id)
        {
            MenuFormViewModel model = new MenuFormViewModel();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                string query = "SELECT id_item, nama_item, harga, kategori FROM items WHERE id_item = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            model.Id = reader.GetInt32(0);
                            model.NamaItem = reader.GetString(1);
                            model.Harga = reader.GetDecimal(2);
                            model.Kategori = reader.GetString(3);
                        }
                        else
                        {
                            return NotFound();
                        }
                    }
                }
            }

            return PartialView("_MenuFormPartial", model);
        }

        [HttpPost]
        public IActionResult Edit(MenuFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                string query = @"UPDATE items 
                                 SET nama_item = @NamaItem, harga = @Harga, kategori = @Kategori 
                                 WHERE id_item = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@NamaItem", model.NamaItem);
                    cmd.Parameters.AddWithValue("@Harga", model.Harga);
                    cmd.Parameters.AddWithValue("@Kategori", model.Kategori);
                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

    }
}
