CREATE OR REPLACE FUNCTION get_user_by_id (p_id INT)
RETURNS TABLE (
    id INT,
    login VARCHAR(50),
    password VARCHAR(100),
    name VARCHAR(50),
    surname VARCHAR(50),
    age INT
) AS $$
BEGIN
    RETURN QUERY
    SELECT u.id, u.login, u.password, u.name, u.surname, u.age
    FROM users u
    WHERE u.id = p_id;
END;
$$ LANGUAGE plpgsql;