using BlogApp.Models.ViewModels;

namespace BlogApp.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardStats> GetDashboardStatsAsync();
        Task<List<UserListItemViewModel>> GetUsersAsync();
    }

    public record AdminDashboardStats(
        int UserCount,
        int PostCount,
        int PendingPostCount,
        int CategoryCount,
        int CommentCount);
}
