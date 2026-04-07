#!/usr/bin/env bash
# Builds the Uno Platform web client (WASM) without Docker.
# Output: multi-client/InternetBankCalculator/InternetBankCalculator/bin/Release/net10.0-browserwasm/publish/wwwroot/
#
# Requirements: .NET 10 SDK + wasm-tools workload
#   brew install --cask dotnet-sdk   (if not already installed)
#   dotnet workload install wasm-tools

set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CLIENT_DIR="$SCRIPT_DIR/multi-client/InternetBankCalculator/InternetBankCalculator"

# Verify NuGet package cache is accessible (symlink may point to an unmounted external drive)
NUGET_LINK="$HOME/.nuget/packages"
if [[ -L "$NUGET_LINK" && ! -e "$NUGET_LINK" ]]; then
  export NUGET_PACKAGES="/tmp/nuget-packages"
  echo -e "${YELLOW}⚠️  ~/.nuget/packages symlink is broken — using $NUGET_PACKAGES as fallback cache${NC}"
  echo -e "${YELLOW}    Mount /Volumes/DockerData to restore normal operation${NC}"
fi

echo -e "${BLUE}── Web client (WASM) build ─────────────────────────${NC}"

# Check dotnet
if ! command -v dotnet &>/dev/null; then
  echo -e "${RED}❌ dotnet not found. Install from https://dot.net or: brew install --cask dotnet-sdk${NC}"
  exit 1
fi
DOTNET_VERSION=$(dotnet --version)
echo -e "${GREEN}✅ .NET $DOTNET_VERSION${NC}"

# Check wasm-tools workload
if ! dotnet workload list 2>/dev/null | grep -q 'wasm-tools'; then
  echo -e "${YELLOW}⚠️  wasm-tools workload not installed. Installing...${NC}"
  dotnet workload install wasm-tools
fi
echo -e "${GREEN}✅ wasm-tools workload present${NC}"

CSPROJ="$CLIENT_DIR/InternetBankCalculator.csproj"

echo -e "${YELLOW}📦 Restoring NuGet packages...${NC}"
dotnet restore "$CSPROJ" -p:TargetFramework=net10.0-browserwasm

echo -e "${YELLOW}🔨 Publishing WASM...${NC}"
dotnet publish "$CSPROJ" \
  -c Release \
  -f net10.0-browserwasm \
  -o "$CLIENT_DIR/bin/Release/net10.0-browserwasm/publish" \
  -p:WasmShellMonoRuntimeExecutionMode=Interpreter \
  --nologo \
  -v minimal

WWWROOT="$CLIENT_DIR/bin/Release/net10.0-browserwasm/publish/wwwroot"
if [[ ! -d "$WWWROOT" ]]; then
  echo -e "${RED}❌ Build failed — wwwroot not found${NC}"
  exit 1
fi

echo -e "${GREEN}✅ Built: $WWWROOT${NC}"

# ── Copy source files for display in the web client ─────────────────────────
echo -e "${BLUE}── Copying source files for /files/ ───────────────────${NC}"
FILES_DIR="$SCRIPT_DIR/multi-client/files"
mkdir -p "$FILES_DIR"
cp "$SCRIPT_DIR/api-application/Dockerfile"    "$FILES_DIR/api.Dockerfile"
cp "$SCRIPT_DIR/multi-client/Dockerfile"       "$FILES_DIR/web-client.Dockerfile"
cp "$SCRIPT_DIR/docker-compose.yml"            "$FILES_DIR/docker-compose.yml"
cp "$SCRIPT_DIR/k8s/api-deployment.yaml"       "$FILES_DIR/k8s-api-deployment.yaml"
cp "$SCRIPT_DIR/k8s/api-hpa.yaml"              "$FILES_DIR/k8s-api-hpa.yaml"
cp "$SCRIPT_DIR/k8s/postgres-statefulset.yaml" "$FILES_DIR/k8s-postgres-statefulset.yaml"
python3 "$SCRIPT_DIR/multi-client/generate-file-viewers.py" "$FILES_DIR"
echo -e "${GREEN}✅ Source files ready in multi-client/files/${NC}"
echo ""
echo -e "${BLUE}Serve locally (requires .NET or any static file server):${NC}"
echo "  cd \"$WWWROOT\" && python3 -m http.server 8080"
echo "  Then visit: http://localhost:8080"
