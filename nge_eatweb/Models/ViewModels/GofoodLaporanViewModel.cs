using System;
using System.Collections.Generic;

namespace nge_eatweb.Models.ViewModels
{
    public class GofoodLaporanViewModel
    {
        public DateTime? FilterTanggal { get; set; }
        public List<TransaksiRow> TransaksiList { get; set; } = new();
        public int TotalItemTerjual { get; set; }
        public int JumlahTransaksi { get; set; }
        public decimal TotalGofood { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
    }

    public class TransaksiRow
    {
        public string IdPesanan { get; set; } = string.Empty;
        public DateTime TanggalTransaksi { get; set; }
        public TimeSpan Waktu { get; set; }
        public string Metode { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public string Kategori { get; set; } = string.Empty;
    }
}
