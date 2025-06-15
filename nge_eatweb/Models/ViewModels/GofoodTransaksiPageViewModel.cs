namespace nge_eatweb.Models.ViewModels
{
    public class GofoodTransaksiPageViewModel
    {
        public List<GofoodTransaksiIndexViewModel> TransaksiList { get; set; } = new();
        public GofoodTransaksiFormViewModel FormModel { get; set; } = new();
        public DateTime? FilterDate { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

    }
}
