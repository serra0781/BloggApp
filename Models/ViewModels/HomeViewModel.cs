namespace BlogApp.Models.ViewModels
{
    public class HomeViewModel
    {
        public Post? FeaturedPost { get; set; }
        public List<Post> LatestPosts { get; set; } = new List<Post>();
    }
}
