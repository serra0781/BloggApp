using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApp.Models;
using BlogApp.Models.ViewModels;

namespace BlogApp.Controllers
{
    public class AuthorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AuthorController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Author - en az bir onaylanmış makalesi olan yazarlar
        [AllowAnonymous]
        public async Task<IActionResult> Index()
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
                    PostCount = g.PostCount
                });
            }

            return View(authors.OrderByDescending(a => a.PostCount).ToList());
        }
    }
}
