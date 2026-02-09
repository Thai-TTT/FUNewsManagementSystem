using Microsoft.EntityFrameworkCore;
using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.DataAccess
{
    public class FUNewsManagementContext : DbContext
    {
        public FUNewsManagementContext(DbContextOptions<FUNewsManagementContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<NewsArticle> NewsArticles { get; set; }
        public DbSet<SystemAccount> SystemAccounts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<NewsTag> NewsTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<NewsArticle>().ToTable("NewsArticle");
            modelBuilder.Entity<SystemAccount>().ToTable("SystemAccount");
            modelBuilder.Entity<Tag>().ToTable("Tag");
            modelBuilder.Entity<NewsTag>().ToTable("NewsTag");

            modelBuilder.Entity<NewsTag>()
                .HasKey(nt => new { nt.NewsArticleID, nt.TagID });

            modelBuilder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NewsArticle>()
                .HasOne(na => na.Category)
                .WithMany(c => c.NewsArticles)
                .HasForeignKey(na => na.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NewsArticle>()
                .HasOne(na => na.CreatedBy)
                .WithMany(sa => sa.CreatedNewsArticles)
                .HasForeignKey(na => na.CreatedByID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NewsArticle>()
                .HasOne(na => na.UpdatedBy)
                .WithMany(sa => sa.UpdatedNewsArticles)
                .HasForeignKey(na => na.UpdatedByID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NewsTag>()
                .HasOne(nt => nt.NewsArticle)
                .WithMany(na => na.NewsTags)
                .HasForeignKey(nt => nt.NewsArticleID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<NewsTag>()
                .HasOne(nt => nt.Tag)
                .WithMany(t => t.NewsTags)
                .HasForeignKey(nt => nt.TagID)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}