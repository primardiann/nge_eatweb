namespace nge_eatweb.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string NamaItem { get; set; } = string.Empty;
        public decimal Harga { get; set; }
        public string Kategori { get; set; } = string.Empty;
    }
}
