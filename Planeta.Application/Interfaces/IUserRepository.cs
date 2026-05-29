using Planeta.Domain.Auth;

namespace Planeta.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserByIdWithRolesAsync(int id);
    Task AddAsync(User user);
    Task<bool> CheckExistingEmailAsync(string email);
    Task<int> GetRoleIdByNameAsync(string rolename);
    Task<IEnumerable<User>> GetUsersAsync();
    Task SaveChangesAsync();
}