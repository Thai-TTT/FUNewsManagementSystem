using Microsoft.AspNetCore.Mvc;
using FUNewsManagement.BusinessLogic.Services;
using FUNewsManagement.DataAccess.Models;
using FUNewsManagement.WebApp.ViewModels;
using FUNewsManagement.WebApp.Helpers;

namespace FUNewsManagement.WebApp.Controllers
{
    [AuthorizeRole("Staff")]
    public class NewsArticleController : Controller
    {
        private readonly NewsArticleService _newsArticleService;
        private readonly CategoryService _categoryService;
        private readonly TagService _tagService;

        public NewsArticleController(
            NewsArticleService newsArticleService,
            CategoryService categoryService,
            TagService tagService)
        {
            _newsArticleService = newsArticleService;
            _categoryService = categoryService;
            _tagService = tagService;
        }

        // GET: NewsArticle
        public async Task<IActionResult> Index(string searchTerm, short? categoryId, bool? status)
        {
            var articles = await _newsArticleService.SearchArticlesAsync(searchTerm, categoryId, status);

            var viewModels = articles.Select(a => new NewsArticleViewModel
            {
                NewsArticleID = a.NewsArticleID,
                NewsTitle = a.NewsTitle,
                Headline = a.Headline,
                CategoryID = a.CategoryID,
                CategoryName = a.Category?.CategoryName,
                NewsStatus = a.NewsStatus ?? false,
                CreatedByName = a.CreatedBy?.AccountName,
                CreatedDate = a.CreatedDate
            }).ToList();

            // Get categories for filter
            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = categories;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SelectedStatus = status;

            return View(viewModels);
        }

        // GET: NewsArticle/Create
        public async Task<IActionResult> Create()
        {
            // Lấy user từ session với key "Account"
            var currentUser = HttpContext.Session.GetObjectFromJson<SystemAccount>("Account");
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new NewsArticleViewModel
            {
                NewsStatus = true, // Mặc định Active
                AvailableCategories = (await _categoryService.GetActiveCategoriesAsync()).ToList(),
                AvailableTags = (await _tagService.GetAllTagsAsync()).ToList()
            };

            return View(model);
        }

        // POST: NewsArticle/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(NewsArticleViewModel model, List<int> SelectedTagIds)
        {
            // Lấy user từ session với key "Account"
            var currentUser = HttpContext.Session.GetObjectFromJson<SystemAccount>("Account");
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Load lại categories và tags nếu validation fail
            model.AvailableCategories = (await _categoryService.GetActiveCategoriesAsync()).ToList();
            model.AvailableTags = (await _tagService.GetAllTagsAsync()).ToList();

            // Validate thủ công các trường bắt buộc
            if (string.IsNullOrWhiteSpace(model.NewsTitle))
            {
                TempData["ErrorMessage"] = "News Title is required";
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Headline))
            {
                TempData["ErrorMessage"] = "Headline is required";
                return View(model);
            }

            if (!model.CategoryID.HasValue || model.CategoryID.Value == 0)
            {
                TempData["ErrorMessage"] = "Category is required";
                return View(model);
            }

            try
            {
                var article = new NewsArticle
                {
                    NewsArticleID = await _newsArticleService.GenerateNewArticleIdAsync(),
                    NewsTitle = model.NewsTitle,
                    Headline = model.Headline,
                    NewsContent = model.NewsContent,
                    NewsSource = model.NewsSource,
                    CategoryID = model.CategoryID.Value,
                    NewsStatus = model.NewsStatus,
                    CreatedByID = currentUser.AccountID,
                    CreatedDate = DateTime.Now
                };

                var result = await _newsArticleService.CreateArticleAsync(article, SelectedTagIds ?? new List<int>());

                if (result)
                {
                    TempData["SuccessMessage"] = "Article created successfully";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to create article";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            return View(model);
        }

        // GET: NewsArticle/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<SystemAccount>("Account");
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var article = await _newsArticleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var model = new NewsArticleViewModel
            {
                NewsArticleID = article.NewsArticleID,
                NewsTitle = article.NewsTitle,
                Headline = article.Headline,
                NewsContent = article.NewsContent,
                NewsSource = article.NewsSource,
                CategoryID = article.CategoryID,
                NewsStatus = article.NewsStatus ?? false,
                AvailableCategories = (await _categoryService.GetActiveCategoriesAsync()).ToList(),
                AvailableTags = (await _tagService.GetAllTagsAsync()).ToList(),
                SelectedTagIds = article.NewsTags?.Select(nt => nt.TagID).ToList() ?? new List<int>()
            };

            return View(model);
        }

        // POST: NewsArticle/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(NewsArticleViewModel model, List<int> SelectedTagIds)
        {
            // Lấy user từ session với key "Account"
            var currentUser = HttpContext.Session.GetObjectFromJson<SystemAccount>("Account");
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Load lại categories và tags
            model.AvailableCategories = (await _categoryService.GetActiveCategoriesAsync()).ToList();
            model.AvailableTags = (await _tagService.GetAllTagsAsync()).ToList();

            // Validate
            if (string.IsNullOrWhiteSpace(model.NewsTitle))
            {
                TempData["ErrorMessage"] = "News Title is required";
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Headline))
            {
                TempData["ErrorMessage"] = "Headline is required";
                return View(model);
            }

            var article = await _newsArticleService.GetArticleByIdAsync(model.NewsArticleID);
            if (article == null)
            {
                return NotFound();
            }

            article.NewsTitle = model.NewsTitle;
            article.Headline = model.Headline;
            article.NewsContent = model.NewsContent;
            article.NewsSource = model.NewsSource;
            article.CategoryID = model.CategoryID;
            article.NewsStatus = model.NewsStatus;
            article.ModifiedDate = DateTime.Now;
            article.UpdatedByID = currentUser.AccountID;

            var result = await _newsArticleService.UpdateArticleAsync(article, SelectedTagIds ?? new List<int>(), currentUser.AccountID);

            if (result)
            {
                TempData["SuccessMessage"] = "Article updated successfully";
                return RedirectToAction(nameof(Index));
            }

            TempData["ErrorMessage"] = "Failed to update article";
            return View(model);
        }

        // POST: NewsArticle/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<SystemAccount>("Account");
            if (currentUser == null)
            {
                return Json(new { success = false, message = "Session expired. Please login again." });
            }

            var result = await _newsArticleService.DeleteArticleAsync(id);

            if (result)
            {
                return Json(new { success = true, message = "Article deleted successfully" });
            }

            return Json(new { success = false, message = "Failed to delete article" });
        }

        // POST: NewsArticle/Duplicate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duplicate(string id)
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<SystemAccount>("Account");
            if (currentUser == null)
            {
                return Json(new { success = false, message = "Session expired. Please login again." });
            }

            var result = await _newsArticleService.DuplicateArticleAsync(id, currentUser.AccountID);

            if (result)
            {
                return Json(new { success = true, message = "Article duplicated successfully" });
            }

            return Json(new { success = false, message = "Failed to duplicate article" });
        }

        // GET: NewsArticle/MyArticles - Xem bài viết của chính mình
        public async Task<IActionResult> MyArticles()
        {
            var currentUser = HttpContext.Session.GetObjectFromJson<SystemAccount>("Account");
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var articles = await _newsArticleService.GetArticlesByCreatorAsync(currentUser.AccountID);

            var viewModels = articles.Select(a => new NewsArticleViewModel
            {
                NewsArticleID = a.NewsArticleID,
                NewsTitle = a.NewsTitle,
                Headline = a.Headline,
                CategoryName = a.Category?.CategoryName,
                NewsStatus = a.NewsStatus ?? false,
                CreatedDate = a.CreatedDate,
                ModifiedDate = a.ModifiedDate
            }).ToList();

            return View(viewModels);
        }

        // GET: NewsArticle/Details/5 - Xem chi tiết (JSON cho modal)
        public async Task<IActionResult> Details(string id)
        {
            var article = await _newsArticleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            var viewModel = new NewsArticleViewModel
            {
                NewsArticleID = article.NewsArticleID,
                NewsTitle = article.NewsTitle,
                Headline = article.Headline,
                NewsContent = article.NewsContent,
                NewsSource = article.NewsSource,
                CategoryID = article.CategoryID,
                CategoryName = article.Category?.CategoryName,
                NewsStatus = article.NewsStatus ?? false,
                CreatedByName = article.CreatedBy?.AccountName,
                UpdatedByName = article.UpdatedBy?.AccountName,
                CreatedDate = article.CreatedDate,
                ModifiedDate = article.ModifiedDate,
                SelectedTagIds = article.NewsTags?.Select(nt => nt.TagID).ToList() ?? new List<int>()
            };

            return Json(viewModel);
        }
    }
}