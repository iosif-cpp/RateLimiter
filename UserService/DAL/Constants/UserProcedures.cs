namespace UserService.DAL.Constants;

public static class UserProcedures
{
    public const string Create = "create_user";
    public const string Update = "SELECT update_user(@p_id, @p_password, @p_name, @p_surname, @p_age)";
    public const string Delete = "SELECT delete_user(@p_id)";
    public const string GetById = "SELECT * FROM get_user_by_id(@p_id)";
    public const string GetByName = "SELECT * FROM get_user_by_name(@p_name, @p_surname)";
}