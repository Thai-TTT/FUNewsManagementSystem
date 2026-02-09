using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FUNewsManagement.DataAccess.Models
{
    [Table("SystemAccount")]
    public class SystemAccount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public short AccountID { get; set; }

        [StringLength(100)]
        public string? AccountName { get; set; }

        [Required]
        [StringLength(70)]
        [EmailAddress]
        public string? AccountEmail { get; set; }

        public int? AccountRole { get; set; }

        [Required]
        [StringLength(70)]
        public string? AccountPassword { get; set; }

        [InverseProperty("CreatedBy")]
        public virtual ICollection<NewsArticle> CreatedNewsArticles { get; set; } = new List<NewsArticle>();

        [InverseProperty("UpdatedBy")]
        public virtual ICollection<NewsArticle>? UpdatedNewsArticles { get; set; } 
    }
}