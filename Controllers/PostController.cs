using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using BlogApp.Helpers;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services.Interfaces;
using BlogApp.Services.Results;

namespace BlogApp.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public PostController(IPostService postService, ICategoryService categoryService,
            UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _postService = postService;
            _categoryService = categoryService;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: /Post - herkese açık, yalnızca onaylanmış makale listesi
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? categoryId, string? authorId)
        {
            if (!string.IsNullOrEmpty(authorId))
            {
                var author = await _userManager.FindByIdAsync(authorId);
                ViewBag.FilterAuthorEmail = author?.Email;
            }

            ViewBag.Categories = new SelectList(
                await _categoryService.GetAllOrderedAsync(), "Id", "Name", categoryId);

            var posts = await _postService.GetApprovedAsync(categoryId, authorId);
            return View(posts);
        }

        // GET: /Post/MyPosts - kullanıcının kendi makaleleri (tüm onay durumları)
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> MyPosts()
        {
            var userId = _userManager.GetUserId(User)!;
            var posts = await _postService.GetMyPostsAsync(userId);
            return View(posts);
        }

        // GET: /Post/Pending - admin onay bekleyen makaleler listesi
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Pending()
        {
            var posts = await _postService.GetPendingAsync();
            return View(posts);
        }

        // POST: /Post/Approve/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var result = await _postService.ApproveAsync(id);
            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Pending));
        }

        // POST: /Post/Reject/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var result = await _postService.RejectAsync(id);
            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Pending));
        }

        // GET: /Post/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var isAdmin = User.IsInRole("Admin");

            var result = await _postService.GetDetailsAsync(id, currentUserId, isAdmin);
            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                ViewBag.CurrentUserPhotoPath = currentUser?.ProfilePhotoPath;
            }

            return View(result.Data);
        }

        // GET: /Post/Create
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new PostViewModel
            {
                Categories = new SelectList(await _categoryService.GetAllOrderedAsync(), "Id", "Name")
            };

            return View(model);
        }

        // POST: /Post/Create
        [HttpPost]
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> Create(PostViewModel model)
        {
            if (model.Image != null && !ImageUploadHelper.IsValidExtension(model.Image))
            {
                ModelState.AddModelError(nameof(model.Image), $"Yalnızca {ImageUploadHelper.AllowedExtensionsText} dosyaları yüklenebilir.");
            }
            else if (model.Image != null && !ImageUploadHelper.IsValidSize(model.Image))
            {
                ModelState.AddModelError(nameof(model.Image), $"Görsel en fazla {ImageUploadHelper.MaxSizeText} olabilir.");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = new SelectList(
                    await _categoryService.GetAllOrderedAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            var userId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");
            var post = await _postService.CreateAsync(model, userId, isAdmin, _environment.WebRootPath);

            TempData["InfoMessage"] = post.Status == PostStatus.Pending
                ? "Makaleniz admin onayına gönderildi. Onaylandığında herkese açık listede görünecek."
                : "Makaleniz yayınlandı.";

            return RedirectToAction(nameof(MyPosts));
        }

        // GET: /Post/Edit/5
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");

            var result = await _postService.GetForEditAsync(id, userId, isAdmin);
            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            if (result.Status == ServiceResultStatus.Forbidden)
            {
                return Forbid();
            }

            var post = result.Data!;
            var model = new PostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CategoryId = post.CategoryId,
                CurrentImagePath = post.ImagePath,
                Categories = new SelectList(
                    await _categoryService.GetAllOrderedAsync(), "Id", "Name", post.CategoryId)
            };

            return View(model);
        }

        // POST: /Post/Edit/5
        [HttpPost]
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> Edit(int id, PostViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");

            if (model.Image != null && !ImageUploadHelper.IsValidExtension(model.Image))
            {
                ModelState.AddModelError(nameof(model.Image), $"Yalnızca {ImageUploadHelper.AllowedExtensionsText} dosyaları yüklenebilir.");
            }
            else if (model.Image != null && !ImageUploadHelper.IsValidSize(model.Image))
            {
                ModelState.AddModelError(nameof(model.Image), $"Görsel en fazla {ImageUploadHelper.MaxSizeText} olabilir.");
            }

            if (!ModelState.IsValid)
            {
                var existing = await _postService.GetForEditAsync(id, userId, isAdmin);
                if (existing.Status == ServiceResultStatus.NotFound)
                {
                    return NotFound();
                }
                if (existing.Status == ServiceResultStatus.Forbidden)
                {
                    return Forbid();
                }

                model.CurrentImagePath = existing.Data!.ImagePath;
                model.Categories = new SelectList(
                    await _categoryService.GetAllOrderedAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            var result = await _postService.UpdateAsync(id, model, userId, isAdmin, _environment.WebRootPath);

            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            if (result.Status == ServiceResultStatus.Forbidden)
            {
                return Forbid();
            }

            // Admin başka bir yazarın makalesini düzenlediğinde MyPosts'ta görünmeyeceği için
            // makalenin kendi sayfasına yönlendirilir; yazar ise kendi makale listesine döner.
            if (isAdmin)
            {
                return RedirectToAction(nameof(Details), new { id = result.Data!.Id });
            }

            return RedirectToAction(nameof(MyPosts));
        }

        // GET: /Post/Delete/5
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");

            var result = await _postService.GetForDeleteAsync(id, userId, isAdmin);
            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            if (result.Status == ServiceResultStatus.Forbidden)
            {
                return Forbid();
            }

            return View(result.Data);
        }

        // POST: /Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User)!;
            var isAdmin = User.IsInRole("Admin");

            var result = await _postService.DeleteAsync(id, userId, isAdmin, _environment.WebRootPath);

            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            if (result.Status == ServiceResultStatus.Forbidden)
            {
                return Forbid();
            }

            // Silinen makale artık yok; admin herkese açık listeye, yazar kendi listesine döner.
            if (isAdmin)
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(MyPosts));
        }
    }
}
