using FUNewsManagement.DataAccess.Models;
using FUNewsManagement.DataAccess.Repositories;

namespace FUNewsManagement.BusinessLogic.Services
{
    public class CategoryService
    {
        private readonly ICategoryRepository _categoryRepo;

        public CategoryService(ICategoryRepository categoryRepo)
        {
            _categoryRepo = categoryRepo;
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesAsync()
        {
            return await _categoryRepo.GetAllCategoriesWithDetailsAsync();
        }

        public async Task<Category> GetCategoryByIdAsync(short id)
        {
            return await _categoryRepo.GetCategoryWithDetailsAsync(id);
        }

        public async Task<bool> CreateCategoryAsync(Category category)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(category.CategoryName))
                    return false;

                if (string.IsNullOrWhiteSpace(category.CategoryDesciption))
                    return false;

                // Set default values
                category.IsActive = category.IsActive ?? true;

                await _categoryRepo.AddAsync(category);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(Category category)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(category.CategoryName))
                    return false;

                if (string.IsNullOrWhiteSpace(category.CategoryDesciption))
                    return false;

                await _categoryRepo.UpdateAsync(category);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCategoryAsync(short id)
        {
            try
            {
                // Check if category is used in articles
                if (await _categoryRepo.IsCategoryUsedInArticlesAsync(id))
                {
                    return false;
                }

                var category = await _categoryRepo.GetByIdAsync(id);
                if (category == null)
                    return false;

                await _categoryRepo.DeleteAsync(category);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm)
        {
            return await _categoryRepo.SearchCategoriesAsync(searchTerm);
        }

        public async Task<bool> CanDeleteCategoryAsync(short id)
        {
            return !await _categoryRepo.IsCategoryUsedInArticlesAsync(id);
        }

        public async Task<int> GetArticleCountAsync(short categoryId)
        {
            return await _categoryRepo.GetArticleCountByCategoryAsync(categoryId);
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _categoryRepo.GetActiveCategoriesAsync();
        }

        public async Task<IEnumerable<Category>> GetParentCategoriesAsync()
        {
            return await _categoryRepo.GetParentCategoriesAsync();
        }
    }
}