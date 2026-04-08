#!/bin/bash
# Debug Uno Platform WASM app
set -e

PROJECT_PATH="InternetBankCalculator.csproj"
TARGET_FRAMEWORK="net10.0-browserwasm"

# Clean and build in Debug mode

dotnet clean "$PROJECT_PATH" -c Debug

dotnet build "$PROJECT_PATH" -c Debug

echo "Launching Uno WASM app in Debug mode..."
dotnet run -c Debug -f $TARGET_FRAMEWORK --project "$PROJECT_PATH"
