using System.ComponentModel.DataAnnotations;

namespace FUNewsManagement.WebApp.ViewModels
{
    public class SystemAccountViewModel
    {
        public short AccountID { get; set; }

        [Required(ErrorMessage = "Account Name is required")]
        [StringLength(100)]
        public string AccountName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [StringLength(70)]
        public string AccountEmail { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [Range(1, 2, ErrorMessage = "Role must be Staff (1) or Lecturer (2)")]
        public int AccountRole { get; set; } // ✅ int - KHÔNG NULLABLE

        [Required(ErrorMessage = "Password is required")]
        [StringLength(70, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string AccountPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("AccountPassword", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } // KHÔNG [Required] vì khi edit không bắt buộc

        // Display only
        public string RoleName { get; set; }
        public bool CanDelete { get; set; }
    }
}