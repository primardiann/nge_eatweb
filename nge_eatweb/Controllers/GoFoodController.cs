using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using nge_eatweb.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nge_eatweb.Controllers
{
    public class GofoodController : Controller
    {
        private readonly IConfiguration _configuration;

        public GofoodController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var transaksiList = LoadTransaksiList();
            var (itemOptions, kategoriList) = LoadItemOptionsWithKategori();

            var viewModel = new GofoodTransaksiPageViewModel
            {
                TransaksiList = transaksiList,
                FormModel = new GofoodTransaksiFormViewModel
                {
                    ItemOptions = itemOptions,
                    KategoriList = kategoriList,
                    TanggalTransaksi = DateTime.Now,
                    Waktu = DateTime.Now.TimeOfDay,
                    IdPesanan = $"GFOOD{DateTime.Now:yyyyMMddHHmmss}",
                    ItemList = new List<ItemOrder> { new ItemOrder() }
                }
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(GofoodTransaksiFormViewModel model)
        {
            if (!ModelState.IsValid || model.ItemList == null || !model.ItemList.Any())
            {
                TempData["Error"] = "Data tidak valid. Mohon cek kembali inputan Anda.";

                var (itemOptions, kategoriList) = LoadItemOptionsWithKategori();
                model.ItemOptions = itemOptions;
                model.KategoriList = kategoriList;

                var vm = new GofoodTransaksiPageViewModel
                {
                    TransaksiList = LoadTransaksiList(),
                    FormModel = model
                };

                return View("Index", vm);
            }

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            var itemIds = model.ItemList.Select(i => i.IdItem).Distinct().ToList();
            if (!itemIds.Any())
            {
                TempData["Error"] = "Item harus diisi.";
                return RedirectToAction(nameof(Index));
            }

            var hargaPerItem = new Dictionary<int, decimal>();
            string queryHarga = $"SELECT id_item, harga FROM items WHERE id_item IN ({string.Join(",", itemIds)})";

            using (var cmd = new SqlCommand(queryHarga, conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    hargaPerItem[reader.GetInt32(0)] = reader.GetDecimal(1);
                }
            }

            foreach (var item in model.ItemList)
            {
                if (!hargaPerItem.TryGetValue(item.IdItem, out var harga))
                {
                    TempData["Error"] = $"Item dengan ID {item.IdItem} tidak ditemukan.";
                    return RedirectToAction(nameof(Index));
                }

                decimal totalItem = item.Jumlah * harga;

                string queryInsert = @"
                    INSERT INTO transaksi 
                    (id_pesanan, id_item, tanggal_transaksi, waktu, metode, nama_pelanggan, total)
                    VALUES (@IdPesanan, @IdItem, @Tanggal, @Waktu, @Metode, @NamaPelanggan, @Total)";

                using var cmdInsert = new SqlCommand(queryInsert, conn);
                cmdInsert.Parameters.AddWithValue("@IdPesanan", model.IdPesanan);
                cmdInsert.Parameters.AddWithValue("@IdItem", item.IdItem);
                cmdInsert.Parameters.AddWithValue("@Tanggal", model.TanggalTransaksi);
                cmdInsert.Parameters.AddWithValue("@Waktu", model.Waktu);
                cmdInsert.Parameters.AddWithValue("@Metode", model.Metode);
                cmdInsert.Parameters.AddWithValue("@NamaPelanggan", model.NamaPelanggan);
                cmdInsert.Parameters.AddWithValue("@Total", totalItem);

                cmdInsert.ExecuteNonQuery();
            }

            TempData["Success"] = "Transaksi berhasil ditambahkan.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(string idPesanan)
        {
            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            string query = "DELETE FROM transaksi WHERE id_pesanan = @IdPesanan";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@IdPesanan", idPesanan);
            cmd.ExecuteNonQuery();

            TempData["Success"] = "Transaksi berhasil dihapus.";
            return RedirectToAction(nameof(Index));
        }

        private (List<SelectListItem>, List<string>) LoadItemOptionsWithKategori()
        {
            var itemOptions = new List<SelectListItem>();
            var kategoriSet = new HashSet<string>();

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            string queryItems = "SELECT id_item, nama_item, harga, kategori FROM items";

            using var cmd = new SqlCommand(queryItems, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var id = reader.GetInt32(0).ToString();
                var nama = reader.GetString(1);
                var harga = reader.GetDecimal(2);
                var kategori = reader.GetString(3);

                itemOptions.Add(new SelectListItem
                {
                    Value = id,
                    Text = $"{nama}|{harga}|{kategori}"
                });

                kategoriSet.Add(kategori);
            }

            return (itemOptions, kategoriSet.ToList());
        }

        private List<GofoodTransaksiIndexViewModel> LoadTransaksiList()
        {
            var transaksiDict = new Dictionary<string, GofoodTransaksiIndexViewModel>();

            using var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            conn.Open();

            string query = @"
        SELECT 
            t.id_transaksi,
            t.id_pesanan,
            t.nama_pelanggan,
            t.tanggal_transaksi,
            t.waktu,
            t.metode,
            i.nama_item,
            i.kategori
        FROM transaksi t
        INNER JOIN items i ON t.id_item = i.id_item
        ORDER BY t.id_pesanan, t.id_transaksi";

            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                int idTransaksi = reader.GetInt32(0);         // t.id_transaksi
                string idPesanan = reader.GetString(1);       // t.id_pesanan

                if (!transaksiDict.ContainsKey(idPesanan))
                {
                    transaksiDict[idPesanan] = new GofoodTransaksiIndexViewModel
                    {
                        IdTransaksi = idTransaksi,
                        IdPesanan = idPesanan,
                        NamaPelanggan = reader.GetString(2),       // t.nama_pelanggan
                        TanggalTransaksi = reader.GetDateTime(3),  // t.tanggal_transaksi
                        Waktu = reader.GetTimeSpan(4),             // t.waktu
                        Metode = reader.GetString(5),              // t.metode
                        Items = new List<ItemDetail>()
                    };
                }

                var transaksi = transaksiDict[idPesanan];
                var kategori = reader.GetString(7); // i.kategori

                // Cegah kategori duplikat
                bool kategoriSudahAda = transaksi.Items.Any(i => i.Kategori == kategori);

                transaksi.Items.Add(new ItemDetail
                {
                    NamaItem = reader.GetString(6),  // i.nama_item
                    Kategori = kategoriSudahAda ? "" : kategori

                });
            }

            return transaksiDict.Values.ToList();
        }



    }
}
