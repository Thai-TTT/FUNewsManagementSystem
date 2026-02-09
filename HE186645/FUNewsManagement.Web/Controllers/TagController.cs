using Microsoft.AspNetCore.Mvc;
using FUNewsManagement.BusinessLogic.Services;
using FUNewsManagement.DataAccess.Models;
using FUNewsManagement.WebApp.ViewModels;
using FUNewsManagement.WebApp.Helpers;

namespace FUNewsManagement.WebApp.Controllers
{
    [AuthorizeRole("Staff")]
    public class TagController : Controller
    {
        private readonly TagService _tagService;

        public TagController(TagService tagService)
        {
            _tagService = tagService;
        }

        // GET: Tag
        public async Task<IActionResult> Index(string searchTerm)
        {
            var tags = await _tagService.SearchTagsAsync(searchTerm);

            var viewModels = new List<TagViewModel>();
            foreach (var tag in tags)
            {
                var articleCount = await _tagService.GetArticleCountAsync(tag.TagID);
                var canDelete = await _tagService.CanDeleteTagAsync(tag.TagID);

                viewModels.Add(new TagViewModel
                {
                    TagID = tag.TagID,
                    TagName = tag.TagName,
                    Note = tag.Note,
                    ArticleCount = articleCount,
                    CanDelete = canDelete
                });
            }

            ViewBag.SearchTerm = searchTerm;

            return View(viewModels);
        }

        // GET: Tag/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                return NotFound();
            }

            var viewModel = new TagViewModel
            {
                TagID = tag.TagID,
                TagName = tag.TagName,
                Note = tag.Note
            };

            return Json(viewModel);
        }

        // POST: Tag/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TagViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            // Check duplicate tag name
            if (await _tagService.IsTagNameExistsAsync(model.TagName))
            {
                return Json(new { success = false, message = "Tag name already exists" });
            }

            var tag = new Tag
            {
                TagName = model.TagName,
                Note = model.Note
            };

            var result = await _tagService.CreateTagAsync(tag);

            if (result)
            {
                return Json(new { success = true, message = "Tag created successfully" });
            }

            return Json(new { success = false, message = "Failed to create tag" });
        }

        // POST: Tag/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TagViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Invalid data" });
            }

            var tag = await _tagService.GetTagByIdAsync(model.TagID);
            if (tag == null)
            {
                return Json(new { success = false, message = "Tag not found" });
            }

            // Check duplicate tag name
            if (await _tagService.IsTagNameExistsAsync(model.TagName, model.TagID))
            {
                return Json(new { success = false, message = "Tag name already exists" });
            }

            tag.TagName = model.TagName;
            tag.Note = model.Note;

            var result = await _tagService.UpdateTagAsync(tag);

            if (result)
            {
                return Json(new { success = true, message = "Tag updated successfully" });
            }

            return Json(new { success = false, message = "Failed to update tag" });
        }

        // POST: Tag/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var canDelete = await _tagService.CanDeleteTagAsync(id);
            if (!canDelete)
            {
                return Json(new { success = false, message = "Cannot delete tag that is used in articles" });
            }

            var result = await _tagService.DeleteTagAsync(id);

            if (result)
            {
                return Json(new { success = true, message = "Tag deleted successfully" });
            }

            return Json(new { success = false, message = "Failed to delete tag" });
        }

        // GET: Tag/GetAllTags
        public async Task<IActionResult> GetAllTags()
        {
            var tags = await _tagService.GetAllTagsAsync();
            return Json(tags.Select(t => new { t.TagID, t.TagName }));
        }
    }
}