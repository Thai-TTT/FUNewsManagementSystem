using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.DataAccess.Repositories
{
    public interface INewsTagRepository : IRepository<NewsTag>
    {
        Task<IEnumerable<NewsTag>> GetTagsByArticleIdAsync(string articleId);
        Task<IEnumerable<NewsTag>> GetArticlesByTagIdAsync(int tagId);
        Task DeleteTagsFromArticleAsync(string articleId);
        Task AddTagsToArticleAsync(string articleId, List<int> tagIds);
        Task<bool> IsTagLinkedToArticleAsync(string articleId, int tagId);
    }
}