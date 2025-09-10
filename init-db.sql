-- Initialize the database with proper permissions
CREATE DATABASE tae_app;
GRANT ALL PRIVILEGES ON DATABASE tae_app TO postgres;

-- Ensure the postgres user can connect
ALTER USER postgres CREATEDB;
