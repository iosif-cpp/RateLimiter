using FluentValidation;
using UserService.AppLayer.Validators;
using UserService.DAL.Interfaces;
using UserService.Domain.Entities;

namespace UserService.AppLayer.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserValidator _validator;

    public UserService(IUserRepository userRepository, UserValidator validator)
    {
        _userRepository = userRepository;
        _validator = validator;
    }

    public async Task<User?> CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(user);
        if (!result.IsValid)
            throw new ValidationException(result.ToString());

        return await _userRepository.CreateUserAsync(user, cancellationToken);
    }

    public Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _userRepository.GetUserByIdAsync(id, cancellationToken);
    }

    public Task<User[]> GetUserByNameAsync(string name, string surname, CancellationToken cancellationToken)
    {
        return _userRepository.GetUserByNameAsync(name, surname, cancellationToken);
    }

    public Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        return _userRepository.UpdateUserAsync(user, cancellationToken);
    }

    public Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken)
    {
        return _userRepository.DeleteUserAsync(id, cancellationToken);
    }
}