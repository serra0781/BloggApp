using Microsoft.EntityFrameworkCore;
using BlogApp.Models;
using BlogApp.Services.Interfaces;
using BlogApp.Services.Results;

namespace BlogApp.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _context;

        public CategoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetAllWithPostsAsync()
        {
            return await _context.Categories
                .Include(c => c.Posts)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<List<Category>> GetAllOrderedAsync()
        {
            return await _context.Categories.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int id)
        {
            return await _context.Categories.FindAsync(id);
        }

        public async Task CreateAsync(string name)
        {
            _context.Categories.Add(new Category { Name = name });
            await _context.SaveChangesAsync();
        }

        public async Task<ServiceResult<Category>> UpdateAsync(int id, string name)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return ServiceResult<Category>.NotFound();
            }

            category.Name = name;
            await _context.SaveChangesAsync();
            return ServiceResult<Category>.Ok(category);
        }

        public async Task<Category?> GetForDeleteAsync(int id)
        {
            return await _context.Categories
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ServiceResult<Category>> DeleteAsync(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Posts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                return ServiceResult<Category>.NotFound();
            }

            if (category.Posts.Any())
            {
                return ServiceResult<Category>.Invalid("Bu kategoriye bağlı makaleler olduğu için silinemez.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return ServiceResult<Category>.Ok(category);
        }
    }
}
