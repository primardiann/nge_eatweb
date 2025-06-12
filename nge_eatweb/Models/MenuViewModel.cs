namespace nge_eatweb.ViewModels
{
    public class MenuViewModel
    {
        public int Id { get; set; }
        public string NamaItem { get; set; } = string.Empty;
        public decimal Harga { get; set; }
        public string Kategori { get; set; } = string.Empty;
    }
}
