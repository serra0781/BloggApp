using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services.Results;

namespace BlogApp.Services.Interfaces
{
    public interface IPostService
    {
        Task<List<Post>> GetApprovedAsync(int? categoryId, string? authorId);
        Task<List<Post>> GetMyPostsAsync(string userId);
        Task<List<Post>> GetPendingAsync();

        Task<ServiceResult<Post>> ApproveAsync(int id);
        Task<ServiceResult<Post>> RejectAsync(int id);

        Task<ServiceResult<Post>> GetDetailsAsync(int id, string? currentUserId, bool isAdmin);

        Task<Post> CreateAsync(PostViewModel model, string userId, bool isAdmin, string webRootPath);

        Task<ServiceResult<Post>> GetForEditAsync(int id, string userId, bool isAdmin);
        Task<ServiceResult<Post>> UpdateAsync(int id, PostViewModel model, string userId, bool isAdmin, string webRootPath);

        Task<ServiceResult<Post>> GetForDeleteAsync(int id, string userId, bool isAdmin);
        Task<ServiceResult<Post>> DeleteAsync(int id, string userId, bool isAdmin, string webRootPath);

        Task<(Post? Featured, List<Post> Latest)> GetHomeFeedAsync(int latestCount);
    }
}
