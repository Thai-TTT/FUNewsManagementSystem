using Microsoft.EntityFrameworkCore;
using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.DataAccess.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(FUNewsManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetAllCategoriesWithDetailsAsync()
        {
            return await _dbSet
                .Include(c => c.ParentCategory)
                .Include(c => c.NewsArticles)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryWithDetailsAsync(short id)
        {
            return await _dbSet
                .Include(c => c.ParentCategory)
                .Include(c => c.NewsArticles)
                .FirstOrDefaultAsync(c => c.CategoryID == id);
        }

        public async Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return await GetAllCategoriesWithDetailsAsync();
            }

            return await _dbSet
                .Include(c => c.ParentCategory)
                .Where(c => c.CategoryName.Contains(searchTerm) ||
                           c.CategoryDesciption.Contains(searchTerm))
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<bool> IsCategoryUsedInArticlesAsync(short categoryId)
        {
            return await _context.NewsArticles
                .AnyAsync(na => na.CategoryID == categoryId);
        }

        public async Task<int> GetArticleCountByCategoryAsync(short categoryId)
        {
            return await _context.NewsArticles
                .CountAsync(na => na.CategoryID == categoryId);
        }

        public async Task<IEnumerable<Category>> GetActiveCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetParentCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.ParentCategoryID == null || c.ParentCategoryID == c.CategoryID)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();
        }
    }
}