using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BlogApp.Models;
using BlogApp.Models.ViewModels;

namespace BlogApp.Controllers
{
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PostController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Post - herkese açık, yalnızca onaylanmış makale listesi
        [AllowAnonymous]
        public async Task<IActionResult> Index(int? categoryId, string? authorId)
        {
            var query = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Where(p => p.Status == PostStatus.Approved)
                .AsQueryable();

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!string.IsNullOrEmpty(authorId))
            {
                query = query.Where(p => p.UserId == authorId);
                var author = await _userManager.FindByIdAsync(authorId);
                ViewBag.FilterAuthorEmail = author?.Email;
            }

            ViewBag.Categories = new SelectList(
                await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", categoryId);

            var posts = await query.OrderByDescending(p => p.CreatedDate).ToListAsync();
            return View(posts);
        }

        // GET: /Post/MyPosts - kullanıcının kendi makaleleri (tüm onay durumları)
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> MyPosts()
        {
            var userId = _userManager.GetUserId(User);
            var posts = await _context.Posts
                .Include(p => p.Category)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            return View(posts);
        }

        // GET: /Post/Pending - admin onay bekleyen makaleler listesi
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Pending()
        {
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Where(p => p.Status == PostStatus.Pending)
                .OrderBy(p => p.CreatedDate)
                .ToListAsync();

            return View(posts);
        }

        // POST: /Post/Approve/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            post.Status = PostStatus.Approved;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Pending));
        }

        // POST: /Post/Reject/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            post.Status = PostStatus.Rejected;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Pending));
        }

        // GET: /Post/Details/5
        [AllowAnonymous]
        public async Task<IActionResult> Details(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Comments.OrderBy(c => c.CreatedDate))
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Onay bekleyen/reddedilen makaleleri yalnızca sahibi veya admin görebilir.
            if (post.Status != PostStatus.Approved && !CanManage(post))
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: /Post/Create
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new PostViewModel
            {
                Categories = new SelectList(await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name")
            };

            return View(model);
        }

        // POST: /Post/Create
        [HttpPost]
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> Create(PostViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Categories = new SelectList(
                    await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                CategoryId = model.CategoryId,
                UserId = _userManager.GetUserId(User)!,
                CreatedDate = DateTime.UtcNow,
                // Admin'in oluşturduğu makale doğrudan yayınlanır; diğer yazarlarınki onay bekler.
                Status = User.IsInRole("Admin") ? PostStatus.Approved : PostStatus.Pending
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            TempData["InfoMessage"] = post.Status == PostStatus.Pending
                ? "Makaleniz admin onayına gönderildi. Onaylandığında herkese açık listede görünecek."
                : "Makaleniz yayınlandı.";

            return RedirectToAction(nameof(MyPosts));
        }

        // GET: /Post/Edit/5
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (!CanManage(post))
            {
                return Forbid();
            }

            var model = new PostViewModel
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CategoryId = post.CategoryId,
                Categories = new SelectList(
                    await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", post.CategoryId)
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

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (!CanManage(post))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                model.Categories = new SelectList(
                    await _context.Categories.OrderBy(c => c.Name).ToListAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            post.Title = model.Title;
            post.Content = model.Content;
            post.CategoryId = model.CategoryId;

            // Admin dışındaki bir yazar makaleyi düzenlediğinde tekrar onay sürecine girer.
            if (!User.IsInRole("Admin"))
            {
                post.Status = PostStatus.Pending;
            }

            await _context.SaveChangesAsync();

            // Admin başka bir yazarın makalesini düzenlediğinde MyPosts'ta görünmeyeceği için
            // makalenin kendi sayfasına yönlendirilir; yazar ise kendi makale listesine döner.
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Details), new { id = post.Id });
            }

            return RedirectToAction(nameof(MyPosts));
        }

        // GET: /Post/Delete/5
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            if (!CanManage(post))
            {
                return Forbid();
            }

            return View(post);
        }

        // POST: /Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Yazar,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            if (!CanManage(post))
            {
                return Forbid();
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            // Silinen makale artık yok; admin herkese açık listeye, yazar kendi listesine döner.
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(MyPosts));
        }

        // Admin her makaleyi yönetebilir; Yazar yalnızca kendi makalesini yönetebilir.
        private bool CanManage(Post post)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            return post.UserId == _userManager.GetUserId(User);
        }
    }
}
