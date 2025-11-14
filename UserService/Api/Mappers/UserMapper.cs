using Riok.Mapperly.Abstractions;
using UserService.Domain.Entities;

namespace UserService.Api.Mappers;


[Mapper]
public static partial class UserMapper
{
    public static partial UserDto ToDto(this User user);
    
    public static partial User ToEntity(this UserDto dto);
    
    public static partial User ToEntity(this CreateUserRequest request);
    
    [MapperIgnoreTarget(nameof(User.Id))]
    [MapperIgnoreTarget(nameof(User.Login))]
    public static partial void UpdateFromRequest(this UpdateUserRequest request, User user);
}