namespace nge_eatweb.Models
{
    public class ItemTerjualViewModel
    {
        public int IdItem { get; set; }
        public string NamaItem { get; set; } = string.Empty;
        public string Kategori { get; set; } = string.Empty;
        public decimal Harga { get; set; }
        public int JumlahTerjual { get; set; }
    }

}
