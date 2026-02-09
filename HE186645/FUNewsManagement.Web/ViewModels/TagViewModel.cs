using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement.WebApp.ViewModels
{
    public class TagViewModel
    {
        public int TagID { get; set; }

        [Required(ErrorMessage = "Tag Name is required")]
        [StringLength(50, ErrorMessage = "Tag Name cannot exceed 50 characters")]
        public string TagName { get; set; }

        [StringLength(400, ErrorMessage = "Note cannot exceed 400 characters")]
        public string Note { get; set; }

        // For display
        public int ArticleCount { get; set; }
        public bool CanDelete { get; set; }
    }
}