using Microsoft.EntityFrameworkCore;
using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.DataAccess.Repositories
{
    public class NewsArticleRepository : Repository<NewsArticle>, INewsArticleRepository
    {
        public NewsArticleRepository(FUNewsManagementContext context) : base(context)
        {
        }

        public async Task<IEnumerable<NewsArticle>> GetArticlesWithDetailsAsync()
        {
            return await _dbSet
                .Include(na => na.Category)
                .Include(na => na.CreatedBy)
                .Include(na => na.UpdatedBy)
                .Include(na => na.NewsTags)
                    .ThenInclude(nt => nt.Tag)
                .OrderByDescending(na => na.CreatedDate)
                .ToListAsync();
        }

        public async Task<NewsArticle?> GetArticleWithDetailsByIdAsync(string id)
        {
            return await _dbSet
                .Include(na => na.Category)
                .Include(na => na.CreatedBy)
                .Include(na => na.UpdatedBy)
                .Include(na => na.NewsTags)
                    .ThenInclude(nt => nt.Tag)
                .FirstOrDefaultAsync(na => na.NewsArticleID == id);
        }

        public async Task<IEnumerable<NewsArticle>> SearchArticlesAsync(string searchTerm, short? categoryId, bool? status)
        {
            var query = _dbSet
                .Include(na => na.Category)
                .Include(na => na.CreatedBy)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(na =>
                    na.NewsTitle.Contains(searchTerm) ||
                    na.Headline.Contains(searchTerm) ||
                    na.CreatedBy.AccountName.Contains(searchTerm) ||
                    na.Category.CategoryName.Contains(searchTerm));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(na => na.CategoryID == categoryId);
            }

            if (status.HasValue)
            {
                query = query.Where(na => na.NewsStatus == status);
            }

            return await query.OrderByDescending(na => na.CreatedDate).ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetArticlesByCreatorAsync(short creatorId)
        {
            return await _dbSet
                .Include(na => na.Category)
                .Where(na => na.CreatedByID == creatorId)
                .OrderByDescending(na => na.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetArticlesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Include(na => na.Category)
                .Include(na => na.CreatedBy)
                .Where(na => na.CreatedDate >= startDate && na.CreatedDate <= endDate)
                .OrderByDescending(na => na.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<NewsArticle>> GetRelatedArticlesAsync(string currentArticleId, short categoryId)
        {
            // Lấy tags của article hiện tại
            var currentTags = await _context.NewsTags
                .Where(nt => nt.NewsArticleID == currentArticleId)
                .Select(nt => nt.TagID)
                .ToListAsync();

            // Tìm các article liên quan
            return await _dbSet
                .Include(na => na.Category)
                .Where(na =>
                    na.NewsArticleID != currentArticleId &&
                    na.NewsStatus == true &&
                    (na.CategoryID == categoryId ||
                     na.NewsTags.Any(nt => currentTags.Contains(nt.TagID))))
                .Take(3)
                .ToListAsync();
        }
    }
}