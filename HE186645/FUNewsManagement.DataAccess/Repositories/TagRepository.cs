using Microsoft.EntityFrameworkCore;
using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.DataAccess.Repositories
{
    public class TagRepository : Repository<Tag>, ITagRepository
    {
        public TagRepository(FUNewsManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Tag>> GetAllTagsWithDetailsAsync()
        {
            return await _dbSet
                .Include(t => t.NewsTags)
                    .ThenInclude(nt => nt.NewsArticle)
                .OrderBy(t => t.TagName)
                .ToListAsync();
        }

        public async Task<Tag?> GetTagWithArticlesAsync(int tagId)
        {
            return await _dbSet
                .Include(t => t.NewsTags)
                    .ThenInclude(nt => nt.NewsArticle)
                        .ThenInclude(na => na.Category)
                .FirstOrDefaultAsync(t => t.TagID == tagId);
        }

        public async Task<IEnumerable<Tag>> SearchTagsAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return await GetAllAsync();
            }

            return await _dbSet
                .Where(t => t.TagName.Contains(searchTerm) ||
                           t.Note.Contains(searchTerm))
                .OrderBy(t => t.TagName)
                .ToListAsync();
        }

        public async Task<bool> IsTagUsedInArticlesAsync(int tagId)
        {
            return await _context.NewsTags
                .AnyAsync(nt => nt.TagID == tagId);
        }

        public async Task<bool> IsTagNameExistsAsync(string tagName, int? excludeTagId = null)
        {
            var query = _dbSet.Where(t => t.TagName == tagName);

            if (excludeTagId.HasValue)
            {
                query = query.Where(t => t.TagID != excludeTagId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Tag>> GetTagsByArticleAsync(string articleId)
        {
            return await _context.NewsTags
                .Where(nt => nt.NewsArticleID == articleId)
                .Select(nt => nt.Tag)
                .OrderBy(t => t.TagName)
                .ToListAsync();
        }

        public async Task<int> GetArticleCountByTagAsync(int tagId)
        {
            return await _context.NewsTags
                .CountAsync(nt => nt.TagID == tagId);
        }
    }
}