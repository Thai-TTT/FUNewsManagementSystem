using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.DataAccess.Repositories
{
    public interface ISystemAccountRepository : IRepository<SystemAccount>
    {
        Task<SystemAccount?> GetAccountByEmailAsync(string email);
        Task<SystemAccount> LoginAsync(string email, string password);
        Task<bool> IsEmailExistsAsync(string email, short? excludeAccountId = null);
        Task<IEnumerable<SystemAccount>> SearchAccountsAsync(string searchTerm, int? role);
        Task<bool> HasCreatedArticlesAsync(short accountId);
        Task<IEnumerable<SystemAccount>> GetAccountsByRoleAsync(int role);
        Task<bool> ChangePasswordAsync(short accountId, string oldPassword, string newPassword);
    }
}