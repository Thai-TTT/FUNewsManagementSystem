using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.DataAccess.Repositories
{
    public interface ITagRepository : IRepository<Tag>
    {
        Task<IEnumerable<Tag>> GetAllTagsWithDetailsAsync();
        Task<Tag?> GetTagWithArticlesAsync(int tagId);
        Task<IEnumerable<Tag>> SearchTagsAsync(string searchTerm);
        Task<bool> IsTagUsedInArticlesAsync(int tagId);
        Task<bool> IsTagNameExistsAsync(string tagName, int? excludeTagId = null);
        Task<IEnumerable<Tag>> GetTagsByArticleAsync(string articleId);
        Task<int> GetArticleCountByTagAsync(int tagId);
    }
}