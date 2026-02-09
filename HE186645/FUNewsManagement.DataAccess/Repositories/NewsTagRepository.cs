using Microsoft.EntityFrameworkCore;
using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.DataAccess.Repositories
{
    public class NewsTagRepository : Repository<NewsTag>, INewsTagRepository
    {
        public NewsTagRepository(FUNewsManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<NewsTag>> GetTagsByArticleIdAsync(string articleId)
        {
            return await _dbSet
                .Include(nt => nt.Tag)
                .Where(nt => nt.NewsArticleID == articleId)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsTag>> GetArticlesByTagIdAsync(int tagId)
        {
            return await _dbSet
                .Include(nt => nt.NewsArticle)
                    .ThenInclude(na => na.Category)
                .Include(nt => nt.NewsArticle)
                    .ThenInclude(na => na.CreatedBy)
                .Where(nt => nt.TagID == tagId)
                .ToListAsync();
        }

        public async Task DeleteTagsFromArticleAsync(string articleId)
        {
            var tags = await _dbSet
                .Where(nt => nt.NewsArticleID == articleId)
                .ToListAsync();

            _dbSet.RemoveRange(tags);
            await _context.SaveChangesAsync();
        }

        public async Task AddTagsToArticleAsync(string articleId, List<int> tagIds)
        {
            if (tagIds == null || !tagIds.Any())
                return;

            var newsTags = tagIds.Select(tagId => new NewsTag
            {
                NewsArticleID = articleId,
                TagID = tagId
            }).ToList();

            await _dbSet.AddRangeAsync(newsTags);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsTagLinkedToArticleAsync(string articleId, int tagId)
        {
            return await _dbSet
                .AnyAsync(nt => nt.NewsArticleID == articleId && nt.TagID == tagId);
        }
    }
}