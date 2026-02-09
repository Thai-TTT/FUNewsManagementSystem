using System.ComponentModel.DataAnnotations;
using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.WebApp.ViewModels
{
    public class NewsArticleViewModel
    {
        public string NewsArticleID { get; set; }

        [Required(ErrorMessage = "News Title is required")]
        [StringLength(400)]
        public string NewsTitle { get; set; }

        [Required(ErrorMessage = "Headline is required")]
        [StringLength(150)]
        public string Headline { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [StringLength(4000)]
        public string NewsContent { get; set; }

        [StringLength(400)]
        public string NewsSource { get; set; }

        [Required(ErrorMessage = "Category is required")]
        public short? CategoryID { get; set; }

        public bool NewsStatus { get; set; }

        public string CategoryName { get; set; }
        public string CreatedByName { get; set; }
        public string UpdatedByName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // For Create/Edit
        public List<Category> AvailableCategories { get; set; } = new List<Category>();
        public List<Tag> AvailableTags { get; set; } = new List<Tag>();
        public List<int> SelectedTagIds { get; set; } = new List<int>();
        public List<Tag> AssignedTags { get; set; } = new List<Tag>();
    }
}