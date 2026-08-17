namespace BlogApp.Models.ViewModels
{
    public class AuthorListItemViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int PostCount { get; set; }
    }
}
