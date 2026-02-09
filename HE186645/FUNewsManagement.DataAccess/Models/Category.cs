using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUNewsManagement.DataAccess.Models
{
    [Table("Category")]
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public short CategoryID { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = null!;

        [Required]
        [StringLength(250)]
        public required string CategoryDesciption { get; set; } = null!;

        public short? ParentCategoryID { get; set; }

        public bool? IsActive { get; set; }

        [ForeignKey("ParentCategoryID")]
        public virtual Category? ParentCategory { get; set; }

        public virtual ICollection<Category>? ChildCategories { get; set; }
        public virtual ICollection<NewsArticle>? NewsArticles { get; set; }
    }
}