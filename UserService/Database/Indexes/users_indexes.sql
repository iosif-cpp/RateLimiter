CREATE INDEX IF NOT EXISTS idx_users_name_surname ON users(name, surname);
CREATE UNIQUE INDEX IF NOT EXISTS idx_unique_users_login ON users(login);