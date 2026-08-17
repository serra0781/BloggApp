using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApp.Models;
using BlogApp.Models.ViewModels;

namespace BlogApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            ViewBag.UserCount = await _userManager.Users.CountAsync();
            ViewBag.PostCount = await _context.Posts.CountAsync(p => p.Status == PostStatus.Approved);
            ViewBag.PendingPostCount = await _context.Posts.CountAsync(p => p.Status == PostStatus.Pending);
            ViewBag.CategoryCount = await _context.Categories.CountAsync();
            ViewBag.CommentCount = await _context.Comments.CountAsync();

            return View();
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
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
                    Roles = roles
                });
            }

            return View(model);
        }
    }
}
