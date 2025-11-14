using System.Data;
using Dapper;
using Npgsql;
using UserService.DAL.Interfaces;
using UserService.DAL.Mappers;
using UserService.Domain.Entities;
using Microsoft.Extensions.Options;
using UserService.DAL.Constants;
using UserService.DAL.Models;
using UserService.SshConnection;

namespace UserService.DAL.Repositories;

public class UserRepository(IOptions<DatabaseSettings> databaseSettings) : IUserRepository
{
    private readonly string _connectionString = databaseSettings.Value.GetConnectionString();

    public async Task<User?> CreateUserAsync(User user, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        var userDbModel = UserDbMapper.ToUserDbModel(user);

        parameters.Add("p_id", dbType: DbType.Int32, direction: ParameterDirection.Output);
        parameters.Add("p_login", userDbModel.Login);
        parameters.Add("p_password", userDbModel.Password);
        parameters.Add("p_name", userDbModel.Name);
        parameters.Add("p_surname", userDbModel.Surname);
        parameters.Add("p_age", userDbModel.Age);

        try
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            await connection.ExecuteAsync(UserProcedures.Create, parameters, commandType: CommandType.StoredProcedure);

            var userId = parameters.Get<int?>("p_id");

            if (!userId.HasValue) return null;

            userDbModel.Id = userId.Value;
            return UserDbMapper.ToUser(userDbModel);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new InvalidOperationException("This login already exists", exception);
        }
    }

    public async Task<User?> GetUserByIdAsync(int id, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", id);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var result = await connection.QueryFirstOrDefaultAsync<UserDbModel>(
            UserProcedures.GetById, parameters, commandType: CommandType.Text);

        return result is null ? null : UserDbMapper.ToUser(result);
    }

    public async Task<User[]> GetUserByNameAsync(string name, string surname, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_name", name);
        parameters.Add("p_surname", surname);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var result = await connection.QueryAsync<UserDbModel>(
            UserProcedures.GetByName, parameters, commandType: CommandType.Text);

        return result.Select(UserDbMapper.ToUser).ToArray();
    }

    public async Task<bool> UpdateUserAsync(User user, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        var userDbModel = UserDbMapper.ToUserDbModel(user);

        parameters.Add("p_id", userDbModel.Id);
        parameters.Add("p_password", userDbModel.Password);
        parameters.Add("p_name", userDbModel.Name);
        parameters.Add("p_surname", userDbModel.Surname);
        parameters.Add("p_age", userDbModel.Age);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            UserProcedures.Update, parameters, commandType: CommandType.Text);

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteUserAsync(int id, CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("p_id", id);

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        var rowsAffected = await connection.ExecuteScalarAsync<int>(
            UserProcedures.Delete, parameters, commandType: CommandType.Text);

        return rowsAffected > 0;
    }
}