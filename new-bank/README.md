# Internet Bank – Containerised & Kubernetes-ready

This directory contains the containerised version of the Internet Bank application.
The `console-client` has been intentionally omitted.

## Services

| Service | Source | Port | Description |
|---|---|---|---|
| `api` | `api-application/` | 8282 | Spring Boot REST API (factorial & Fibonacci) |
| `web` | `web-application/` | 3000 | Node.js web server + API proxy |

## Architecture

The web application proxies all `/api/` requests server-side to the API service.
This means the browser never needs direct access to the API — it only talks to the web service.

```
Browser → web-service:80 → web pod (Node.js)
                               ↓ /api/* proxy
                           api-service:8282 → api pod (Spring Boot)
```

## Building Docker Images

```bash
# API (Spring Boot) – multi-stage build, requires no local JDK/Maven
docker build -t internet-bank/api-application:latest ./api-application

# Web (Node.js)
docker build -t internet-bank/web-application:latest ./web-application
```

If you are targeting a remote registry, tag accordingly:

```bash
docker build -t <registry>/internet-bank/api-application:latest ./api-application
docker build -t <registry>/internet-bank/web-application:latest ./web-application
docker push <registry>/internet-bank/api-application:latest
docker push <registry>/internet-bank/web-application:latest
```

Then update the `image:` fields in `k8s/api-deployment.yaml` and `k8s/web-deployment.yaml`.

## Deploying to Kubernetes

```bash
kubectl apply -f k8s/
```

Check rollout status:

```bash
kubectl rollout status deployment/api
kubectl rollout status deployment/web
```

Get the external address of the web service:

```bash
kubectl get service web
```

> **Local clusters (minikube / kind):** `LoadBalancer` services do not get an external IP automatically.
> Use `minikube service web` or `kubectl port-forward service/web 8080:80` instead.

## Environment Variables

### web-application

| Variable | Default | Description |
|---|---|---|
| `PORT` | `3000` | Port the Node.js server listens on |
| `API_URL` | `http://localhost:8282` | Base URL of the API service (server-side proxy target) |

The Kubernetes `web-deployment.yaml` sets `API_URL=http://api:8282` so the web pod reaches the API via the cluster-internal `api` Service.
