using Microsoft.AspNetCore.Mvc;
using FUNewsManagement.BusinessLogic.Services;
using FUNewsManagement.DataAccess.Models;
using FUNewsManagement.WebApp.ViewModels;
using FUNewsManagement.WebApp.Helpers;

namespace FUNewsManagement.WebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly SystemAccountService _accountService;
        private readonly IConfiguration _configuration;

        public AccountController(SystemAccountService accountService, IConfiguration configuration)
        {
            _accountService = accountService;
            _configuration = configuration;
        }
        // GET: Login
        [HttpGet]
        public IActionResult Login()
        {
            var account = HttpContext.Session.GetObjectFromJson<SystemAccount>("Account");
            if (account != null)
            {
                if (account.AccountRole == 0)
                    return RedirectToAction("Index", "SystemAccount");
                else if (account.AccountRole == 1)
                    return RedirectToAction("Index", "NewsArticle");
                else
                    return RedirectToAction("Index", "Home");
            }
            return View();
        }

        // POST: Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SystemAccount? account = null;

            var adminEmail = _configuration["AdminAccount:Email"];
            var adminPassword = _configuration["AdminAccount:Password"];

            if (model.Email == adminEmail && model.Password == adminPassword)
            {
                // Admin - KHÔNG LƯU TRONG DB
                account = new SystemAccount
                {
                    AccountID = 0,
                    AccountName = "Administrator",
                    AccountEmail = adminEmail,
                    AccountRole = 0 // Role 0 = Admin
                };
            }
            else
            {
                // Staff/Lecturer - TỪ DATABASE
                account = await _accountService.LoginAsync(model.Email, model.Password);
            }

            if (account != null)
            {
                HttpContext.Session.SetObjectAsJson("Account", account);
                // Redirect theo role
                if (account.AccountRole == 0) // Admin
                {
                    return RedirectToAction("Index", "SystemAccount");
                }
                else if (account.AccountRole == 1) // Staff
                {
                    return RedirectToAction("Index", "NewsArticle");
                }
                else // Lecturer (role = 2)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            TempData["ErrorMessage"] = "Invalid email or password";
            return View(model);
        }

        // GET: Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: Profile
        [AuthorizeRole("Staff", "Lecturer")]
        public async Task<IActionResult> Profile()
        {
            var account = HttpContext.Session.GetObjectFromJson<SystemAccount>("Account");
            if (account == null)
            {
                return RedirectToAction("Login");
            }

            // Admin không có profile
            if (account.AccountRole == 0)
            {
                var viewModel = new SystemAccountViewModel
                {
                    AccountID = 0,
                    AccountName = account.AccountName ?? string.Empty,
                    AccountEmail = account.AccountEmail ?? string.Empty,
                    AccountRole = 0,
                    RoleName = "Administrator"
                };
                return View(viewModel);
            }

            // Staff/Lecturer 
            var dbAccount = await _accountService.GetAccountByIdAsync(account.AccountID);
            if (dbAccount == null)
            {
                return RedirectToAction("Login");
            }

            var model = new SystemAccountViewModel
            {
                AccountID = dbAccount.AccountID,
                AccountName = dbAccount.AccountName ?? string.Empty,
                AccountEmail = dbAccount.AccountEmail ?? string.Empty,
                AccountRole = dbAccount.AccountRole ?? 0,
                RoleName = GetRoleName(dbAccount.AccountRole ?? 0)
            };

            return View(model);
        }

        // POST: Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Staff", "Lecturer")]
        public async Task<IActionResult> Profile(SystemAccountViewModel model)
        {
            var account = HttpContext.Session.GetObjectFromJson<SystemAccount>("Account");
            if (account == null)
            {
                return RedirectToAction("Login");
            }

            // Admin không update profile
            if (account.AccountRole == 0)
            {
                TempData["ErrorMessage"] = "Admin profile cannot be updated";
                return RedirectToAction("Profile");
            }

            // Chỉ validate AccountName
            if (string.IsNullOrWhiteSpace(model.AccountName))
            {
                TempData["ErrorMessage"] = "Account Name is required";
                model.RoleName = GetRoleName(model.AccountRole);
                return View(model);
            }

            var dbAccount = await _accountService.GetAccountByIdAsync(account.AccountID);
            if (dbAccount == null)
            {
                return RedirectToAction("Login");
            }

            // Update only name
            dbAccount.AccountName = model.AccountName;

            var result = await _accountService.UpdateAccountAsync(dbAccount);

            if (result)
            {
                // Update session
                account.AccountName = model.AccountName;
                HttpContext.Session.SetObjectAsJson("Account", account);

                TempData["SuccessMessage"] = "Profile updated successfully";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update profile";
            }

            return RedirectToAction("Profile");
        }

        // GET: ChangePassword
        [AuthorizeRole("Staff", "Lecturer")]
        public IActionResult ChangePassword()
        {
            var account = HttpContext.Session.GetObjectFromJson<SystemAccount>("Account");
            if (account == null)
            {
                return RedirectToAction("Login");
            }

            // Admin không đổi password
            if (account.AccountRole == 0)
            {
                TempData["ErrorMessage"] = "Admin password is managed in configuration file";
                return RedirectToAction("Index", "SystemAccount");
            }

            return View();
        }

        // POST: ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole("Staff", "Lecturer")]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var account = HttpContext.Session.GetObjectFromJson<SystemAccount>("Account");
            if (account == null)
            {
                return RedirectToAction("Login");
            }

            // Admin không đổi password qua đây
            if (account.AccountRole == 0)
            {
                TempData["ErrorMessage"] = "Admin password is managed in configuration file";
                return RedirectToAction("Index", "SystemAccount");
            }

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            {
                TempData["ErrorMessage"] = "All fields are required";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                TempData["ErrorMessage"] = "New password and confirmation do not match";
                return View();
            }

            if (newPassword.Length < 2)
            {
                TempData["ErrorMessage"] = "Password must be at least 2 characters";
                return View();
            }

            var result = await _accountService.ChangePasswordAsync(account.AccountID, oldPassword, newPassword);

            if (result)
            {
                TempData["SuccessMessage"] = "Password changed successfully";
                return RedirectToAction("Profile");
            }
            else
            {
                TempData["ErrorMessage"] = "Current password is incorrect";
                return View();
            }
        }

        // GET: AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
        }

        private string GetRoleName(int? role)
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