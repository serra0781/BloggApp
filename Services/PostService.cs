using Microsoft.EntityFrameworkCore;
using BlogApp.Helpers;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services.Interfaces;
using BlogApp.Services.Results;

namespace BlogApp.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDbContext _context;

        public PostService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Post>> GetApprovedAsync(int? categoryId, string? authorId)
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
            }

            return await query.OrderByDescending(p => p.CreatedDate).ToListAsync();
        }

        public async Task<List<Post>> GetMyPostsAsync(string userId)
        {
            return await _context.Posts
                .Include(p => p.Category)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPendingAsync()
        {
            return await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Where(p => p.Status == PostStatus.Pending)
                .OrderBy(p => p.CreatedDate)
                .ToListAsync();
        }

        public async Task<ServiceResult<Post>> ApproveAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return ServiceResult<Post>.NotFound();
            }

            post.Status = PostStatus.Approved;
            await _context.SaveChangesAsync();
            return ServiceResult<Post>.Ok(post);
        }

        public async Task<ServiceResult<Post>> RejectAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return ServiceResult<Post>.NotFound();
            }

            post.Status = PostStatus.Rejected;
            await _context.SaveChangesAsync();
            return ServiceResult<Post>.Ok(post);
        }

        public async Task<ServiceResult<Post>> GetDetailsAsync(int id, string? currentUserId, bool isAdmin)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Include(p => p.Comments.OrderBy(c => c.CreatedDate))
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return ServiceResult<Post>.NotFound();
            }

            // Onay bekleyen/reddedilen makaleleri yalnızca sahibi veya admin görebilir;
            // yetkisiz erişimde NotFound dönülür (varlığını Forbidden ile ele vermemek için).
            if (post.Status != PostStatus.Approved && !CanManage(post, currentUserId, isAdmin))
            {
                return ServiceResult<Post>.NotFound();
            }

            if (post.Status == PostStatus.Approved)
            {
                post.ViewCount++;
                await _context.SaveChangesAsync();
            }

            return ServiceResult<Post>.Ok(post);
        }

        public async Task<Post> CreateAsync(PostViewModel model, string userId, bool isAdmin, string webRootPath)
        {
            var post = new Post
            {
                Title = model.Title,
                Content = model.Content,
                CategoryId = model.CategoryId,
                UserId = userId,
                CreatedDate = DateTime.UtcNow,
                Status = isAdmin ? PostStatus.Approved : PostStatus.Pending
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Görselin dosya adı makalenin Id'sine dayandığı için önce makale kaydedilir,
            // ardından görsel yüklenip ImagePath ikinci bir kayıtla eklenir.
            if (model.Image != null)
            {
                post.ImagePath = await ImageUploadHelper.SaveAsync(model.Image, webRootPath, "posts", post.Id.ToString());
                await _context.SaveChangesAsync();
            }

            return post;
        }

        public async Task<ServiceResult<Post>> GetForEditAsync(int id, string userId, bool isAdmin)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return ServiceResult<Post>.NotFound();
            }

            if (!CanManage(post, userId, isAdmin))
            {
                return ServiceResult<Post>.Forbidden();
            }

            return ServiceResult<Post>.Ok(post);
        }

        public async Task<ServiceResult<Post>> UpdateAsync(int id, PostViewModel model, string userId, bool isAdmin, string webRootPath)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return ServiceResult<Post>.NotFound();
            }

            if (!CanManage(post, userId, isAdmin))
            {
                return ServiceResult<Post>.Forbidden();
            }

            post.Title = model.Title;
            post.Content = model.Content;
            post.CategoryId = model.CategoryId;

            if (model.Image != null)
            {
                ImageUploadHelper.DeleteIfExists(webRootPath, post.ImagePath);
                post.ImagePath = await ImageUploadHelper.SaveAsync(model.Image, webRootPath, "posts", post.Id.ToString());
            }

            // Admin dışındaki bir yazar makaleyi düzenlediğinde tekrar onay sürecine girer.
            if (!isAdmin)
            {
                post.Status = PostStatus.Pending;
            }

            await _context.SaveChangesAsync();
            return ServiceResult<Post>.Ok(post);
        }

        public async Task<ServiceResult<Post>> GetForDeleteAsync(int id, string userId, bool isAdmin)
        {
            var post = await _context.Posts
                .Include(p => p.Category)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return ServiceResult<Post>.NotFound();
            }

            if (!CanManage(post, userId, isAdmin))
            {
                return ServiceResult<Post>.Forbidden();
            }

            return ServiceResult<Post>.Ok(post);
        }

        public async Task<ServiceResult<Post>> DeleteAsync(int id, string userId, bool isAdmin, string webRootPath)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return ServiceResult<Post>.NotFound();
            }

            if (!CanManage(post, userId, isAdmin))
            {
                return ServiceResult<Post>.Forbidden();
            }

            ImageUploadHelper.DeleteIfExists(webRootPath, post.ImagePath);

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return ServiceResult<Post>.Ok(post);
        }

        public async Task<(Post? Featured, List<Post> Latest)> GetHomeFeedAsync(int latestCount)
        {
            var featured = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Where(p => p.Status == PostStatus.Approved)
                .OrderByDescending(p => p.ViewCount)
                .ThenByDescending(p => p.CreatedDate)
                .FirstOrDefaultAsync();

            var latestQuery = _context.Posts
                .Include(p => p.User)
                .Include(p => p.Category)
                .Where(p => p.Status == PostStatus.Approved);

            if (featured != null)
            {
                latestQuery = latestQuery.Where(p => p.Id != featured.Id);
            }

            var latest = await latestQuery
                .OrderByDescending(p => p.CreatedDate)
                .Take(latestCount)
                .ToListAsync();

            return (featured, latest);
        }

        // Admin her makaleyi yönetebilir; Yazar yalnızca kendi makalesini yönetebilir.
        private static bool CanManage(Post post, string? userId, bool isAdmin)
        {
            return isAdmin || post.UserId == userId;
        }
    }
}
