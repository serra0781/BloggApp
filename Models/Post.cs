using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık zorunludur.")]
        [StringLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "İçerik zorunludur.")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Kategori seçilmelidir.")]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public PostStatus Status { get; set; } = PostStatus.Pending;

        public int ViewCount { get; set; } = 0;

        // wwwroot altındaki göreli yol, örn. "/uploads/posts/12.jpg". Yüklenmemişse null.
        public string? ImagePath { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}