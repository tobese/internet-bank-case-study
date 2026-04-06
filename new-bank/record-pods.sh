#!/usr/bin/env bash
# Polls API pod count every 5s and writes a CSV for the scaling demo.
# Usage: bash record-pods.sh <output.csv>

OUTPUT="${1:-pod-scaling.csv}"
echo "timestamp,elapsed_s,ready,total,hpa_desired,hpa_current" > "$OUTPUT"
START=$(date +%s)

echo "Recording pod counts → $OUTPUT  (Ctrl-C to stop)"

while true; do
  NOW=$(date +%s)
  ELAPSED=$((NOW - START))

  READY=$(kubectl -n internet-bank get pods -l app=api \
    --no-headers 2>/dev/null | grep -c "1/1" || echo 0)
  TOTAL=$(kubectl -n internet-bank get pods -l app=api \
    --no-headers 2>/dev/null | grep -c "" || echo 0)

  HPA_LINE=$(kubectl -n internet-bank get hpa api-hpa \
    --no-headers 2>/dev/null || echo "")
  HPA_DESIRED=$(echo "$HPA_LINE" | awk '{print $6}')
  HPA_CURRENT=$(echo "$HPA_LINE" | awk '{print $7}')
  HPA_DESIRED=${HPA_DESIRED:-0}
  HPA_CURRENT=${HPA_CURRENT:-0}

  ROW="$(date -u +%Y-%m-%dT%H:%M:%SZ),$ELAPSED,$READY,$TOTAL,$HPA_DESIRED,$HPA_CURRENT"
  echo "$ROW" | tee -a "$OUTPUT"
  sleep 5
done
