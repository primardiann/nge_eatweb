using System;
using System.Collections.Generic;

namespace nge_eatweb.Models.ViewModels
{
    public class GofoodTransaksiIndexViewModel
    {
        public string IdPesanan { get; set; } = string.Empty;
        public string NamaPelanggan { get; set; } = string.Empty;
        public DateTime TanggalTransaksi { get; set; }
        public TimeSpan Waktu { get; set; }
        public string Metode { get; set; } = string.Empty;
        public List<ItemDetail> Items { get; set; } = new();
    }

    public class ItemDetail
    {
        public string NamaItem { get; set; } = string.Empty;
        public string Kategori { get; set; } = string.Empty;
    }
}
