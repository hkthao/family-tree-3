#!/bin/bash
set -euo pipefail

# --- Config ---
SOLUTION_FILE="backend/backend.sln"
COVERAGE_OUTPUT_DIR="backend/artifacts/coverage"
THRESHOLD=75 # Define threshold here

# Clean old coverage
rm -rf "$COVERAGE_OUTPUT_DIR"
mkdir -p "$COVERAGE_OUTPUT_DIR"

# --- Step 1: Run dotnet test with coverage ---
echo "Running tests with coverage..."
dotnet test "$SOLUTION_FILE" \
  --collect "XPlat Code Coverage" \
  --results-directory "$COVERAGE_OUTPUT_DIR" \
  --logger "trx;LogFileName=test_results.trx" \
  --configuration Release \
  /p:CollectCoverage=true

TEST_EXIT_CODE=$?
if [ $TEST_EXIT_CODE -ne 0 ]; then
  echo "❌ Tests failed. Exiting."
  exit $TEST_EXIT_CODE
fi

# --- Step 2: Parse coverage report ---
COVERAGE_XML=$(find "$COVERAGE_OUTPUT_DIR" -name "coverage.cobertura.xml" | head -1)

if [ ! -f "$COVERAGE_XML" ]; then
  echo "❌ Coverage report not found: $COVERAGE_XML"
  exit 1
fi

# Extract line-rate from cobertura XML using portable grep/cut
LINE_RATE=$(grep -o 'line-rate="[^"]*"' "$COVERAGE_XML" | head -1 | cut -d'"' -f2)
COVERAGE_PERCENT=$(awk -v lr="$LINE_RATE" 'BEGIN { printf "%.2f", lr * 100 }')

# --- Step 3: Print summary table and check threshold ---
echo ""
echo "================ Coverage Report ================"
printf "| %-15s | %-10s |\n" "Metric" "Value"
echo "|-----------------|------------|"
printf "| %-15s | %-10s |\n" "Line Coverage" "$COVERAGE_PERCENT%"
echo "================================================="

if (( $(echo "$COVERAGE_PERCENT < $THRESHOLD" | bc -l) )); then
  echo "❌ Coverage ($COVERAGE_PERCENT%) is below threshold ($THRESHOLD%)."
  exit 1
else
  echo "✅ Coverage ($COVERAGE_PERCENT%) meets or exceeds threshold ($THRESHOLD%)."
fi

echo "🎯 Coverage process completed."