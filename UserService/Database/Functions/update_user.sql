CREATE OR REPLACE FUNCTION update_user(
    p_id INT,
    p_password VARCHAR(100),
    p_name VARCHAR(50),
    p_surname VARCHAR(50),
    p_age INT
) RETURNS INT LANGUAGE plpgsql
AS $$
DECLARE
updated_rows INT;
BEGIN
UPDATE users
SET
    password = p_password,
    name = p_name,
    surname = p_surname,
    age = p_age
WHERE id = p_id;

GET DIAGNOSTICS updated_rows = ROW_COUNT;
RETURN updated_rows;
END;
$$;