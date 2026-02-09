using FUNewsManagement.DataAccess.Models;
using FUNewsManagement.DataAccess.Repositories;

namespace FUNewsManagement.BusinessLogic.Services
{
    public class SystemAccountService
    {
        private readonly ISystemAccountRepository _accountRepo;

        public SystemAccountService(ISystemAccountRepository accountRepo)
        {
            _accountRepo = accountRepo;
        }

        public async Task<SystemAccount> LoginAsync(string email, string password)
        {
            return await _accountRepo.LoginAsync(email, password);
        }

        public async Task<SystemAccount> GetAccountByIdAsync(short id)
        {
            return await _accountRepo.GetByIdAsync(id);
        }

        public async Task<SystemAccount> GetAccountByEmailAsync(string email)
        {
            return await _accountRepo.GetAccountByEmailAsync(email);
        }

        public async Task<IEnumerable<SystemAccount>> GetAllAccountsAsync()
        {
            return await _accountRepo.GetAllAsync();
        }

        public async Task<bool> CreateAccountAsync(SystemAccount account)
        {
            try
            {
                // Validation
                if (string.IsNullOrWhiteSpace(account.AccountEmail))
                    return false;

                if (string.IsNullOrWhiteSpace(account.AccountPassword))
                    return false;

                // ✅ Validate với int? (vì Entity là int?)
                if (!account.AccountRole.HasValue)
                    return false;

                if (account.AccountRole.Value != 1 && account.AccountRole.Value != 2)
                    return false;

                // Check duplicate email
                if (await _accountRepo.IsEmailExistsAsync(account.AccountEmail))
                    return false;

                // TẠO ACCOUNT ID MỚI
                account.AccountID = await GenerateNewAccountIdAsync();

                await _accountRepo.AddAsync(account);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating account: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateAccountAsync(SystemAccount account)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(account.AccountEmail))
                    return false;

                if (!account.AccountRole.HasValue)
                    return false;

                if (account.AccountRole.Value != 1 && account.AccountRole.Value != 2)
                    return false;

                if (await _accountRepo.IsEmailExistsAsync(account.AccountEmail, account.AccountID))
                    return false;

                await _accountRepo.UpdateAsync(account);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating account: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAccountAsync(short id)
        {
            try
            {
                if (await _accountRepo.HasCreatedArticlesAsync(id))
                    return false;

                var account = await _accountRepo.GetByIdAsync(id);
                if (account == null)
                    return false;

                await _accountRepo.DeleteAsync(account);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting account: {ex.Message}");
                return false;
            }
        }

        public async Task<IEnumerable<SystemAccount>> SearchAccountsAsync(string searchTerm, int? role)
        {
            return await _accountRepo.SearchAccountsAsync(searchTerm, role);
        }

        public async Task<bool> CanDeleteAccountAsync(short id)
        {
            return !await _accountRepo.HasCreatedArticlesAsync(id);
        }

        public async Task<bool> ChangePasswordAsync(short accountId, string oldPassword, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword))
                return false;

            return await _accountRepo.ChangePasswordAsync(accountId, oldPassword, newPassword);
        }

        public async Task<bool> IsEmailExistsAsync(string email, short? excludeAccountId = null)
        {
            return await _accountRepo.IsEmailExistsAsync(email, excludeAccountId);
        }

        public async Task<IEnumerable<SystemAccount>> GetAccountsByRoleAsync(int role)
        {
            return await _accountRepo.GetAccountsByRoleAsync(role);
        }

        // PRIVATE: Generate new AccountID
        private async Task<short> GenerateNewAccountIdAsync()
        {
            try
            {
                var accounts = await _accountRepo.GetAllAsync();

                if (!accounts.Any())
                    return 1;

                short maxId = accounts.Max(a => a.AccountID);
                return (short)(maxId + 1);
            }
            catch
            {
                // Fallback: return 100 nếu error
                return 100;
            }
        }
    }
}
