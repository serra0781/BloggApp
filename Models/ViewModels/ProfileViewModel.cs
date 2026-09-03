namespace BlogApp.Models.ViewModels
{
    public class ProfileViewModel
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? PhotoPath { get; set; }
        public IList<string> Roles { get; set; } = new List<string>();
        public List<Post> Posts { get; set; } = new List<Post>();
        public bool IsOwnProfile { get; set; }
    }
}
