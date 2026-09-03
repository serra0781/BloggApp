using Microsoft.AspNetCore.Http;

namespace BlogApp.Helpers
{
    // Profil fotoğrafı ve makale kapak görseli yüklemelerinde ortak doğrulama/kaydetme mantığı.
    public static class ImageUploadHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long MaxSizeBytes = 2 * 1024 * 1024; // 2 MB

        public const string AllowedExtensionsText = "JPG, PNG veya WEBP";
        public const string MaxSizeText = "2 MB";

        public static bool IsValidExtension(IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        public static bool IsValidSize(IFormFile file) => file.Length <= MaxSizeBytes;

        // Dosyayı wwwroot/uploads/{subfolder}/{baseFileName}{uzantı} olarak kaydeder,
        // web'den erişilebilir göreli yolu (örn. "/uploads/posts/12.jpg") döndürür.
        public static async Task<string> SaveAsync(IFormFile file, string webRootPath, string subfolder, string baseFileName)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var uploadsFolder = Path.Combine(webRootPath, "uploads", subfolder);
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{baseFileName}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/{subfolder}/{fileName}";
        }

        // Daha önce kaydedilmiş bir dosyayı (varsa) diskten siler; eski uzantı yenisinden
        // farklıysa da (örn. .jpg -> .png değişimi) dosya sistemi temiz kalır.
        public static void DeleteIfExists(string webRootPath, string? relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
            {
                return;
            }

            var trimmed = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webRootPath, trimmed);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
