using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.DataAccess.Repositories
{
    public interface INewsArticleRepository : IRepository<NewsArticle>
    {
        Task<IEnumerable<NewsArticle>> GetArticlesWithDetailsAsync();
        Task<NewsArticle?> GetArticleWithDetailsByIdAsync(string id);
        Task<IEnumerable<NewsArticle>> SearchArticlesAsync(string searchTerm, short? categoryId, bool? status);
        Task<IEnumerable<NewsArticle>> GetArticlesByCreatorAsync(short creatorId);
        Task<IEnumerable<NewsArticle>> GetArticlesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<NewsArticle>> GetRelatedArticlesAsync(string currentArticleId, short categoryId);
    }
}