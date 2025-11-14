using UserService.Domain.Entities;

namespace UserService.DAL.Interfaces;

public interface IUserRepository
{
    Task<User?> CreateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User[]> GetUserByNameAsync(string name, string surname, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken = default);
}