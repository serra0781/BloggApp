using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services.Interfaces;

namespace BlogApp.Controllers;

public class HomeController : Controller
{
    private readonly IPostService _postService;

    public HomeController(IPostService postService)
    {
        _postService = postService;
    }

    public async Task<IActionResult> Index()
    {
        var (featured, latest) = await _postService.GetHomeFeedAsync(6);

        var model = new HomeViewModel
        {
            FeaturedPost = featured,
            LatestPosts = latest
        };

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
