namespace FUNewsManagement.WebApp.ViewModels
{
    public class ReportViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Statistics
        public int TotalArticles { get; set; }
        public int ActiveArticles { get; set; }
        public int InactiveArticles { get; set; }

        // Group by Category
        public List<CategoryStatistic> CategoryStatistics { get; set; } = new List<CategoryStatistic>();

        // Group by Author
        public List<AuthorStatistic> AuthorStatistics { get; set; } = new List<AuthorStatistic>();

        // Articles list
        public List<NewsArticleViewModel> Articles { get; set; } = new List<NewsArticleViewModel>();
    }

    public class CategoryStatistic
    {
        public short CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int ArticleCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
    }

    public class AuthorStatistic
    {
        public short AccountID { get; set; }
        public string AccountName { get; set; }
        public int ArticleCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
    }
}