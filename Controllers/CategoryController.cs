using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BlogApp.Models.ViewModels;
using BlogApp.Services.Interfaces;
using BlogApp.Services.Results;

namespace BlogApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: /Category
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllWithPostsAsync();
            return View(categories);
        }

        // GET: /Category/Create
        public IActionResult Create()
        {
            return View(new CategoryViewModel());
        }

        // POST: /Category/Create
        [HttpPost]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _categoryService.CreateAsync(model.Name);

            return RedirectToAction(nameof(Index));
        }

        // GET: /Category/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(new CategoryViewModel { Id = category.Id, Name = category.Name });
        }

        // POST: /Category/Edit/5
        [HttpPost]
        public async Task<IActionResult> Edit(int id, CategoryViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _categoryService.UpdateAsync(id, model.Name);
            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Category/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _categoryService.GetForDeleteAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: /Category/Delete/5
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var result = await _categoryService.DeleteAsync(id);

            if (result.Status == ServiceResultStatus.NotFound)
            {
                return NotFound();
            }

            if (result.Status == ServiceResultStatus.ValidationError)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage!);
                var category = await _categoryService.GetForDeleteAsync(id);
                return View("Delete", category);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
