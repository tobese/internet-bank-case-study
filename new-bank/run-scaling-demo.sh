#!/usr/bin/env bash
# run-scaling-demo.sh
#
# Orchestrates the full autoscaling demo:
#   1. Temporarily lower HPA CPU threshold to 20% (demo visibility)
#   2. Start pod-count recorder in background
#   3. Run k6 load test (~/5 min) — peaks at 150 VUs
#   4. Wait 9 min for HPA scale-down cooldown (stabilizationWindowSeconds=300)
#   5. Restore HPA CPU threshold to 70%
#   6. Stop recorder & generate self-contained HTML chart
#
# Outputs (all in same directory as this script):
#   scaling-pods-<RUN_ID>.csv    — pod count timeline
#   scaling-k6-<RUN_ID>.json     — k6 NDJSON metrics
#   scaling-report-<RUN_ID>.html — self-contained Chart.js report
#
# Usage:
#   bash run-scaling-demo.sh
#   BASE_URL=https://... bash run-scaling-demo.sh   (optional override)

set -euo pipefail

DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
RUN_ID="$(date +%Y%m%d-%H%M%S)"
NS="internet-bank"
HPA="api-hpa"

POD_CSV="$DIR/scaling-pods-$RUN_ID.csv"
K6_JSON="$DIR/scaling-k6-$RUN_ID.json"
HTML_OUT="$DIR/scaling-report-$RUN_ID.html"

RECORDER_PID=""

cleanup() {
  echo ""
  echo "[cleanup] Stopping recorder (if running)..."
  [[ -n "$RECORDER_PID" ]] && kill "$RECORDER_PID" 2>/dev/null || true

  echo "[cleanup] Restoring HPA CPU threshold → 70%..."
  kubectl -n "$NS" patch hpa "$HPA" --type=merge -p \
    '{"spec":{"metrics":[
        {"type":"Resource","resource":{"name":"cpu","target":{"type":"Utilization","averageUtilization":70}}},
        {"type":"Resource","resource":{"name":"memory","target":{"type":"AverageValue","averageValue":"700Mi"}}}
    ]}}' 2>/dev/null || true
  echo "[cleanup] Done."
}
trap cleanup EXIT INT TERM

# ── 1. Lower HPA threshold ────────────────────────────────────────────────
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "[1/5] Lowering HPA CPU threshold → 20% (was 70%)..."
kubectl -n "$NS" patch hpa "$HPA" --type=merge -p \
  '{"spec":{"metrics":[
      {"type":"Resource","resource":{"name":"cpu","target":{"type":"Utilization","averageUtilization":20}}},
      {"type":"Resource","resource":{"name":"memory","target":{"type":"AverageValue","averageValue":"700Mi"}}}
  ]}}'
echo "  Done. Current HPA:"
kubectl -n "$NS" get hpa "$HPA"

# ── 2. Start pod recorder ─────────────────────────────────────────────────
echo ""
echo "[2/5] Starting pod-count recorder → $POD_CSV"
bash "$DIR/record-pods.sh" "$POD_CSV" &
RECORDER_PID=$!
echo "  Recorder PID: $RECORDER_PID"

# Give recorder time to write its first few rows before load starts
sleep 10

# ── 3. Run k6 load test ───────────────────────────────────────────────────
echo ""
echo "[3/5] Running k6 load test (peaks at 150 VUs, ~5 min)..."
echo "  Output: $K6_JSON"
k6 run \
  --out "json=$K6_JSON" \
  ${BASE_URL:+--env BASE_URL="$BASE_URL"} \
  "$DIR/load-test.js" || true   # don't abort demo if thresholds fail

echo ""
echo "[3/5] k6 finished. Recording continues..."

# ── 4. Wait for scale-down ────────────────────────────────────────────────
WAIT_SECS=540   # 9 min: HPA stabilization (5min) + pod termination grace (~1min) + buffer
echo ""
echo "[4/5] Waiting ${WAIT_SECS}s for HPA scale-down cooldown..."
echo "  (stabilizationWindowSeconds=300 + termination margin)"

# Print pod count every 30s while waiting
WAITED=0
while (( WAITED < WAIT_SECS )); do
  sleep 30
  WAITED=$((WAITED + 30))
  PODS_READY=$(kubectl -n "$NS" get pods -l app=api --no-headers 2>/dev/null | grep -c "1/1" || echo "?")
  HPA_STATUS=$(kubectl -n "$NS" get hpa "$HPA" --no-headers 2>/dev/null || echo "?")
  echo "  [+${WAITED}s] ready pods: $PODS_READY  |  $HPA_STATUS"
done

# ── 5. Restore HPA + stop recorder ───────────────────────────────────────
echo ""
echo "[5/5] Restoring HPA CPU threshold → 70%..."
kubectl -n "$NS" patch hpa "$HPA" --type=merge -p \
  '{"spec":{"metrics":[
      {"type":"Resource","resource":{"name":"cpu","target":{"type":"Utilization","averageUtilization":70}}},
      {"type":"Resource","resource":{"name":"memory","target":{"type":"AverageValue","averageValue":"700Mi"}}}
  ]}}'

echo "  Stopping recorder (PID $RECORDER_PID)..."
kill "$RECORDER_PID" 2>/dev/null || true
RECORDER_PID=""

# ── Generate HTML report ──────────────────────────────────────────────────
echo ""
echo "[report] Extracting VU metrics from k6 output..."
K6_VUS_JSON="${K6_JSON%.json}-vus-only.json"
grep '"metric":"vus"' "$K6_JSON" > "$K6_VUS_JSON" || true

echo "[report] Generating HTML chart..."
python3 "$DIR/plot-scaling.py" "$POD_CSV" "$K6_VUS_JSON" "$HTML_OUT"

echo ""
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo "  DEMO COMPLETE"
echo "  Pod CSV   : $POD_CSV"
echo "  k6 JSON   : $K6_JSON"
echo "  HTML chart: $HTML_OUT"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"

# Open the report in the default browser (macOS)
open "$HTML_OUT" 2>/dev/null || xdg-open "$HTML_OUT" 2>/dev/null || true
