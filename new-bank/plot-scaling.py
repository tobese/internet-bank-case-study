#!/usr/bin/env python3
"""
plot-scaling.py  <pod-csv>  <k6-ndjson>  <output.html>

Reads the pod count CSV produced by record-pods.sh and the k6 NDJSON output,
then generates a self-contained HTML file with a Chart.js dual-axis chart:
  left axis  → API pod count (ready / desired)
  right axis → active VUs (from k6)
"""
import csv
import json
import sys
from datetime import datetime, timezone

# ---------------------------------------------------------------------------
# 1. Parse pod CSV
# ---------------------------------------------------------------------------
def parse_pods(path):
    rows = []
    with open(path) as f:
        reader = csv.DictReader(f)
        for row in reader:
            try:
                rows.append({
                    "ts": datetime.fromisoformat(row["timestamp"].replace("Z", "+00:00")),
                    "elapsed": int(row["elapsed_s"]),
                    "ready": int(row["ready"]),
                    "total": int(row["total"]),
                    "hpa_desired": int(row["hpa_desired"]) if row["hpa_desired"].isdigit() else 0,
                })
            except (ValueError, KeyError):
                pass
    return rows


# ---------------------------------------------------------------------------
# 2. Parse k6 NDJSON — extract only "vus" metric Points
# ---------------------------------------------------------------------------
def parse_k6_vus(path):
    """Returns list of (datetime, vu_count) sorted by time."""
    points = []
    with open(path) as f:
        for line in f:
            line = line.strip()
            if not line:
                continue
            try:
                obj = json.loads(line)
            except json.JSONDecodeError:
                continue
            if obj.get("metric") == "vus" and obj.get("type") == "Point":
                t_str = obj["data"]["time"]
                # k6 uses RFC3339 with nanoseconds like "2026-04-06T08:22:14.29863+00:00"
                # Strip to microseconds and normalise
                t_str = t_str.replace("Z", "+00:00")
                if "." in t_str:
                    # Split off the fractional part before the timezone offset
                    dot_idx = t_str.index(".")
                    tz_idx = t_str.index("+", dot_idx) if "+" in t_str[dot_idx:] else len(t_str)
                    frac = t_str[dot_idx + 1:tz_idx][:6].ljust(6, "0")  # pad/clip to microseconds
                    base = t_str[:dot_idx]
                    tz = t_str[tz_idx:]
                    t_str = f"{base}.{frac}{tz}"
                ts = datetime.fromisoformat(t_str)
                points.append((ts, int(obj["data"]["value"])))
    points.sort(key=lambda x: x[0])
    return points


# ---------------------------------------------------------------------------
# 3. Align timelines (both relative to pod recorder start)
# ---------------------------------------------------------------------------
def align(pod_rows, vus_points):
    if not pod_rows:
        return [], []

    origin = pod_rows[0]["ts"]

    pod_data = [
        {"x": r["elapsed"], "ready": r["ready"], "total": r["total"]}
        for r in pod_rows
    ]

    vu_data = []
    for ts, vu in vus_points:
        elapsed = round((ts - origin).total_seconds())
        vu_data.append({"x": elapsed, "y": vu})

    # Deduplicate VU points by keeping last value per elapsed second
    vu_by_sec = {}
    for pt in vu_data:
        vu_by_sec[pt["x"]] = pt["y"]
    vu_data = sorted(({"x": k, "y": v} for k, v in vu_by_sec.items()), key=lambda d: d["x"])

    return pod_data, vu_data


# ---------------------------------------------------------------------------
# 4. Detect scale-up and scale-down events for annotations
# ---------------------------------------------------------------------------
def events(pod_data):
    evts = []
    prev = pod_data[0]["total"] if pod_data else 0
    for pt in pod_data[1:]:
        cur = pt["total"]
        if cur > prev:
            evts.append({"x": pt["x"], "label": f"↑ {cur} pods", "color": "#22c55e"})
        elif cur < prev:
            evts.append({"x": pt["x"], "label": f"↓ {cur} pods", "color": "#f97316"})
        prev = cur
    return evts


# ---------------------------------------------------------------------------
# 5. Generate HTML
# ---------------------------------------------------------------------------
HTML_TEMPLATE = """\
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8"/>
  <title>API Autoscaling Demo</title>
  <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.3/dist/chart.umd.min.js"></script>
  <script src="https://cdn.jsdelivr.net/npm/chartjs-plugin-annotation@3.0.1/dist/chartjs-plugin-annotation.min.js"></script>
  <style>
    * {{ box-sizing: border-box; margin: 0; padding: 0; }}
    body {{ font-family: system-ui, sans-serif; background: #0f172a; color: #e2e8f0; padding: 2rem; }}
    h1 {{ font-size: 1.5rem; margin-bottom: 0.25rem; }}
    p.sub {{ color: #94a3b8; font-size: 0.9rem; margin-bottom: 2rem; }}
    .chart-wrap {{ background: #1e293b; border-radius: 0.75rem; padding: 1.5rem; }}
    canvas {{ max-height: 480px; }}
    .stats {{ display: flex; gap: 1.5rem; margin-top: 2rem; flex-wrap: wrap; }}
    .stat {{ background: #1e293b; border-radius: 0.75rem; padding: 1rem 1.5rem; flex: 1; min-width: 160px; }}
    .stat .val {{ font-size: 2rem; font-weight: 700; color: #38bdf8; }}
    .stat .lbl {{ font-size: 0.8rem; color: #94a3b8; margin-top: 0.25rem; }}
  </style>
</head>
<body>
  <h1>API Autoscaling Demo</h1>
  <p class="sub">Horizontal Pod Autoscaler in action — pod count vs load over time</p>
  <div class="chart-wrap">
    <canvas id="chart"></canvas>
  </div>
  <div class="stats">
    <div class="stat"><div class="val" id="stat-min">—</div><div class="lbl">Min pods</div></div>
    <div class="stat"><div class="val" id="stat-max">—</div><div class="lbl">Max pods</div></div>
    <div class="stat"><div class="val" id="stat-vu">—</div><div class="lbl">Peak VUs</div></div>
    <div class="stat"><div class="val" id="stat-dur">—</div><div class="lbl">Total elapsed</div></div>
  </div>

  <script>
  const podData   = {pod_data_json};
  const vuData    = {vu_data_json};
  const evtData   = {evt_data_json};

  // Fill stat cards
  const readyArr = podData.map(d => d.ready).filter(v => v > 0);
  const totalArr = podData.map(d => d.total).filter(v => v > 0);
  document.getElementById('stat-min').textContent = readyArr.length ? Math.min(...readyArr) : '—';
  document.getElementById('stat-max').textContent = totalArr.length ? Math.max(...totalArr) : '—';
  const vuArr = vuData.map(d => d.y);
  document.getElementById('stat-vu').textContent  = vuArr.length ? Math.max(...vuArr) : '—';
  const lastElapsed = podData.length ? podData[podData.length - 1].x : 0;
  const mins = Math.floor(lastElapsed / 60), secs = lastElapsed % 60;
  document.getElementById('stat-dur').textContent = `${{mins}}m ${{String(secs).padStart(2,'0')}}s`;

  // Annotation lines for scale events
  const annotations = {{}};
  evtData.forEach((e, i) => {{
    annotations[`line${{i}}`] = {{
      type: 'line', scaleID: 'x', value: e.x,
      borderColor: e.color, borderWidth: 1.5, borderDash: [5, 4],
      label: {{
        display: true, content: e.label, backgroundColor: e.color + 'cc',
        color: '#fff', font: {{ size: 11 }}, position: 'start', rotation: -90, xAdjust: 6,
      }},
    }};
  }});

  const ctx = document.getElementById('chart').getContext('2d');
  new Chart(ctx, {{
    data: {{
      datasets: [
        {{
          type: 'line',
          label: 'Ready pods',
          data: podData.map(d => ({{ x: d.x, y: d.ready }})),
          borderColor: '#38bdf8', backgroundColor: '#38bdf830',
          fill: true, stepped: 'before', pointRadius: 3, yAxisID: 'pods',
        }},
        {{
          type: 'line',
          label: 'Total pods (HPA requested)',
          data: podData.map(d => ({{ x: d.x, y: d.total }})),
          borderColor: '#a78bfa', backgroundColor: 'transparent',
          borderDash: [6, 4], pointRadius: 0, stepped: 'before', yAxisID: 'pods',
        }},
        {{
          type: 'line',
          label: 'Virtual users (VUs)',
          data: vuData,
          borderColor: '#fb923c', backgroundColor: '#fb923c18',
          fill: true, pointRadius: 0, yAxisID: 'vus', tension: 0.2,
        }},
      ],
    }},
    options: {{
      responsive: true,
      interaction: {{ mode: 'index', intersect: false }},
      plugins: {{
        legend: {{ labels: {{ color: '#e2e8f0' }} }},
        annotation: {{ annotations }},
      }},
      scales: {{
        x: {{
          type: 'linear', title: {{ display: true, text: 'Elapsed (seconds)', color: '#94a3b8' }},
          ticks: {{ color: '#94a3b8' }}, grid: {{ color: '#334155' }},
        }},
        pods: {{
          type: 'linear', position: 'left',
          title: {{ display: true, text: 'Pod count', color: '#38bdf8' }},
          ticks: {{ color: '#38bdf8', stepSize: 1 }}, grid: {{ color: '#334155' }},
          min: 0,
        }},
        vus: {{
          type: 'linear', position: 'right',
          title: {{ display: true, text: 'Virtual users', color: '#fb923c' }},
          ticks: {{ color: '#fb923c' }}, grid: {{ drawOnChartArea: false }},
          min: 0,
        }},
      }},
    }},
  }});
  </script>
</body>
</html>
"""


def generate_html(pod_data, vu_data, evt_data, output_path):
    html = HTML_TEMPLATE.format(
        pod_data_json=json.dumps(pod_data),
        vu_data_json=json.dumps(list(vu_data)),
        evt_data_json=json.dumps(evt_data),
    )
    with open(output_path, "w") as f:
        f.write(html)
    print(f"Report written → {output_path}")


# ---------------------------------------------------------------------------
# main
# ---------------------------------------------------------------------------
def main():
    if len(sys.argv) < 4:
        print("Usage: plot-scaling.py <pod-csv> <k6-ndjson> <output.html>")
        sys.exit(1)

    pod_csv_path = sys.argv[1]
    k6_json_path = sys.argv[2]
    html_path = sys.argv[3]

    print(f"Reading pods CSV   : {pod_csv_path}")
    pod_rows = parse_pods(pod_csv_path)
    print(f"  {len(pod_rows)} rows")

    print(f"Reading k6 NDJSON  : {k6_json_path}")
    vus_points = parse_k6_vus(k6_json_path)
    print(f"  {len(vus_points)} VU data points")

    pod_data, vu_data = align(pod_rows, vus_points)
    evt_data = events(pod_data)
    print(f"  {len(evt_data)} scale event(s) detected")

    generate_html(pod_data, vu_data, evt_data, html_path)


if __name__ == "__main__":
    main()
