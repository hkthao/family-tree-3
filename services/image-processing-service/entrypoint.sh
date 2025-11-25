#!/bin/bash
set -e

# Check if the first argument is "pytest"
if [ "$1" = "pytest" ]; then
  echo "Running tests..."
  exec pytest "$@"
else
  echo "Starting Uvicorn server with arguments: $*"
  exec uvicorn "$@" --host "0.0.0.0" --port "8000"
fi
