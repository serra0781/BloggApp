using BlogApp.Models;
using BlogApp.Services.Results;

namespace BlogApp.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetAllWithPostsAsync();
        Task<List<Category>> GetAllOrderedAsync();
        Task<Category?> GetByIdAsync(int id);
        Task CreateAsync(string name);
        Task<ServiceResult<Category>> UpdateAsync(int id, string name);
        Task<Category?> GetForDeleteAsync(int id);
        Task<ServiceResult<Category>> DeleteAsync(int id);
    }
}
