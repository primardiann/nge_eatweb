using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using System;
using System.Collections.Generic;
using System.IO;
using nge_eatweb.Models.ViewModels;

namespace nge_eatweb.Controllers
{
    public class LaporanController : Controller
    {
        private readonly IConfiguration _config;

        public LaporanController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index(DateTime? tanggalFilter, int page = 1, int pageSize = 10)
        {
            var model = new GofoodLaporanViewModel
            {
                FilterTanggal = tanggalFilter,
                TransaksiList = new List<TransaksiRow>()
            };

            int totalItemTerjual = 0;
            int totalTransaksi = 0;
            decimal totalGofood = 0;

            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            // Ringkasan (summary)
            var cmdTotal = new SqlCommand(@"
                SELECT 
                    SUM(t.jumlah) AS TotalItemTerjual,
                    COUNT(DISTINCT t.id_pesanan) AS JumlahTransaksi,
                    SUM(t.total) AS TotalGofood
                FROM transaksi t
                WHERE (@Tanggal IS NULL OR CAST(t.tanggal_transaksi AS DATE) = @Tanggal)
            ", conn);
            cmdTotal.Parameters.AddWithValue("@Tanggal", tanggalFilter ?? (object)DBNull.Value);

            using (var reader = cmdTotal.ExecuteReader())
            {
                if (reader.Read())
                {
                    totalItemTerjual = reader["TotalItemTerjual"] != DBNull.Value ? Convert.ToInt32(reader["TotalItemTerjual"]) : 0;
                    totalTransaksi = reader["JumlahTransaksi"] != DBNull.Value ? Convert.ToInt32(reader["JumlahTransaksi"]) : 0;
                    totalGofood = reader["TotalGofood"] != DBNull.Value ? Convert.ToDecimal(reader["TotalGofood"]) : 0;
                }
            }

            // Ambil data transaksi
            var cmdTrans = new SqlCommand(@"
                SELECT * FROM (
                    SELECT 
                        t.id_pesanan, t.tanggal_transaksi, t.waktu, t.metode, t.total,
                        STRING_AGG(i.kategori, ', ') AS kategori,
                        ROW_NUMBER() OVER (ORDER BY t.tanggal_transaksi DESC) AS RowNum
                    FROM transaksi t
                    LEFT JOIN items i ON t.id_item = i.id_item
                    WHERE (@Tanggal IS NULL OR CAST(t.tanggal_transaksi AS DATE) = @Tanggal)
                    GROUP BY t.id_pesanan, t.tanggal_transaksi, t.waktu, t.metode, t.total
                ) AS Result
                WHERE RowNum BETWEEN @Start AND @End
            ", conn);

            int startRow = (page - 1) * pageSize + 1;
            int endRow = startRow + pageSize - 1;

            cmdTrans.Parameters.AddWithValue("@Tanggal", tanggalFilter ?? (object)DBNull.Value);
            cmdTrans.Parameters.AddWithValue("@Start", startRow);
            cmdTrans.Parameters.AddWithValue("@End", endRow);

            using (var reader = cmdTrans.ExecuteReader())
            {
                while (reader.Read())
                {
                    model.TransaksiList.Add(new TransaksiRow
                    {
                        IdPesanan = reader["id_pesanan"]?.ToString() ?? "",
                        TanggalTransaksi = Convert.ToDateTime(reader["tanggal_transaksi"]),
                        Waktu = reader["waktu"] != DBNull.Value ? (TimeSpan)reader["waktu"] : TimeSpan.Zero,
                        Metode = reader["metode"]?.ToString() ?? "-",
                        Total = Convert.ToDecimal(reader["total"]),
                        Kategori = reader["kategori"]?.ToString() ?? "-"
                    });
                }
            }

            // Hitung total pages
            var cmdCount = new SqlCommand(@"
                SELECT COUNT(DISTINCT id_pesanan)
                FROM transaksi
                WHERE (@Tanggal IS NULL OR CAST(tanggal_transaksi AS DATE) = @Tanggal)
            ", conn);
            cmdCount.Parameters.AddWithValue("@Tanggal", tanggalFilter ?? (object)DBNull.Value);
            int totalRecords = (int)cmdCount.ExecuteScalar();

            model.TotalItemTerjual = totalItemTerjual;
            model.JumlahTransaksi = totalTransaksi;
            model.TotalGofood = totalGofood;
            model.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            model.CurrentPage = page;

            return View(model);
        }

        [HttpGet]
        public IActionResult ExportPdf(DateTime? tanggalFilter)
        {
            var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);
            var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            doc.Add(new Paragraph("Laporan Transaksi GoFood")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER));

            var table = new Table(6, false);
            table.AddHeaderCell("ID Pesanan");
            table.AddHeaderCell("Tanggal");
            table.AddHeaderCell("Waktu");
            table.AddHeaderCell("Metode");
            table.AddHeaderCell("Total");
            table.AddHeaderCell("Kategori");

            var data = GetTransaksi(tanggalFilter);
            foreach (var trx in data)
            {
                table.AddCell(trx.IdPesanan);
                table.AddCell(trx.TanggalTransaksi.ToString("yyyy-MM-dd"));
                table.AddCell(trx.Waktu.ToString(@"hh\:mm"));
                table.AddCell(trx.Metode);
                table.AddCell("Rp " + trx.Total.ToString("N0"));
                table.AddCell(trx.Kategori ?? "-");
            }

            doc.Add(table);
            doc.Close();

            return File(stream.ToArray(), "application/pdf", "LaporanGoFood.pdf");
        }

        [HttpGet]
        public IActionResult ExportExcel(DateTime? tanggalFilter)
        {
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Laporan");

            sheet.Cells[1, 1].Value = "ID Pesanan";
            sheet.Cells[1, 2].Value = "Tanggal";
            sheet.Cells[1, 3].Value = "Waktu";
            sheet.Cells[1, 4].Value = "Metode";
            sheet.Cells[1, 5].Value = "Total";
            sheet.Cells[1, 6].Value = "Kategori";

            var data = GetTransaksi(tanggalFilter);
            int row = 2;
            foreach (var trx in data)
            {
                sheet.Cells[row, 1].Value = trx.IdPesanan;
                sheet.Cells[row, 2].Value = trx.TanggalTransaksi.ToString("yyyy-MM-dd");
                sheet.Cells[row, 3].Value = trx.Waktu.ToString(@"hh\:mm");
                sheet.Cells[row, 4].Value = trx.Metode;
                sheet.Cells[row, 5].Value = trx.Total;
                sheet.Cells[row, 6].Value = trx.Kategori ?? "-";
                row++;
            }

            return File(package.GetAsByteArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "LaporanGoFood.xlsx");
        }

        private List<TransaksiRow> GetTransaksi(DateTime? tanggalFilter)
        {
            var list = new List<TransaksiRow>();

            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            conn.Open();

            var cmd = new SqlCommand(@"
                SELECT 
                    t.id_pesanan, t.tanggal_transaksi, t.waktu, t.metode, t.total,
                    STRING_AGG(i.kategori, ', ') AS kategori
                FROM transaksi t
                LEFT JOIN items i ON t.id_item = i.id_item
                WHERE (@Tanggal IS NULL OR CAST(t.tanggal_transaksi AS DATE) = @Tanggal)
                GROUP BY t.id_pesanan, t.tanggal_transaksi, t.waktu, t.metode, t.total
                ORDER BY t.tanggal_transaksi DESC
            ", conn);
            cmd.Parameters.AddWithValue("@Tanggal", tanggalFilter ?? (object)DBNull.Value);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new TransaksiRow
                {
                    IdPesanan = reader["id_pesanan"]?.ToString() ?? "",
                    TanggalTransaksi = Convert.ToDateTime(reader["tanggal_transaksi"]),
                    Waktu = reader["waktu"] != DBNull.Value ? (TimeSpan)reader["waktu"] : TimeSpan.Zero,
                    Metode = reader["metode"]?.ToString() ?? "-",
                    Total = Convert.ToDecimal(reader["total"]),
                    Kategori = reader["kategori"]?.ToString() ?? "-"
                });
            }

            return list;
        }
    }
}
