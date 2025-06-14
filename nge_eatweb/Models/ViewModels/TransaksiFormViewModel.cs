using System;
using System.ComponentModel.DataAnnotations;

namespace nge_eatweb.Models.ViewModels
{
    public class TransaksiFormViewModel
    {
        [Required]
        public int IdItems { get; set; } 

        [Required]
        public DateTime TanggalTransaksi { get; set; }

        [Required]
        public TimeSpan Waktu { get; set; }

        [Required]
        public string Metode { get; set; } = string.Empty;

        [Required]
        public string NamaPelanggan { get; set; } = string.Empty;
    }
}
