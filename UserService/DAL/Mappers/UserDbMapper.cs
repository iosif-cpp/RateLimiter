using Riok.Mapperly.Abstractions;
using UserService.DAL.Models;
using UserService.Domain.Entities;

namespace UserService.DAL.Mappers;

[Mapper]
public static partial class UserDbMapper
{
    public static partial UserDbModel ToUserDbModel(User user);
    public static partial User ToUser(UserDbModel userDbModel);
}