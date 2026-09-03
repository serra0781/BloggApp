namespace BlogApp.Models.ViewModels
{
    public class AuthorListItemViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhotoPath { get; set; }
        public int PostCount { get; set; }
    }
}
