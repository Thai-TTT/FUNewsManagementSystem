using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement.WebApp.ViewModels
{
    public class CategoryViewModel
    {
        public short CategoryID { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters")]
        public string CategoryName { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
        public string CategoryDesciption { get; set; }

        public short? ParentCategoryID { get; set; }

        public bool IsActive { get; set; }

        // For display
        public string ParentCategoryName { get; set; }
        public int ArticleCount { get; set; }
        public bool CanDelete { get; set; }
    }
}