#!/bin/zsh

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/InternetBankCalculator/InternetBankCalculator"

echo "Starting InternetBankCalculator Web Client (WebAssembly)..."
dotnet run --project "$PROJECT_DIR" --framework net10.0-browserwasm
