using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApp.Models;
using BlogApp.Models.ViewModels;

namespace BlogApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CommentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: /Comment/Create - kayıtlı ve giriş yapmış herhangi bir kullanıcı yorum yapabilir
        [HttpPost]
        public async Task<IActionResult> Create(CommentViewModel model)
        {
            var postExists = await _context.Posts.AnyAsync(p => p.Id == model.PostId);
            if (!postExists)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["CommentError"] = "Yorum eklenemedi. Yorum boş olamaz ve en fazla 1000 karakter olabilir.";
                return RedirectToAction("Details", "Post", new { id = model.PostId });
            }

            var comment = new Comment
            {
                PostId = model.PostId,
                Content = model.Content,
                UserId = _userManager.GetUserId(User)!,
                CreatedDate = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Post", new { id = model.PostId });
        }

        // POST: /Comment/Delete/5 - yorumu yazan kişi veya admin silebilir
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (!User.IsInRole("Admin") && comment.UserId != currentUserId)
            {
                return Forbid();
            }

            var postId = comment.PostId;
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Post", new { id = postId });
        }

        // GET: /Comment - admin yorum moderasyon listesi
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var comments = await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();

            return View(comments);
        }
    }
}
