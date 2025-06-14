namespace nge_eatweb.Models
{
    public class Transaksi
    {
        public int IdTransaksi { get; set; }
        public int IdItem { get; set; }
        public DateTime TanggalTransaksi { get; set; }
        public string Waktu { get; set; } = string.Empty;
        public string Metode { get; set; } = string.Empty;
        public string NamaPelanggan { get; set; } = string.Empty;
    }
}
