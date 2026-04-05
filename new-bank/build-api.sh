#!/usr/bin/env bash
# Builds the Java API fat-jar without Docker.
# Output: api-application/target/math-server-0.0.1-SNAPSHOT.jar
#
# Requirements: Java 21+ and Maven 3.9+ on PATH
#   brew install openjdk@21 maven   (if not already installed)

set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
API_DIR="$SCRIPT_DIR/api-application"

echo -e "${BLUE}── Java API build ──────────────────────────────────${NC}"

# Check Java
if ! command -v java &>/dev/null; then
  echo -e "${RED}❌ java not found. Install with: brew install openjdk@21${NC}"
  exit 1
fi
JAVA_VERSION=$(java -version 2>&1 | awk -F '"' '/version/ {print $2}' | cut -d'.' -f1)
if [[ "$JAVA_VERSION" -lt 21 ]]; then
  echo -e "${RED}❌ Java 21+ required (found $JAVA_VERSION)${NC}"
  exit 1
fi
echo -e "${GREEN}✅ Java $JAVA_VERSION${NC}"

# Check Maven
if command -v mvn &>/dev/null; then
  MVN=mvn
elif [[ -x "$API_DIR/mvnw" ]]; then
  MVN="$API_DIR/mvnw"
else
  echo -e "${RED}❌ Maven not found. Install with: brew install maven${NC}"
  exit 1
fi
echo -e "${GREEN}✅ Maven: $MVN${NC}"

echo -e "${YELLOW}🔨 Compiling and packaging...${NC}"
cd "$API_DIR"
$MVN clean package -DskipTests -q

JAR=$(ls target/math-server-*.jar 2>/dev/null | head -1)
if [[ -z "$JAR" ]]; then
  echo -e "${RED}❌ Build failed — no jar found in target/${NC}"
  exit 1
fi

echo -e "${GREEN}✅ Built: api-application/$JAR${NC}"
echo ""
echo -e "${BLUE}Run locally (requires PostgreSQL):${NC}"
echo "  DB_HOST=localhost DB_USER=bankuser DB_PASSWORD=bankpass123 \\"
echo "    java -jar $API_DIR/$JAR"
