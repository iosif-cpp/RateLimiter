CREATE OR REPLACE PROCEDURE create_user(
    OUT p_id INT,
    p_login VARCHAR(50),
    p_password VARCHAR(100),
    p_name VARCHAR(50),
    p_surname VARCHAR(50),
    p_age INT
) LANGUAGE plpgsql
AS $$
    BEGIN
        INSERT INTO users (login, password, name, surname, age)
        VALUES (p_login, p_password, p_name, p_surname, p_age)
        RETURNING id INTO p_id;

        EXCEPTION
            WHEN unique_violation THEN
                RAISE EXCEPTION '23505: This login already exists';
    END;
$$