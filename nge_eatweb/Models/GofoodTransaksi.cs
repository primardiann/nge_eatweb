using System;

namespace nge_eatweb.Models
{
    public class GofoodTransaksi
    {
        public int IdTransaksi { get; set; }
        public string IdPesanan { get; set; } = "";
        public int IdItem { get; set; }
        public DateTime TanggalTransaksi { get; set; }
        public TimeSpan Waktu { get; set; }
        public string Metode { get; set; } = "";
        public string NamaPelanggan { get; set; } = "";
        public decimal Total { get; set; }
    }
}
