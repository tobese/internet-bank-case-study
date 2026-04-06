import http from "k6/http";
import { check } from "k6";
import { Counter, Trend } from "k6/metrics";

const BASE_URL =
  __ENV.BASE_URL ||
  "http://k8s-internet-webclien-68f0e88262-565965479.eu-central-1.elb.amazonaws.com";

// Custom metrics
const facErrors = new Counter("fac_errors");
const facDuration = new Trend("fac_duration", true);

export const options = {
  stages: [
    { duration: "30s", target: 20 }, // ramp up to 20 VUs (baseline, no scaling)
    { duration: "60s", target: 20 }, // hold at 20 VUs
    { duration: "30s", target: 150 }, // spike to 150 VUs → triggers HPA scale-up
    { duration: "120s", target: 150 }, // hold at 150 VUs (pods scale up)
    { duration: "30s", target: 0 }, // ramp down → pods will scale back after cooldown
  ],
  thresholds: {
    http_req_failed: ["rate<0.05"], // <5% errors (lenient during pod scale-up)
    http_req_duration: ["p(95)<5000"], // 95th pct under 5s (lenient during spinning up)
    fac_duration: ["p(95)<4000"],
  },
};

export default function () {
  const n = Math.floor(Math.random() * 20) + 1;

  const res = http.get(`${BASE_URL}/api/fac/${n}`, {
    tags: { endpoint: "factorial" },
  });

  const ok = check(res, {
    "status 200": (r) => r.status === 200,
    "has result": (r) => {
      try {
        return JSON.parse(r.body).result !== undefined;
      } catch {
        return false;
      }
    },
  });

  facDuration.add(res.timings.duration);
  if (!ok) facErrors.add(1);

  // no sleep — maximise request rate for HPA pressure
}
