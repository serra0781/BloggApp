using Microsoft.EntityFrameworkCore;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services.Interfaces;
using BlogApp.Services.Results;

namespace BlogApp.Services
{
    public class CommentService : ICommentService
    {
        private readonly ApplicationDbContext _context;

        public CommentService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> PostExistsAsync(int postId)
        {
            return await _context.Posts.AnyAsync(p => p.Id == postId);
        }

        public async Task<Comment> CreateAsync(CommentViewModel model, string userId)
        {
            var comment = new Comment
            {
                PostId = model.PostId,
                Content = model.Content,
                UserId = userId,
                CreatedDate = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<ServiceResult<Comment>> DeleteAsync(int id, string currentUserId, bool isAdmin)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null)
            {
                return ServiceResult<Comment>.NotFound();
            }

            if (!isAdmin && comment.UserId != currentUserId)
            {
                return ServiceResult<Comment>.Forbidden();
            }

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return ServiceResult<Comment>.Ok(comment);
        }

        public async Task<List<Comment>> GetAllForModerationAsync()
        {
            return await _context.Comments
                .Include(c => c.User)
                .Include(c => c.Post)
                .OrderByDescending(c => c.CreatedDate)
                .ToListAsync();
        }
    }
}
