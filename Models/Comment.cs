using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Yorum boş olamaz.")]
        [StringLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir.")]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required]
        public int PostId { get; set; }
        public Post? Post { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
    }
}
