namespace nge_eatweb.Models.ViewModels
{
    public class GofoodTransaksiIndexViewModel
    {
        public int IdTransaksi { get; set; }
        public int IdItem { get; set; }
        public string NamaItem { get; set; } = "";
        public DateTime TanggalTransaksi { get; set; }
        public TimeSpan Waktu { get; set; }
        public string Metode { get; set; } = "";
        public string NamaPelanggan { get; set; } = "";

        // Properti tambahan untuk ID Pesanan custom
        public string IdPesanan => $"GFOOD{IdTransaksi.ToString().PadLeft(5, '0')}";
    }
}
