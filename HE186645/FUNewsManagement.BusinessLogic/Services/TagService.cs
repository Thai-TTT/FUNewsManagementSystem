using FUNewsManagement.DataAccess.Models;
using FUNewsManagement.DataAccess.Repositories;

namespace FUNewsManagement.BusinessLogic.Services
{
    public class TagService
    {
        private readonly ITagRepository _tagRepo;

        public TagService(ITagRepository tagRepo)
        {
            _tagRepo = tagRepo;
        }

        public async Task<IEnumerable<Tag>> GetAllTagsAsync()
        {
            return await _tagRepo.GetAllTagsWithDetailsAsync();
        }

        public async Task<Tag> GetTagByIdAsync(int id)
        {
            return await _tagRepo.GetTagWithArticlesAsync(id);
        }

        public async Task<bool> CreateTagAsync(Tag tag)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(tag.TagName))
                    return false;

                // Check duplicate tag name
                if (await _tagRepo.IsTagNameExistsAsync(tag.TagName))
                    return false;

                await _tagRepo.AddAsync(tag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTagAsync(Tag tag)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(tag.TagName))
                    return false;

                // Check duplicate tag name (excluding current tag)
                if (await _tagRepo.IsTagNameExistsAsync(tag.TagName, tag.TagID))
                    return false;

                await _tagRepo.UpdateAsync(tag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteTagAsync(int id)
        {
            try
            {
                // Check if tag is used in articles
                if (await _tagRepo.IsTagUsedInArticlesAsync(id))
                {
                    return false;
                }

                var tag = await _tagRepo.GetByIdAsync(id);
                if (tag == null)
                    return false;

                await _tagRepo.DeleteAsync(tag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Tag>> SearchTagsAsync(string searchTerm)
        {
            return await _tagRepo.SearchTagsAsync(searchTerm);
        }

        public async Task<bool> CanDeleteTagAsync(int id)
        {
            return !await _tagRepo.IsTagUsedInArticlesAsync(id);
        }

        public async Task<IEnumerable<Tag>> GetTagsByArticleAsync(string articleId)
        {
            return await _tagRepo.GetTagsByArticleAsync(articleId);
        }

        public async Task<int> GetArticleCountAsync(int tagId)
        {
            return await _tagRepo.GetArticleCountByTagAsync(tagId);
        }

        public async Task<bool> IsTagNameExistsAsync(string tagName, int? excludeTagId = null)
        {
            return await _tagRepo.IsTagNameExistsAsync(tagName, excludeTagId);
        }
    }
}