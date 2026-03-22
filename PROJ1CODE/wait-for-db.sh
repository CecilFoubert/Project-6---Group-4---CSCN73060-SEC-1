#!/bin/bash
# wait-for-db.sh - Wait for MySQL database to be ready before starting the application

set -e

# Database host is always 'mysql' from docker-compose service name
DB_HOST="${DB_HOST:-mysql}"
DB_PORT="${DB_PORT:-3306}"

echo "Waiting for MySQL database at $DB_HOST:$DB_PORT..."

# Wait for database to be ready
until nc -z -v -w30 "$DB_HOST" "$DB_PORT"
do
  echo "Waiting for database connection..."
  sleep 5
done

echo "Database is ready!"
echo "Starting application..."

# Execute the command passed as arguments
exec "$@"

