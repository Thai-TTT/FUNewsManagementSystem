using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.DataAccess.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<IEnumerable<Category>> GetAllCategoriesWithDetailsAsync();
        Task<Category?> GetCategoryWithDetailsAsync(short id);
        Task<IEnumerable<Category>> SearchCategoriesAsync(string searchTerm);
        Task<bool> IsCategoryUsedInArticlesAsync(short categoryId);
        Task<int> GetArticleCountByCategoryAsync(short categoryId);
        Task<IEnumerable<Category>> GetActiveCategoriesAsync();
        Task<IEnumerable<Category>> GetParentCategoriesAsync();
    }
}