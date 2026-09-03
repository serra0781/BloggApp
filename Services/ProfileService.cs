using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlogApp.Helpers;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services.Interfaces;

namespace BlogApp.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ProfileViewModel?> GetProfileAsync(string id, string? currentUserId)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isOwnProfile = currentUserId == id;

            var postsQuery = _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .Where(p => p.UserId == id);

            // Kendi profilini görüntüleyen kişi tüm durumdaki makalelerini görür;
            // başkaları yalnızca onaylı (yayındaki) makaleleri görebilir.
            if (!isOwnProfile)
            {
                postsQuery = postsQuery.Where(p => p.Status == PostStatus.Approved);
            }

            var posts = await postsQuery.OrderByDescending(p => p.CreatedDate).ToListAsync();

            return new ProfileViewModel
            {
                UserId = user.Id,
                Email = user.Email ?? user.UserName ?? string.Empty,
                FullName = user.FullName,
                Bio = user.Bio,
                PhotoPath = user.ProfilePhotoPath,
                Roles = roles,
                Posts = posts,
                IsOwnProfile = isOwnProfile
            };
        }

        public async Task<ProfileEditViewModel?> GetEditModelAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            return new ProfileEditViewModel
            {
                FullName = user.FullName,
                Bio = user.Bio,
                CurrentPhotoPath = user.ProfilePhotoPath
            };
        }

        public async Task<ApplicationUser?> UpdateProfileAsync(string userId, ProfileEditViewModel model, string webRootPath)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return null;
            }

            if (model.ProfilePhoto != null)
            {
                ImageUploadHelper.DeleteIfExists(webRootPath, user.ProfilePhotoPath);
                user.ProfilePhotoPath = await ImageUploadHelper.SaveAsync(model.ProfilePhoto, webRootPath, "avatars", user.Id);
            }

            user.FullName = model.FullName;
            user.Bio = model.Bio;

            await _userManager.UpdateAsync(user);
            return user;
        }
    }
}
