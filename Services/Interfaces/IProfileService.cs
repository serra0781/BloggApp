using BlogApp.Models;
using BlogApp.Models.ViewModels;

namespace BlogApp.Services.Interfaces
{
    public interface IProfileService
    {
        Task<ProfileViewModel?> GetProfileAsync(string id, string? currentUserId);
        Task<ProfileEditViewModel?> GetEditModelAsync(string userId);
        Task<ApplicationUser?> UpdateProfileAsync(string userId, ProfileEditViewModel model, string webRootPath);
    }
}
