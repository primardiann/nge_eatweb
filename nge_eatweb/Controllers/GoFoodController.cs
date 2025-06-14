using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using nge_eatweb.Models.ViewModels;

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
            var transaksiList = new List<GofoodTransaksiIndexViewModel>();
            var itemOptions = new List<SelectListItem>();

            using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();

                // Ambil daftar transaksi
                var queryTransaksi = @"
            SELECT t.id_transaksi, t.id_item, i.nama_item, 
                   t.tanggal_transaksi, t.waktu, t.metode, t.nama_pelanggan
            FROM transaksi t
            JOIN items i ON i.id_item = t.id_item";
                using (var cmd = new SqlCommand(queryTransaksi, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        transaksiList.Add(new GofoodTransaksiIndexViewModel
                        {
                            IdTransaksi = reader.GetInt32(0),
                            IdItem = reader.GetInt32(1),
                            NamaItem = reader.GetString(2),
                            TanggalTransaksi = reader.GetDateTime(3),
                            Waktu = reader.GetTimeSpan(4), 
                            Metode = reader.GetString(5),
                            NamaPelanggan = reader.GetString(6)
                        });
                    }
                }

                // Ambil daftar item untuk dropdown
                var queryItems = "SELECT id_item, nama_item FROM items";
                using (var cmd = new SqlCommand(queryItems, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itemOptions.Add(new SelectListItem
                        {
                            Value = reader.GetInt32(0).ToString(),
                            Text = reader.GetString(1)
                        });
                    }
                }
            }

            var viewModel = new GofoodTransaksiPageViewModel
            {
                TransaksiList = transaksiList,
                FormModel = new GofoodTransaksiFormViewModel
                {
                    ItemOptions = itemOptions
                }
            };

            return View(viewModel);
        }

    }
}
