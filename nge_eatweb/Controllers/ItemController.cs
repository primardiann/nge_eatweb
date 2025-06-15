using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using nge_eatweb.Models;
using System.Collections.Generic;
using System.Linq;

namespace nge_eatweb.Controllers
{
    public class ItemController : Controller
    {
        private readonly IConfiguration _configuration;

        public ItemController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(int page = 1)
        {
            int pageSize = 10;
            var itemList = new List<ItemTerjualViewModel>();
            var connStr = _configuration.GetConnectionString("DefaultConnection");

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                var query = @"
                    SELECT 
                        i.id_item,
                        i.nama_item,
                        i.kategori,
                        i.harga,
                        ISNULL(SUM(t.jumlah), 0) AS jumlah_terjual
                    FROM items i
                    LEFT JOIN transaksi t ON i.id_item = t.id_item
                    GROUP BY 
                        i.id_item, i.nama_item, i.kategori, i.harga
                    ORDER BY jumlah_terjual DESC";

                using (var cmd = new SqlCommand(query, conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            itemList.Add(new ItemTerjualViewModel
                            {
                                IdItem = (int)reader["id_item"],
                                NamaItem = reader["nama_item"].ToString(),
                                Kategori = reader["kategori"].ToString(),
                                Harga = Convert.ToDecimal(reader["harga"]),
                                JumlahTerjual = Convert.ToInt32(reader["jumlah_terjual"])
                            });
                        }
                    }
                }
            }

            int totalItems = itemList.Count;
            var pagedItems = itemList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentPage = page;

            return View(pagedItems);
        }
    }
}
