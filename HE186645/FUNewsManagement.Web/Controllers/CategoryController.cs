using Microsoft.AspNetCore.Mvc;
using FUNewsManagement.BusinessLogic.Services;
using FUNewsManagement.DataAccess.Models;
using FUNewsManagement.WebApp.ViewModels;
using FUNewsManagement.WebApp.Helpers;

namespace FUNewsManagement.WebApp.Controllers
{
    [AuthorizeRole("Staff")]
    public class CategoryController : Controller
    {
        private readonly CategoryService _categoryService;

        public CategoryController(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: Category
        public async Task<IActionResult> Index(string searchTerm)
        {
            var categories = await _categoryService.SearchCategoriesAsync(searchTerm);

            var viewModels = new List<CategoryViewModel>();
            foreach (var category in categories)
            {
                var articleCount = await _categoryService.GetArticleCountAsync(category.CategoryID);
                var canDelete = await _categoryService.CanDeleteCategoryAsync(category.CategoryID);

                viewModels.Add(new CategoryViewModel
                {
                    CategoryID = category.CategoryID,
                    CategoryName = category.CategoryName,
                    CategoryDesciption = category.CategoryDesciption,
                    ParentCategoryID = category.ParentCategoryID,
                    ParentCategoryName = category.ParentCategory?.CategoryName,
                    IsActive = category.IsActive ?? false,
                    ArticleCount = articleCount,
                    CanDelete = canDelete
                });
            }

            ViewBag.SearchTerm = searchTerm;

            return View(viewModels);
        }

        // GET: Category/Details/5
        public async Task<IActionResult> Details(short id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var parentCategories = await _categoryService.GetParentCategoriesAsync();

            var viewModel = new CategoryViewModel
            {
                CategoryID = category.CategoryID,
                CategoryName = category.CategoryName,
                CategoryDesciption = category.CategoryDesciption,
                ParentCategoryID = category.ParentCategoryID,
                IsActive = category.IsActive ?? false
            };

            return Json(new
            {
                category = viewModel,
                parentCategories = parentCategories.Select(c => new { c.CategoryID, c.CategoryName })
            });
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryViewModel model)
        {
            // Validate thủ công thay vì dùng ModelState (vì có nhiều property display-only)
            if (string.IsNullOrWhiteSpace(model.CategoryName))
            {
                return Json(new { success = false, message = "Category Name is required" });
            }

            if (string.IsNullOrWhiteSpace(model.CategoryDesciption))
            {
                return Json(new { success = false, message = "Description is required" });
            }

            if (model.CategoryName.Length > 100)
            {
                return Json(new { success = false, message = "Category Name cannot exceed 100 characters" });
            }

            if (model.CategoryDesciption.Length > 250)
            {
                return Json(new { success = false, message = "Description cannot exceed 250 characters" });
            }

            var category = new Category
            {
                CategoryName = model.CategoryName,
                CategoryDesciption = model.CategoryDesciption,
                ParentCategoryID = model.ParentCategoryID,
                IsActive = model.IsActive
            };

            var result = await _categoryService.CreateCategoryAsync(category);

            if (result)
            {
                return Json(new { success = true, message = "Category created successfully" });
            }

            return Json(new { success = false, message = "Failed to create category" });
        }

        // POST: Category/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryViewModel model)
        {
            // Validate thủ công
            if (string.IsNullOrWhiteSpace(model.CategoryName))
            {
                return Json(new { success = false, message = "Category Name is required" });
            }

            if (string.IsNullOrWhiteSpace(model.CategoryDesciption))
            {
                return Json(new { success = false, message = "Description is required" });
            }

            var category = await _categoryService.GetCategoryByIdAsync(model.CategoryID);
            if (category == null)
            {
                return Json(new { success = false, message = "Category not found" });
            }

            category.CategoryName = model.CategoryName;
            category.CategoryDesciption = model.CategoryDesciption;
            category.ParentCategoryID = model.ParentCategoryID;
            category.IsActive = model.IsActive;

            var result = await _categoryService.UpdateCategoryAsync(category);

            if (result)
            {
                return Json(new { success = true, message = "Category updated successfully" });
            }

            return Json(new { success = false, message = "Failed to update category" });
        }

        // POST: Category/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(short id)
        {
            var canDelete = await _categoryService.CanDeleteCategoryAsync(id);
            if (!canDelete)
            {
                return Json(new { success = false, message = "Cannot delete category that has articles" });
            }

            var result = await _categoryService.DeleteCategoryAsync(id);

            if (result)
            {
                return Json(new { success = true, message = "Category deleted successfully" });
            }

            return Json(new { success = false, message = "Failed to delete category" });
        }

        // POST: Category/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(short id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return Json(new { success = false, message = "Category not found" });
            }

            category.IsActive = !category.IsActive;
            var result = await _categoryService.UpdateCategoryAsync(category);

            if (result)
            {
                return Json(new { success = true, message = "Category status updated" });
            }

            return Json(new { success = false, message = "Failed to update status" });
        }

        // GET: Category/GetParentCategories
        public async Task<IActionResult> GetParentCategories()
        {
            var categories = await _categoryService.GetParentCategoriesAsync();
            return Json(categories.Select(c => new { c.CategoryID, c.CategoryName }));
        }
    }
}