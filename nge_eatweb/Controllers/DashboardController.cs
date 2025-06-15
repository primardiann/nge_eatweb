using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace nge_eatweb.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IConfiguration _configuration;

        public DashboardController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            var connStr = _configuration.GetConnectionString("DefaultConnection");
            var pendapatanBulanan = new decimal[12];
            decimal totalGofood = 0;
            int jumlahTransaksi = 0;
            int totalItemTerjual = 0;

            using (var conn = new SqlConnection(connStr))
            {
                conn.Open();

                // Ambil data pendapatan per bulan
                var cmdPendapatan = new SqlCommand(@"
                    SELECT MONTH(tanggal_transaksi) AS Bulan, SUM(total) AS TotalPendapatan
                    FROM transaksi
                    GROUP BY MONTH(tanggal_transaksi)", conn);

                using (var reader = cmdPendapatan.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int bulan = Convert.ToInt32(reader["Bulan"]);
                        decimal total = Convert.ToDecimal(reader["TotalPendapatan"]);
                        if (bulan >= 1 && bulan <= 12)
                        {
                            pendapatanBulanan[bulan - 1] = total / 1_000_000; // dalam juta rupiah
                        }
                    }
                }

                // Total keseluruhan
                var cmdTotal = new SqlCommand("SELECT SUM(total) FROM transaksi", conn);
                totalGofood = Convert.ToDecimal(cmdTotal.ExecuteScalar() ?? 0);

                // Jumlah transaksi unik (id_pesanan)
                var cmdJumlah = new SqlCommand("SELECT COUNT(DISTINCT id_pesanan) FROM transaksi", conn);
                jumlahTransaksi = Convert.ToInt32(cmdJumlah.ExecuteScalar() ?? 0);

                // Total item terjual (jumlah)
                var cmdItem = new SqlCommand("SELECT SUM(jumlah) FROM transaksi", conn);
                totalItemTerjual = Convert.ToInt32(cmdItem.ExecuteScalar() ?? 0);
            }

            // Kirim ke View via ViewBag
            ViewBag.PendapatanBulanan = pendapatanBulanan;
            ViewBag.TotalGofood = totalGofood;
            ViewBag.JumlahTransaksi = jumlahTransaksi;
            ViewBag.TotalItemTerjual = totalItemTerjual;

            return View();
        }
    }
}
