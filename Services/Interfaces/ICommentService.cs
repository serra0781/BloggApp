using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services.Results;

namespace BlogApp.Services.Interfaces
{
    public interface ICommentService
    {
        Task<bool> PostExistsAsync(int postId);
        Task<Comment> CreateAsync(CommentViewModel model, string userId);
        Task<ServiceResult<Comment>> DeleteAsync(int id, string currentUserId, bool isAdmin);
        Task<List<Comment>> GetAllForModerationAsync();
    }
}
