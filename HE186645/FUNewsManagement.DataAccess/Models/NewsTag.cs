using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUNewsManagement.DataAccess.Models
{
    [Table("NewsTag")]
    public class NewsTag
    {
        [Key, Column(Order = 0)]
        [StringLength(20)]
        public string? NewsArticleID { get; set; }

        [Key, Column(Order = 1)]
        public int TagID { get; set; }

        [ForeignKey("NewsArticleID")]
        public virtual NewsArticle NewsArticle { get; set; } = null!;

        [ForeignKey("TagID")]
        public virtual Tag Tag { get; set; } = null!;
    }
}