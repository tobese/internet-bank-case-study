#!/usr/bin/env bash
# demo-aws.sh — Live load test demo against AWS EKS (doggerbank.online)
#
# 1. Port-forwards Prometheus + Grafana from the EKS cluster to localhost
# 2. Imports the load-test dashboard into the cluster Grafana via API
# 3. Opens the dashboard in the browser
# 4. Runs k6 against https://doggerbank.online, pushing metrics to the
#    in-cluster Prometheus — kube-state-metrics shows real pod scaling

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
NAMESPACE="internet-bank"
DASHBOARD_JSON="${SCRIPT_DIR}/grafana/provisioning/dashboards/load-test-demo.json"
DASHBOARD_UID="internet-bank-demo"

# Use non-standard ports to avoid conflicting with local Docker Compose
PROMETHEUS_LOCAL="9091"
GRAFANA_LOCAL="3001"

GRAFANA_URL="http://localhost:${GRAFANA_LOCAL}"

# ── Preflight ──────────────────────────────────────────────────────────────
CURRENT_CTX=$(kubectl config current-context 2>/dev/null || echo "none")
echo "==> kubectl context: ${CURRENT_CTX}"
if [[ "${CURRENT_CTX}" != *"internet-bank"* && "${CURRENT_CTX}" != *"dogger"* && "${CURRENT_CTX}" != *"eks"* ]]; then
  echo "    Tip: if this isn't the EKS context, run:"
  echo "         kubectl config use-context <your-eks-context>"
  echo "    Continuing in 3s..."
  sleep 3
fi

# ── Port-forwards ──────────────────────────────────────────────────────────
cleanup() {
  echo ""
  echo "==> Cleaning up port-forwards..."
  [[ -n "${PF_PROM_PID:-}" ]]    && kill "${PF_PROM_PID}"    2>/dev/null || true
  [[ -n "${PF_GRAFANA_PID:-}" ]] && kill "${PF_GRAFANA_PID}" 2>/dev/null || true
}
trap cleanup EXIT INT TERM

echo "==> Port-forwarding Prometheus  localhost:${PROMETHEUS_LOCAL} → cluster:9090"
kubectl port-forward -n "${NAMESPACE}" svc/prometheus \
  "${PROMETHEUS_LOCAL}:9090" > /dev/null 2>&1 &
PF_PROM_PID=$!

echo "==> Port-forwarding Grafana     localhost:${GRAFANA_LOCAL} → cluster:3000"
kubectl port-forward -n "${NAMESPACE}" svc/grafana \
  "${GRAFANA_LOCAL}:3000" > /dev/null 2>&1 &
PF_GRAFANA_PID=$!

# ── Wait for readiness ─────────────────────────────────────────────────────
echo -n "==> Waiting for Prometheus"
until curl -sf "http://localhost:${PROMETHEUS_LOCAL}/-/ready" > /dev/null 2>&1; do
  sleep 2; printf "."
done
echo " ready"

echo -n "==> Waiting for Grafana"
until curl -sf "${GRAFANA_URL}/api/health" > /dev/null 2>&1; do
  sleep 2; printf "."
done
echo " ready"

# ── Import dashboard ───────────────────────────────────────────────────────
echo -n "==> Importing dashboard..."
PAYLOAD=$(python3 -c "
import json
with open('${DASHBOARD_JSON}') as f:
    d = json.load(f)
# Clear id so Grafana treats it as new / overwrite
d.pop('id', None)
print(json.dumps({'dashboard': d, 'overwrite': True, 'folderId': 0}))
")
curl -sf -X POST -u admin:admin \
  -H 'Content-Type: application/json' \
  -d "${PAYLOAD}" \
  "${GRAFANA_URL}/api/dashboards/db" > /dev/null
echo " done"

# Resolve slug (Grafana generates it from the title)
SLUG=$(curl -s -u admin:admin \
  "${GRAFANA_URL}/api/dashboards/uid/${DASHBOARD_UID}" \
  | python3 -c "import sys,json; print(json.load(sys.stdin)['meta']['url'].split('/')[-1])")
DASHBOARD_URL="${GRAFANA_URL}/d/${DASHBOARD_UID}/${SLUG}?refresh=5s&from=now-10m&to=now"

echo "==> Opening Grafana dashboard..."
open "${DASHBOARD_URL}"

sleep 3

# ── k6 ─────────────────────────────────────────────────────────────────────
echo "==> Running k6 load test against doggerbank.online (Ctrl-C to stop)..."
echo "    Metrics → http://localhost:${PROMETHEUS_LOCAL}/api/v1/write"
echo "    Dashboard → ${DASHBOARD_URL}"
echo ""

K6_PROMETHEUS_RW_SERVER_URL="http://localhost:${PROMETHEUS_LOCAL}/api/v1/write" \
K6_PROMETHEUS_RW_TREND_AS_NATIVE_HISTOGRAM="false" \
K6_PROMETHEUS_RW_PUSH_INTERVAL="10s" \
k6 run \
  --out experimental-prometheus-rw \
  -e BASE_URL=https://doggerbank.online \
  "${SCRIPT_DIR}/load-test.js"

echo "==> Load test complete."
