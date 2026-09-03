using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services.Interfaces;
using BlogApp.Services.Results;

namespace BlogApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ICommentService _commentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentController(ICommentService commentService, UserManager<ApplicationUser> userManager)
        {
            _commentService = commentService;
            _userManager = userManager;
        }

        // POST: /Comment/Create - kayıtlı ve giriş yapmış herhangi bir kullanıcı yorum yapabilir
        [HttpPost]
        public async Task<IActionResult> Create(CommentViewModel model)
        {
            var postExists = await _commentService.PostExistsAsync(model.PostId);
            if (!postExists)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["CommentError"] = "Yorum eklenemedi. Yorum boş olamaz ve en fazla 1000 karakter olabilir.";
                return RedirectToAction("Details", "Post", new { id = model.PostId });
            }

            var userId = _userManager.GetUserId(User)!;
            await _commentService.CreateAsync(model, userId);

            return RedirectToAction("Details", "Post", new { id = model.PostId });
        }

        // POST: /Comment/Delete/5 - yorumu yazan kişi veya admin silebilir
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");

            // Redirect için postId gerekiyor; silme öncesi yorumu ayrıca sorgulamamak adına
            // servis bulunamadı/forbidden durumlarını ServiceResult ile bildiriyor.
            var lookup = await _commentService.DeleteAsync(id, currentUserId, isAdmin);

            if (lookup.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            if (lookup.Status == ServiceResultStatus.Forbidden)
            {
                return Forbid();
            }

            return RedirectToAction("Details", "Post", new { id = lookup.Data!.PostId });
        }

        // GET: /Comment - admin yorum moderasyon listesi
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var comments = await _commentService.GetAllForModerationAsync();
            return View(comments);
        }
    }
}
