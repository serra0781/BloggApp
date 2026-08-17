using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models.ViewModels
{
    public class CommentViewModel
    {
        [Required]
        public int PostId { get; set; }

        [Required(ErrorMessage = "Yorum boş olamaz.")]
        [StringLength(1000, ErrorMessage = "Yorum en fazla 1000 karakter olabilir.")]
        public string Content { get; set; } = string.Empty;
    }
}
