#!/usr/bin/env bash
# demo.sh — Local Docker Compose load-test demo
#
# 1. Starts the full stack (Prometheus, Grafana, API, Redis, Postgres, …)
# 2. Waits until Grafana and the API are healthy
# 3. Opens the pre-built Grafana dashboard in the browser
# 4. Runs k6 with Prometheus remote-write so every metric appears live

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

GRAFANA_URL="http://localhost:3000"
DASHBOARD_URL="${GRAFANA_URL}/d/internet-bank-demo/internet-bank-e28094-live-load-test?refresh=5s&from=now-10m&to=now"
API_HEALTH="http://localhost:8282/actuator/health"

echo "==> Starting Internet Bank stack..."
docker compose -f "${SCRIPT_DIR}/docker-compose.yml" up -d

echo -n "==> Waiting for Prometheus"
until curl -sf "http://localhost:9090/-/ready" > /dev/null 2>&1; do
  sleep 2; printf "."
done
echo " ready"

echo -n "==> Waiting for Grafana"
until curl -sf "${GRAFANA_URL}/api/health" > /dev/null 2>&1; do
  sleep 2; printf "."
done
echo " ready"

echo -n "==> Waiting for API"
until curl -sf "${API_HEALTH}" > /dev/null 2>&1; do
  sleep 2; printf "."
done
echo " ready"

echo "==> Opening Grafana dashboard..."
if [[ "$(uname)" == "Darwin" ]]; then
  open "${DASHBOARD_URL}"
else
  xdg-open "${DASHBOARD_URL}" 2>/dev/null || echo "Open manually: ${DASHBOARD_URL}"
fi

# Brief pause so the dashboard loads before the test generates data
sleep 3

echo "==> Running k6 load test (Ctrl-C to stop)..."
K6_PROMETHEUS_RW_SERVER_URL="http://localhost:9090/api/v1/write" \
K6_PROMETHEUS_RW_TREND_AS_NATIVE_HISTOGRAM="false" \
K6_PROMETHEUS_RW_PUSH_INTERVAL="10s" \
k6 run \
  --out experimental-prometheus-rw \
  -e BASE_URL=http://localhost:8282 \
  "${SCRIPT_DIR}/load-test.js"

echo "==> Load test complete."
