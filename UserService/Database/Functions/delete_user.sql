CREATE OR REPLACE FUNCTION delete_user(p_id INT)
RETURNS INT
LANGUAGE plpgsql
AS $$
DECLARE
    deleted_rows INT;
BEGIN
    DELETE FROM users
    WHERE id = p_id;

    GET DIAGNOSTICS deleted_rows = ROW_COUNT;
    RETURN deleted_rows;
END;
$$;