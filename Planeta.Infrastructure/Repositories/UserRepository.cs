using Microsoft.EntityFrameworkCore;
using Planeta.Application.Interfaces;
using Planeta.Domain.Auth;
using Planeta.Infrastructure.Persistence;

namespace Planeta.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly PlanetaDbContext _dbContext;

    public UserRepository(PlanetaDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user;
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
        return user;
    }

    public async Task<User?> GetUserByIdWithRolesAsync(int id)
    {
        var user = await _dbContext.Users
            .Include(u => u.Role)
            .ThenInclude(r => r.Permissions)
            .FirstOrDefaultAsync(u => u.Id == id);
        
        return user;
    }

    public async Task AddAsync(User user)
    { 
        _dbContext.Users.Add(user);
        await  _dbContext.SaveChangesAsync();
    }

    public async Task<bool> CheckExistingEmailAsync(string email)
    {
        return await _dbContext.Users.AnyAsync(u => u.Email == email);
    }

    public async Task SaveChangesAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> GetRoleIdByNameAsync(string rolename)
    {
        var role = await _dbContext.Roles.FirstOrDefaultAsync(r => r.Name == rolename);

        if(role == null)
        {
            throw new Exception($"Role with name '{rolename}' not found.");
        }

        return role.Id;
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        return await _dbContext.Users.ToListAsync();
    }
}