using Microsoft.AspNetCore.Mvc;
using FUNewsManagement.BusinessLogic.Services;
using FUNewsManagement.WebApp.ViewModels;
using FUNewsManagement.WebApp.Models;
using System.Diagnostics;

namespace FUNewsManagement.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly NewsArticleService _newsArticleService;
        private readonly CategoryService _categoryService;

        public HomeController(
            NewsArticleService newsArticleService,
            CategoryService categoryService)
        {
            _newsArticleService = newsArticleService;
            _categoryService = categoryService;
        }

        // GET: Home
        public async Task<IActionResult> Index(short? categoryId, string searchTerm)
        {
            // Get only active articles
            var articles = await _newsArticleService.SearchArticlesAsync(searchTerm, categoryId, true);

            var viewModels = articles.Select(a => new NewsArticleViewModel
            {
                NewsArticleID = a.NewsArticleID,
                NewsTitle = a.NewsTitle,
                Headline = a.Headline,
                NewsContent = a.NewsContent?.Length > 200
                    ? a.NewsContent.Substring(0, 200) + "..."
                    : a.NewsContent,
                CategoryName = a.Category?.CategoryName,
                CategoryID = a.CategoryID,
                CreatedByName = a.CreatedBy?.AccountName,
                CreatedDate = a.CreatedDate
            }).ToList();

            // Get active categories for filter
            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.SearchTerm = searchTerm;

            return View(viewModels);
        }

        // GET: Home/ViewArticle/5
        public async Task<IActionResult> ViewArticle(string id)
        {
            var article = await _newsArticleService.GetArticleByIdAsync(id);
            if (article == null || article.NewsStatus != true)
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
                CategoryName = article.Category?.CategoryName,
                CategoryID = article.CategoryID,
                CreatedByName = article.CreatedBy?.AccountName,
                CreatedDate = article.CreatedDate,
                ModifiedDate = article.ModifiedDate,
                UpdatedByName = article.UpdatedBy?.AccountName,
                AssignedTags = article.NewsTags?.Select(nt => nt.Tag).ToList() ?? new List<FUNewsManagement.DataAccess.Models.Tag>()
            };

            // Get related articles
            var relatedArticles = await _newsArticleService.GetRelatedArticlesAsync(id);
            ViewBag.RelatedArticles = relatedArticles.Select(a => new NewsArticleViewModel
            {
                NewsArticleID = a.NewsArticleID,
                NewsTitle = a.NewsTitle,
                Headline = a.Headline,
                CategoryName = a.Category?.CategoryName,
                CreatedDate = a.CreatedDate
            }).Take(3).ToList();

            return View(viewModel);
        }

        // GET: Home/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: Home/Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}