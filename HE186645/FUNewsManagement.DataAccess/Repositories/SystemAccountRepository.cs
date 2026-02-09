using Microsoft.EntityFrameworkCore;
using FUNewsManagement.DataAccess.Models;

namespace FUNewsManagement.DataAccess.Repositories
{
    public class SystemAccountRepository : Repository<SystemAccount>, ISystemAccountRepository
    {
        public SystemAccountRepository(FUNewsManagementContext context) : base(context)
        {
        }

        public async Task<SystemAccount?> GetAccountByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.AccountEmail == email);
        }

        public async Task<SystemAccount?> LoginAsync(string email, string password)
        {
            return await _dbSet
                .FirstOrDefaultAsync(a => a.AccountEmail == email && a.AccountPassword == password);
        }

        public async Task<bool> IsEmailExistsAsync(string email, short? excludeAccountId = null)
        {
            var query = _dbSet.Where(a => a.AccountEmail == email);

            if (excludeAccountId.HasValue)
            {
                query = query.Where(a => a.AccountID != excludeAccountId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<SystemAccount>> SearchAccountsAsync(string searchTerm, int? role)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(a =>
                    a.AccountName.Contains(searchTerm) ||
                    a.AccountEmail.Contains(searchTerm));
            }

            if (role.HasValue)
            {
                query = query.Where(a => a.AccountRole == role);
            }

            return await query.OrderBy(a => a.AccountName).ToListAsync();
        }

        public async Task<bool> HasCreatedArticlesAsync(short accountId)
        {
            return await _context.NewsArticles
                .AnyAsync(na => na.CreatedByID == accountId);
        }

        public async Task<IEnumerable<SystemAccount>> GetAccountsByRoleAsync(int role)
        {
            return await _dbSet
                .Where(a => a.AccountRole == role)
                .OrderBy(a => a.AccountName)
                .ToListAsync();
        }

        public async Task<bool> ChangePasswordAsync(short accountId, string oldPassword, string newPassword)
        {
            var account = await _dbSet.FindAsync(accountId);

            if (account == null || account.AccountPassword != oldPassword)
            {
                return false;
            }

            account.AccountPassword = newPassword;
            await _context.SaveChangesAsync();

            return true;
        }
    }
}