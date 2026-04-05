# Internet Bank – Containerised & Kubernetes-ready

This directory contains the containerised version of the Internet Bank application.
The `console-client` has been intentionally omitted.

## Services

- **`api`** — `api-application/` — Spring Boot REST API (factorial & Fibonacci) on port 8282
- **`web-client`** — `multi-client/` — Uno Platform WASM app served by nginx on port 80

## Architecture

The web-client container serves the Uno/WASM application via nginx.
nginx reverse-proxies all `/api/` requests to the API service, so the browser never needs direct access to the backend.

```
Browser → web-client-service:80 → nginx (Uno WASM static files)
                                      ↓ /api/* proxy
                                  api-service:8282 → api pod (Spring Boot)
```

## Quick Deploy

The `deploy.sh` script builds images, loads them into minikube (if applicable), and applies manifests:

```bash
./deploy.sh                   # build + deploy (local/minikube)
./deploy.sh -p                # … then port-forward to localhost:8080
./deploy.sh -r ghcr.io/myorg  # push to a remote registry
./deploy.sh -s                # skip builds, just re-deploy manifests
```

Run `./deploy.sh -h` for all options.

## Manual Build & Deploy

```bash
# API (Spring Boot) – multi-stage build, requires no local JDK/Maven
docker build -t internet-bank/api-application:latest ./api-application

# Web Client (Uno WASM + nginx)
docker build -t internet-bank/web-client:latest ./multi-client
```

Deploy to Kubernetes:

```bash
kubectl apply -f k8s/
kubectl rollout status deployment/api
kubectl rollout status deployment/web-client
```

Access the app:

```bash
kubectl get service web-client
```

> **Local clusters (minikube / kind):** `LoadBalancer` services do not get an external IP automatically.
> Use `kubectl port-forward service/web-client 8080:80` instead.
