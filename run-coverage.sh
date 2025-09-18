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

# --- Step 2: Parse and aggregate coverage reports ---
echo "Aggregating coverage reports..."
COVERAGE_XML_FILES=$(find "$COVERAGE_OUTPUT_DIR" -name "coverage.cobertura.xml")

TOTAL_LINE_RATE=0
REPORT_COUNT=0

for xml_file in $COVERAGE_XML_FILES; do
  if [ -f "$xml_file" ]; then
    LINE_RATE=$(grep -o 'line-rate="[^"]*"' "$xml_file" | head -1 | cut -d'"' -f2)
    if [ -n "$LINE_RATE" ]; then
      TOTAL_LINE_RATE=$(awk -v tl="$TOTAL_LINE_RATE" -v lr="$LINE_RATE" 'BEGIN { print tl + lr }')
      REPORT_COUNT=$((REPORT_COUNT + 1))
    fi
  fi
done

if [ "$REPORT_COUNT" -eq 0 ]; then
  echo "❌ No coverage reports found to aggregate."
  exit 1
fi

AVERAGE_LINE_RATE=$(awk -v tl="$TOTAL_LINE_RATE" -v rc="$REPORT_COUNT" 'BEGIN { printf "%.4f", tl / rc }')
COVERAGE_PERCENT=$(awk -v alr="$AVERAGE_LINE_RATE" 'BEGIN { printf "%.2f", alr * 100 }')

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