#!/bin/bash

# Define paths
SOLUTION_FILE="backend/backend.sln"
COVERAGE_OUTPUT_DIR="backend/artifacts/coverage"

# --- Step 1: Run Tests with Coverlet ---
echo "Running tests and collecting coverage..."
dotnet test "$SOLUTION_FILE" \
  --collect "XPlat Code Coverage" \
  --results-directory "$COVERAGE_OUTPUT_DIR" \
  --logger "trx;LogFileName=test_results.trx" \
  --configuration Release \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=json \
  /p:CoverletOutput="$COVERAGE_OUTPUT_DIR/" \
  /p:Threshold=80 \
  /p:ThresholdType=line \
  /p:ThresholdStat=total

TEST_EXIT_CODE=$?

if [ $TEST_EXIT_CODE -ne 0 ]; then
  echo "Error: Tests failed. Exiting."
  exit $TEST_EXIT_CODE
fi

echo "Tests passed. Extracting coverage percentage..."

# --- Step 2: Extract and Print Coverage Percentage ---
COVERAGE_FILE="$COVERAGE_OUTPUT_DIR/coverage.json"

if [ -f "$COVERAGE_FILE" ]; then
  COVERAGE_PERCENTAGE=$(grep -oP '"Total":\s*\K\d+\.\d+' "$COVERAGE_FILE" | head -1)
  if [ -n "$COVERAGE_PERCENTAGE" ]; then
    echo "Coverage: $COVERAGE_PERCENTAGE%"
  else
    echo "Warning: Could not extract coverage percentage from $COVERAGE_FILE."
  fi
else
  echo "Warning: Coverage JSON file not found at $COVERAGE_FILE."
fi

echo "Coverage process completed."