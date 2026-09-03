using BlogApp.Models.ViewModels;

namespace BlogApp.Services.Interfaces
{
    public interface IAuthorService
    {
        Task<List<AuthorListItemViewModel>> GetApprovedAuthorsAsync();
    }
}
