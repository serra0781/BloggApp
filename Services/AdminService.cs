using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services.Interfaces;

namespace BlogApp.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdminDashboardStats> GetDashboardStatsAsync()
        {
            return new AdminDashboardStats(
                UserCount: await _userManager.Users.CountAsync(),
                PostCount: await _context.Posts.CountAsync(p => p.Status == PostStatus.Approved),
                PendingPostCount: await _context.Posts.CountAsync(p => p.Status == PostStatus.Pending),
                CategoryCount: await _context.Categories.CountAsync(),
                CommentCount: await _context.Comments.CountAsync());
        }

        public async Task<List<UserListItemViewModel>> GetUsersAsync()
        {
            var users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();
            var model = new List<UserListItemViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserListItemViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? user.UserName ?? string.Empty,
                    PhotoPath = user.ProfilePhotoPath,
                    Roles = roles
                });
            }

            return model;
        }
    }
}
