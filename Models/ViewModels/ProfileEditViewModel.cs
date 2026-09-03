using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace BlogApp.Models.ViewModels
{
    public class ProfileEditViewModel
    {
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        [Display(Name = "Ad Soyad")]
        public string? FullName { get; set; }

        [StringLength(500, ErrorMessage = "Hakkımda en fazla 500 karakter olabilir.")]
        [Display(Name = "Hakkımda")]
        public string? Bio { get; set; }

        [Display(Name = "Profil Fotoğrafı")]
        public IFormFile? ProfilePhoto { get; set; }

        public string? CurrentPhotoPath { get; set; }
    }
}
