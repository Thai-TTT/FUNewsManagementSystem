using Microsoft.AspNetCore.Mvc;
using FUNewsManagement.BusinessLogic.Services;
using FUNewsManagement.DataAccess.Models;
using FUNewsManagement.WebApp.ViewModels;
using FUNewsManagement.WebApp.Helpers;

namespace FUNewsManagement.WebApp.Controllers
{
    [AuthorizeRole("Admin")]
    public class SystemAccountController : Controller
    {
        private readonly SystemAccountService _accountService;

        public SystemAccountController(SystemAccountService accountService)
        {
            _accountService = accountService;
        }

        // GET: SystemAccount
        public async Task<IActionResult> Index(string searchTerm, int? role)
        {
            var accounts = await _accountService.SearchAccountsAsync(searchTerm, role);

            var viewModels = new List<SystemAccountViewModel>();
            foreach (var account in accounts)
            {
                var canDelete = await _accountService.CanDeleteAccountAsync(account.AccountID);
                viewModels.Add(new SystemAccountViewModel
                {
                    AccountID = account.AccountID,
                    AccountName = account.AccountName,
                    AccountEmail = account.AccountEmail,
                    AccountRole = account.AccountRole ?? 0,
                    RoleName = GetRoleName(account.AccountRole ?? 0),
                    CanDelete = canDelete
                });
            }

            ViewBag.SearchTerm = searchTerm;
            ViewBag.SelectedRole = role;

            return View(viewModels);
        }

        // GET: SystemAccount/Details/5 - Lấy thông tin để hiển thị trong modal
        public async Task<IActionResult> Details(short id)
        {
            var account = await _accountService.GetAccountByIdAsync(id);
            if (account == null)
            {
                return NotFound();
            }

            var viewModel = new SystemAccountViewModel
            {
                AccountID = account.AccountID,
                AccountName = account.AccountName,
                AccountEmail = account.AccountEmail,
                AccountRole = account.AccountRole ?? 0,
                RoleName = GetRoleName(account.AccountRole ?? 0)
            };

            return Json(viewModel);
        }

        // POST: SystemAccount/Create - Tạo tài khoản Staff/Lecturer (Admin only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SystemAccountViewModel model)
        {
            // Validate Role - chỉ cho phép Staff (1) hoặc Lecturer (2)
            if (model.AccountRole != 1 && model.AccountRole != 2)
            {
                return Json(new { success = false, message = "Invalid role. Only Staff (1) or Lecturer (2) are allowed." });
            }

            // Validate Email
            if (string.IsNullOrWhiteSpace(model.AccountEmail))
            {
                return Json(new { success = false, message = "Email is required" });
            }

            // Check duplicate email
            if (await _accountService.IsEmailExistsAsync(model.AccountEmail))
            {
                return Json(new { success = false, message = "Email already exists" });
            }

            // Validate password
            if (string.IsNullOrEmpty(model.AccountPassword) || model.AccountPassword.Length < 2)
            {
                return Json(new { success = false, message = "Password must be at least 2 characters" });
            }

            // Validate name
            if (string.IsNullOrWhiteSpace(model.AccountName))
            {
                return Json(new { success = false, message = "Account Name is required" });
            }

            var account = new SystemAccount
            {
                AccountName = model.AccountName,
                AccountEmail = model.AccountEmail,
                AccountRole = model.AccountRole,
                AccountPassword = model.AccountPassword
            };

            var result = await _accountService.CreateAccountAsync(account);

            if (result)
            {
                return Json(new { success = true, message = "Account created successfully" });
            }

            return Json(new { success = false, message = "Failed to create account" });
        }

        // POST: SystemAccount/Edit - Sửa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SystemAccountViewModel model)
        {
            // Validate Role
            if (model.AccountRole != 1 && model.AccountRole != 2)
            {
                return Json(new { success = false, message = "Invalid role. Only Staff (1) or Lecturer (2) are allowed." });
            }

            var account = await _accountService.GetAccountByIdAsync(model.AccountID);
            if (account == null)
            {
                return Json(new { success = false, message = "Account not found" });
            }

            // Check duplicate email (trừ chính nó)
            if (await _accountService.IsEmailExistsAsync(model.AccountEmail, model.AccountID))
            {
                return Json(new { success = false, message = "Email already exists" });
            }

            // Validate name
            if (string.IsNullOrWhiteSpace(model.AccountName))
            {
                return Json(new { success = false, message = "Account Name is required" });
            }

            account.AccountName = model.AccountName;
            account.AccountEmail = model.AccountEmail;
            account.AccountRole = model.AccountRole;

            // Only update password if provided
            if (!string.IsNullOrEmpty(model.AccountPassword))
            {
                if (model.AccountPassword.Length < 2)
                {
                    return Json(new { success = false, message = "Password must be at least 2 characters" });
                }
                account.AccountPassword = model.AccountPassword;
            }

            var result = await _accountService.UpdateAccountAsync(account);

            if (result)
            {
                return Json(new { success = true, message = "Account updated successfully" });
            }

            return Json(new { success = false, message = "Failed to update account" });
        }

        // POST: SystemAccount/Delete/5 - Xóa tài khoản
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(short id)
        {
            // Kiểm tra xem account có tạo bài viết nào không
            var canDelete = await _accountService.CanDeleteAccountAsync(id);
            if (!canDelete)
            {
                return Json(new { success = false, message = "Cannot delete account that has created articles" });
            }

            var result = await _accountService.DeleteAccountAsync(id);

            if (result)
            {
                return Json(new { success = true, message = "Account deleted successfully" });
            }

            return Json(new { success = false, message = "Failed to delete account" });
        }

        // PRIVATE HELPER METHOD
        private string GetRoleName(int role)
        {
            return role switch
            {
                0 => "Administrator",
                1 => "Staff",
                2 => "Lecturer",
                _ => "Unknown"
            };
        }
    }
}