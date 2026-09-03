using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlogApp.Services.Interfaces;

namespace BlogApp.Controllers
{
    public class AuthorController : Controller
    {
        private readonly IAuthorService _authorService;

        public AuthorController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        // GET: /Author - en az bir onaylanmış makalesi olan yazarlar
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var authors = await _authorService.GetApprovedAuthorsAsync();
            return View(authors);
        }
    }
}
