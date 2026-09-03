using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Models
{
    // Kullanıcı profili için Identity'nin hazır IdentityUser sınıfına ek alanlar taşır.
    public class ApplicationUser : IdentityUser
    {
        [StringLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        public string? FullName { get; set; }

        [StringLength(500, ErrorMessage = "Hakkımda en fazla 500 karakter olabilir.")]
        public string? Bio { get; set; }

        // wwwroot altındaki göreli yol, örn. "/uploads/avatars/{userId}.jpg". Yüklenmemişse null.
        public string? ProfilePhotoPath { get; set; }
    }
}
