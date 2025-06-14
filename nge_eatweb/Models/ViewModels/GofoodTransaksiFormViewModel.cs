using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nge_eatweb.Models.ViewModels
{
    public class GofoodTransaksiFormViewModel
    {
        [Required]
        public string IdPesanan { get; set; } = string.Empty;

        [Required]
        public DateTime TanggalTransaksi { get; set; }

        [Required]
        public TimeSpan Waktu { get; set; }

        [Required]
        public string Metode { get; set; } = string.Empty;

        [Required]
        public string NamaPelanggan { get; set; } = string.Empty;

        public decimal Total { get; set; }

        [Required]
        public List<SelectListItem> ItemOptions { get; set; } = new();

        public List<string> KategoriList { get; set; } = new();

        // ✅ WAJIB: Daftar item yang akan dibeli
        [Required(ErrorMessage = "Minimal 1 item harus dipilih.")]
        public List<ItemOrder> ItemList { get; set; } = new(); // <--- ini yang sebelumnya belum ada
    }

    public class ItemOrder
    {
        public int IdItem { get; set; }
        public string NamaItem { get; set; } = string.Empty;
        public string Kategori { get; set; } = string.Empty;
        public decimal Harga { get; set; }
        public int Jumlah { get; set; }
    }
}
