using Grpc.Core;
using Microsoft.Extensions.Logging;
using UserService.Api.Mappers;
using System.Threading.Tasks;
using FluentValidation;

namespace UserService.Api.Services;

public class UserServiceRpc : UserServiceApi.UserServiceApiBase
{
    private readonly ILogger<UserServiceRpc> _logger;
    private readonly AppLayer.Services.UserService _userService;

    public UserServiceRpc(ILogger<UserServiceRpc> logger, AppLayer.Services.UserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    public override async Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        try
        {
            var user = request.ToEntity();
            var createdUser = await _userService.CreateUserAsync(user, context.CancellationToken);

            if (createdUser == null)
            {
                return new CreateUserResponse
                {
                    Success = false,
                    ErrorMessage = "Failed to create user"
                };
            }

            return new CreateUserResponse
            {
                Success = true,
                User = createdUser.ToDto()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
            return new CreateUserResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(request.Id, context.CancellationToken);

            if (user == null)
            {
                return new GetUserByIdResponse
                {
                    Found = false
                };
            }

            return new GetUserByIdResponse
            {
                Found = true,
                User = user.ToDto()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by id: {Id}", request.Id);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<GetUserByNameResponse> GetUserByName(GetUserByNameRequest request,
        ServerCallContext context)
    {
        try
        {
            var users = await _userService.GetUserByNameAsync(request.Name, request.Surname, context.CancellationToken);

            var response = new GetUserByNameResponse();
            response.Users.AddRange(users.Select(u => u.ToDto()));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by name: {Name} {Surname}", request.Name, request.Surname);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        try
        {
            var existingUser = await _userService.GetUserByIdAsync(request.Id, context.CancellationToken);
            if (existingUser == null)
            {
                return new UpdateUserResponse
                {
                    Success = false,
                    ErrorMessage = $"User with id {request.Id} not found"
                };
            }

            request.UpdateFromRequest(existingUser);

            var success = await _userService.UpdateUserAsync(existingUser, context.CancellationToken);

            return new UpdateUserResponse
            {
                Success = success,
                ErrorMessage = success ? string.Empty : "Failed to update user"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {Id}", request.Id);
            return new UpdateUserResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(request.Id, context.CancellationToken);

            return new DeleteUserResponse
            {
                Success = success
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {Id}", request.Id);
            return new DeleteUserResponse
            {
                Success = false
            };
        }
    }
}