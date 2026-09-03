using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlogApp.Services.Interfaces;

namespace BlogApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var stats = await _adminService.GetDashboardStatsAsync();
            ViewBag.UserCount = stats.UserCount;
            ViewBag.PostCount = stats.PostCount;
            ViewBag.PendingPostCount = stats.PendingPostCount;
            ViewBag.CategoryCount = stats.CategoryCount;
            ViewBag.CommentCount = stats.CommentCount;

            return View();
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            var model = await _adminService.GetUsersAsync();
            return View(model);
        }
    }
}
