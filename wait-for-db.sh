#!/bin/bash
# wait-for-db.sh - Wait for MySQL database to be ready before starting the application

set -e

host="${ConnectionStrings__DefaultConnection#*Server=}"
host="${host%%;*}"

echo "Waiting for MySQL database at $host:3306..."

until nc -z -v -w30 mysql 3306
do
  echo "Waiting for database connection..."
  sleep 5
done

echo "Database is ready!"
echo "Starting application..."

# Execute the command passed as arguments
exec "$@"

