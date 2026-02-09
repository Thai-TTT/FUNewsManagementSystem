using FUNewsManagement.DataAccess.Models;
using FUNewsManagement.DataAccess.Repositories;

namespace FUNewsManagement.BusinessLogic.Services
{
    public class NewsArticleService
    {
        private readonly INewsArticleRepository _newsArticleRepo;
        private readonly INewsTagRepository _newsTagRepo;

        public NewsArticleService(INewsArticleRepository newsArticleRepo, INewsTagRepository newsTagRepo)
        {
            _newsArticleRepo = newsArticleRepo;
            _newsTagRepo = newsTagRepo;
        }

        public async Task<IEnumerable<NewsArticle>> GetAllArticlesAsync()
        {
            return await _newsArticleRepo.GetArticlesWithDetailsAsync();
        }

        public async Task<NewsArticle?> GetArticleByIdAsync(string id)
        {
            return await _newsArticleRepo.GetArticleWithDetailsByIdAsync(id);
        }

        public async Task<bool> CreateArticleAsync(NewsArticle article, List<int> tagIds)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(article.Headline))
                    return false;

                // Generate new ID
                article.NewsArticleID = await GenerateNewArticleIdAsync();
                article.CreatedDate = DateTime.Now;
                article.NewsStatus = article.NewsStatus ?? false;

                await _newsArticleRepo.AddAsync(article);

                // Add tags
                if (tagIds != null && tagIds.Any())
                {
                    await _newsTagRepo.AddTagsToArticleAsync(article.NewsArticleID, tagIds);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateArticleAsync(NewsArticle article, List<int> tagIds, short updatedById)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(article.Headline))
                    return false;

                // Update tracking fields
                article.UpdatedByID = updatedById;
                article.ModifiedDate = DateTime.Now;

                await _newsArticleRepo.UpdateAsync(article);

                // Update tags
                await _newsTagRepo.DeleteTagsFromArticleAsync(article.NewsArticleID);

                if (tagIds != null && tagIds.Any())
                {
                    await _newsTagRepo.AddTagsToArticleAsync(article.NewsArticleID, tagIds);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteArticleAsync(string id)
        {
            try
            {
                var article = await _newsArticleRepo.GetByIdAsync(id);
                if (article == null)
                    return false;

                // Delete related tags first
                await _newsTagRepo.DeleteTagsFromArticleAsync(id);

                // Delete article
                await _newsArticleRepo.DeleteAsync(article);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<NewsArticle>> SearchArticlesAsync(string searchTerm, short? categoryId, bool? status)
        {
            return await _newsArticleRepo.SearchArticlesAsync(searchTerm, categoryId, status);
        }

        public async Task<IEnumerable<NewsArticle>> GetArticlesByCreatorAsync(short creatorId)
        {
            return await _newsArticleRepo.GetArticlesByCreatorAsync(creatorId);
        }

        public async Task<IEnumerable<NewsArticle>> GetArticlesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _newsArticleRepo.GetArticlesByDateRangeAsync(startDate, endDate);
        }

        public async Task<IEnumerable<NewsArticle>> GetRelatedArticlesAsync(string articleId)
        {
            var article = await _newsArticleRepo.GetByIdAsync(articleId);
            if (article == null || !article.CategoryID.HasValue)
                return new List<NewsArticle>();

            return await _newsArticleRepo.GetRelatedArticlesAsync(articleId, article.CategoryID.Value);
        }

        public async Task<IEnumerable<NewsArticle>> GetActiveArticlesAsync()
        {
            return await _newsArticleRepo.FindAsync(na => na.NewsStatus == true);
        }

        public async Task<string> GenerateNewArticleIdAsync()
        {
            var articles = await _newsArticleRepo.GetAllAsync();

            if (!articles.Any())
                return "1";

            var maxId = articles
                .Select(a =>
                {
                    if (int.TryParse(a.NewsArticleID, out int id))
                        return id;
                    return 0;
                })
                .Max();

            return (maxId + 1).ToString();
        }

        public async Task<bool> DuplicateArticleAsync(string sourceArticleId, short createdById)
        {
            try
            {
                var sourceArticle = await _newsArticleRepo.GetArticleWithDetailsByIdAsync(sourceArticleId);
                if (sourceArticle == null)
                    return false;

                var newArticle = new NewsArticle
                {
                    NewsArticleID = await GenerateNewArticleIdAsync(),
                    NewsTitle = sourceArticle.NewsTitle + " (Copy)",
                    Headline = sourceArticle.Headline,
                    NewsContent = sourceArticle.NewsContent,
                    NewsSource = sourceArticle.NewsSource,
                    CategoryID = sourceArticle.CategoryID,
                    NewsStatus = false,
                    CreatedByID = createdById,
                    CreatedDate = DateTime.Now
                };

                await _newsArticleRepo.AddAsync(newArticle);

                // Copy tags
                var sourceTags = sourceArticle.NewsTags?.Select(nt => nt.TagID).ToList();
                if (sourceTags != null && sourceTags.Any())
                {
                    await _newsTagRepo.AddTagsToArticleAsync(newArticle.NewsArticleID, sourceTags);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}