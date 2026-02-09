using Microsoft.AspNetCore.Mvc;
using FUNewsManagement.BusinessLogic.Services;
using FUNewsManagement.WebApp.ViewModels;
using FUNewsManagement.WebApp.Helpers;

namespace FUNewsManagement.WebApp.Controllers
{
    [AuthorizeRole("Admin")]
    public class ReportController : Controller
    {
        private readonly NewsArticleService _newsArticleService;
        private readonly CategoryService _categoryService;
        private readonly SystemAccountService _accountService;

        public ReportController(
            NewsArticleService newsArticleService,
            CategoryService categoryService,
            SystemAccountService accountService)
        {
            _newsArticleService = newsArticleService;
            _categoryService = categoryService;
            _accountService = accountService;
        }

        // GET: Report
        public IActionResult Index()
        {
            var viewModel = new ReportViewModel
            {
                StartDate = DateTime.Now.AddMonths(-1).Date,
                EndDate = DateTime.Now.Date
            };

            return View(viewModel);
        }

        // POST: Report/Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(DateTime? startDate, DateTime? endDate)
        {
            // Start date: đầu ngày (00:00:00)
            var start = startDate?.Date ?? DateTime.Now.AddMonths(-1).Date;

            // End date: cuối ngày (23:59:59) để bao gồm cả ngày đó
            var end = endDate?.Date.AddDays(1).AddSeconds(-1) ?? DateTime.Now.Date.AddDays(1).AddSeconds(-1);

            // Get articles in date range
            var articles = await _newsArticleService.GetArticlesByDateRangeAsync(start, end);
            var articlesList = articles.ToList();

            var viewModel = new ReportViewModel
            {
                StartDate = startDate?.Date ?? DateTime.Now.AddMonths(-1).Date,
                EndDate = endDate?.Date ?? DateTime.Now.Date,
                TotalArticles = articlesList.Count,
                ActiveArticles = articlesList.Count(a => a.NewsStatus == true),
                InactiveArticles = articlesList.Count(a => a.NewsStatus == false || a.NewsStatus == null)
            };

            // Statistics by Category
            var categoryStats = articlesList
                .GroupBy(a => new { a.CategoryID, a.Category?.CategoryName })
                .Select(g => new CategoryStatistic
                {
                    CategoryID = g.Key.CategoryID ?? 0,
                    CategoryName = g.Key.CategoryName ?? "Uncategorized",
                    ArticleCount = g.Count(),
                    ActiveCount = g.Count(a => a.NewsStatus == true),
                    InactiveCount = g.Count(a => a.NewsStatus == false || a.NewsStatus == null)
                })
                .OrderByDescending(c => c.ArticleCount)
                .ToList();

            viewModel.CategoryStatistics = categoryStats;

            // Statistics by Author
            var authorStats = articlesList
                .GroupBy(a => new { a.CreatedByID, a.CreatedBy?.AccountName })
                .Select(g => new AuthorStatistic
                {
                    AccountID = g.Key.CreatedByID ?? 0,
                    AccountName = g.Key.AccountName ?? "Unknown",
                    ArticleCount = g.Count(),
                    ActiveCount = g.Count(a => a.NewsStatus == true),
                    InactiveCount = g.Count(a => a.NewsStatus == false || a.NewsStatus == null)
                })
                .OrderByDescending(a => a.ArticleCount)
                .ToList();

            viewModel.AuthorStatistics = authorStats;

            // Articles list - sắp xếp theo CreatedDate giảm dần (theo đề bài)
            viewModel.Articles = articlesList
                .OrderByDescending(a => a.CreatedDate)
                .Select(a => new NewsArticleViewModel
                {
                    NewsArticleID = a.NewsArticleID,
                    NewsTitle = a.NewsTitle,
                    Headline = a.Headline,
                    CategoryName = a.Category?.CategoryName,
                    CreatedByName = a.CreatedBy?.AccountName,
                    UpdatedByName = a.UpdatedBy?.AccountName,
                    CreatedDate = a.CreatedDate,
                    ModifiedDate = a.ModifiedDate,
                    NewsStatus = a.NewsStatus ?? false
                })
                .ToList();

            return View("Index", viewModel);
        }

        // GET: Report/ExportToExcel (Optional)
        public async Task<IActionResult> ExportToExcel(DateTime? startDate, DateTime? endDate)
        {
            // This is optional - requires EPPlus or ClosedXML package
            TempData["InfoMessage"] = "Excel export feature coming soon";
            return RedirectToAction("Index");
        }
    }
}