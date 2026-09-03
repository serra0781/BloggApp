using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services.Interfaces;

namespace BlogApp.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthorService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<AuthorListItemViewModel>> GetApprovedAuthorsAsync()
        {
            var grouped = await _context.Posts
                .Where(p => p.Status == PostStatus.Approved)
                .GroupBy(p => p.UserId)
                .Select(g => new { UserId = g.Key, PostCount = g.Count() })
                .ToListAsync();

            var authors = new List<AuthorListItemViewModel>();
            foreach (var g in grouped)
            {
                var user = await _userManager.FindByIdAsync(g.UserId);
                authors.Add(new AuthorListItemViewModel
                {
                    UserId = g.UserId,
                    Email = user?.Email ?? string.Empty,
                    FullName = user?.FullName,
                    PhotoPath = user?.ProfilePhotoPath,
                    PostCount = g.PostCount
                });
            }

            return authors.OrderByDescending(a => a.PostCount).ToList();
        }
    }
}
