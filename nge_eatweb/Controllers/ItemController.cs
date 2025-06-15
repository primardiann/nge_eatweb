using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using nge_eatweb.Models;
using System;
using System.Collections.Generic;

namespace nge_eatweb.Controllers
{
    public class ItemController : Controller
    {
        private readonly IConfiguration _configuration;

        public ItemController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index(string kategori, int page = 1)
        {
            int pageSize = 10;
            var itemList = new List<ItemTerjualViewModel>();
            var kategoriList = new List<string>();
            int totalItems = 0;

            using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();

                // Ambil semua kategori unik
                var kategoriCmd = new SqlCommand("SELECT DISTINCT kategori FROM items", conn);
                using (var reader = kategoriCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        kategoriList.Add(reader["kategori"].ToString());
                    }
                }

                // Hitung total item untuk pagination
                var countCmd = new SqlCommand(@"
                    SELECT COUNT(*) FROM (
                        SELECT i.id_item
                        FROM items i
                        LEFT JOIN transaksi t ON i.id_item = t.id_item
                        WHERE (@kategori IS NULL OR i.kategori = @kategori)
                        GROUP BY i.id_item
                    ) AS CountedItems", conn);
                countCmd.Parameters.AddWithValue("@kategori", string.IsNullOrEmpty(kategori) ? DBNull.Value : (object)kategori);
                totalItems = (int)countCmd.ExecuteScalar();

                // Ambil data item terjual dengan pagination
                var dataCmd = new SqlCommand(@"
                    SELECT 
                        i.id_item,
                        i.nama_item,
                        i.kategori,
                        i.harga,
                        ISNULL(SUM(t.jumlah), 0) AS jumlah_terjual
                    FROM items i
                    LEFT JOIN transaksi t ON i.id_item = t.id_item
                    WHERE (@kategori IS NULL OR i.kategori = @kategori)
                    GROUP BY i.id_item, i.nama_item, i.kategori, i.harga
                    ORDER BY jumlah_terjual DESC
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY", conn);

                dataCmd.Parameters.AddWithValue("@kategori", string.IsNullOrEmpty(kategori) ? DBNull.Value : (object)kategori);
                dataCmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
                dataCmd.Parameters.AddWithValue("@pageSize", pageSize);

                using (var reader = dataCmd.ExecuteReader())
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

            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.KategoriList = kategoriList;
            ViewBag.SelectedKategori = kategori;

            return View(itemList);
        }
    }
}
