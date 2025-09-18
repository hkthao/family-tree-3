#!/bin/bash
set -e

# ------------------------------
# Paths
# ------------------------------
SOLUTION_FILE="backend/backend.sln"
COVERAGE_OUTPUT_DIR="backend/artifacts/coverage"
COVERLET_TOOL="./.coverlet/coverlet" # Path to coverlet.console executable

# Array of unit test projects
TEST_PROJECTS=(
    "backend/tests/Application.UnitTests/Application.UnitTests.csproj"
    "backend/tests/Domain.UnitTests/Domain.UnitTests.csproj"
    # "backend/tests/Infrastructure.IntegrationTests/Infrastructure.IntegrationTests.csproj" # Excluded as per user's request for unit tests only.
)

# ------------------------------
# Create coverage dir
# ------------------------------
echo "Cleaning and creating coverage directory: $COVERAGE_OUTPUT_DIR"
rm -rf "$COVERAGE_OUTPUT_DIR"
mkdir -p "$COVERAGE_OUTPUT_DIR"

# ------------------------------
# Restore dependencies
# ------------------------------
echo "Restoring dependencies for $SOLUTION_FILE..."
dotnet restore "$SOLUTION_FILE"

# ------------------------------
# Run tests with coverage using coverlet.console
# ------------------------------
echo "Running unit tests with code coverage using coverlet.console..."

# Store paths to generated coverage files
GENERATED_COVERAGE_FILES=()

for PROJECT in "${TEST_PROJECTS[@]}"; do
    PROJECT_NAME=$(basename "$PROJECT" .csproj)
    OUTPUT_FILE="$COVERAGE_OUTPUT_DIR/$PROJECT_NAME.opencover.xml"
    LOG_FILE="$COVERAGE_OUTPUT_DIR/$PROJECT_NAME.log"

    echo "  -> Building $PROJECT_NAME to get test assembly path..."
    # Get the full path to the test assembly DLL
    TEST_ASSEMBLY_PATH=$(dotnet build "$PROJECT" --configuration Release --no-restore --no-dependencies --getProperty:TargetPath)
    TEST_ASSEMBLY_PATH=$(echo "$TEST_ASSEMBLY_PATH" | xargs) # Trim whitespace

    if [ -z "$TEST_ASSEMBLY_PATH" ]; then
        echo "  -> ERROR: Could not determine test assembly path for $PROJECT_NAME. Skipping."
        continue
    fi

    echo "  -> Test assembly path: $TEST_ASSEMBLY_PATH"
    echo "  -> Running coverlet.console for $PROJECT_NAME. Output logged to $LOG_FILE"

    # Run coverlet.console directly on the test assembly
    "$COVERLET_TOOL" "$TEST_ASSEMBLY_PATH" \
        --target "dotnet" \
        --targetargs "test \"$PROJECT\" --no-build --configuration Release" \
        --format opencover \
        --output "$OUTPUT_FILE" > "$LOG_FILE" 2>&1

    if [ -f "$OUTPUT_FILE" ]; then
        echo "  -> Found coverage file: $OUTPUT_FILE"
        GENERATED_COVERAGE_FILES+=("$OUTPUT_FILE")
    else
        echo "  -> WARNING: Could not find coverage.opencover.xml for $PROJECT_NAME at $OUTPUT_FILE. Check $LOG_FILE for details."
    fi
done

echo "Code coverage generation complete. Processed files: ${GENERATED_COVERAGE_FILES[@]}"
echo "Please use Gemini CLI to read these files and generate a summary."

echo "Coverage run completed!"
