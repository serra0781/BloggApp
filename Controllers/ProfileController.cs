using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using BlogApp.Helpers;
using BlogApp.Models;
using BlogApp.Models.ViewModels;
using BlogApp.Services.Interfaces;

namespace BlogApp.Controllers
{
    public class ProfileController : Controller
    {
        private readonly IProfileService _profileService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(IProfileService profileService, UserManager<ApplicationUser> userManager, IWebHostEnvironment environment)
        {
            _profileService = profileService;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: /Profile - kendi profiline yönlendirir
        [Authorize]
        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User)!;
            return RedirectToAction(nameof(Details), new { id = userId });
        }

        // GET: /Profile/Details/{id} - herkese açık profil görünümü
        [AllowAnonymous]
        public async Task<IActionResult> Details(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            var model = await _profileService.GetProfileAsync(id, currentUserId);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // GET: /Profile/Edit
        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var userId = _userManager.GetUserId(User)!;
            var model = await _profileService.GetEditModelAsync(userId);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: /Profile/Edit
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(ProfileEditViewModel model)
        {
            var userId = _userManager.GetUserId(User)!;
            var existing = await _profileService.GetEditModelAsync(userId);
            if (existing == null)
            {
                return NotFound();
            }

            var currentPhotoPath = existing.CurrentPhotoPath;

            if (model.ProfilePhoto != null)
            {
                if (!ImageUploadHelper.IsValidExtension(model.ProfilePhoto))
                {
                    ModelState.AddModelError(nameof(model.ProfilePhoto), $"Yalnızca {ImageUploadHelper.AllowedExtensionsText} dosyaları yüklenebilir.");
                    model.CurrentPhotoPath = currentPhotoPath;
                    return View(model);
                }

                if (!ImageUploadHelper.IsValidSize(model.ProfilePhoto))
                {
                    ModelState.AddModelError(nameof(model.ProfilePhoto), $"Fotoğraf en fazla {ImageUploadHelper.MaxSizeText} olabilir.");
                    model.CurrentPhotoPath = currentPhotoPath;
                    return View(model);
                }
            }

            if (!ModelState.IsValid)
            {
                model.CurrentPhotoPath = currentPhotoPath;
                return View(model);
            }

            var user = await _profileService.UpdateProfileAsync(userId, model, _environment.WebRootPath);
            if (user == null)
            {
                return NotFound();
            }

            TempData["InfoMessage"] = "Profiliniz güncellendi.";
            return RedirectToAction(nameof(Details), new { id = user.Id });
        }
    }
}
