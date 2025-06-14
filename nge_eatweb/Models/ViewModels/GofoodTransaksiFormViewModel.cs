using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace nge_eatweb.Models.ViewModels
{
    public class GofoodTransaksiFormViewModel
    {
        [Required]
        [Display(Name = "Menu")]
        public int IdItems { get; set; }

        [Required]
        public DateTime TanggalTransaksi { get; set; }

        [Required]
        public TimeSpan Waktu { get; set; }

        [Required]
        public string Metode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Nama Pelanggan")]
        public string NamaPelanggan { get; set; } = string.Empty;

        // Dropdown source
        public List<SelectListItem> ItemOptions { get; set; } = new();
    }
}
