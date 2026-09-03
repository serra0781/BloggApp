namespace BlogApp.Models.ViewModels
{
    public class AvatarViewModel
    {
        public string? Email { get; set; }
        public string? PhotoPath { get; set; }
        public int Size { get; set; } = 32;
        public string? LinkUserId { get; set; }
    }
}
