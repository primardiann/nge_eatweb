using System.Collections.Generic;

namespace nge_eatweb.Models.ViewModels
{
    public class GofoodTransaksiPageViewModel
    {
        public List<GofoodTransaksiIndexViewModel> TransaksiList { get; set; } = new();
        public GofoodTransaksiFormViewModel FormModel { get; set; } = new();
    }
}
