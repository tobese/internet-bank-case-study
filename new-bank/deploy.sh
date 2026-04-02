#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Defaults
REGISTRY=""
TAG="latest"
SKIP_BUILD=false
PORT_FORWARD=false
LOCAL_PORT=8080

usage() {
  cat <<EOF
Usage: $(basename "$0") [OPTIONS]

Build and deploy the Internet Bank cluster to Kubernetes.

Options:
  -r, --registry REGISTRY   Docker registry prefix (e.g. ghcr.io/myorg)
  -t, --tag TAG              Image tag (default: latest)
  -s, --skip-build           Skip Docker image builds
  -p, --port-forward         Set up port-forwarding after deploy (for local clusters)
  -l, --local-port PORT      Local port for port-forwarding (default: 8080)
  -h, --help                 Show this help message
EOF
  exit 0
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -r|--registry)   REGISTRY="$2"; shift 2 ;;
    -t|--tag)        TAG="$2"; shift 2 ;;
    -s|--skip-build) SKIP_BUILD=true; shift ;;
    -p|--port-forward) PORT_FORWARD=true; shift ;;
    -l|--local-port) LOCAL_PORT="$2"; shift 2 ;;
    -h|--help)       usage ;;
    *) echo "Unknown option: $1"; usage ;;
  esac
done

# Build image names
if [[ -n "$REGISTRY" ]]; then
  API_IMAGE="${REGISTRY}/internet-bank/api-application:${TAG}"
  WEB_CLIENT_IMAGE="${REGISTRY}/internet-bank/web-client:${TAG}"
else
  API_IMAGE="internet-bank/api-application:${TAG}"
  WEB_CLIENT_IMAGE="internet-bank/web-client:${TAG}"
fi

# ── Pre-flight checks ───────────────────────────────────────────────
echo "==> Checking prerequisites..."
for cmd in docker kubectl; do
  if ! command -v "$cmd" &>/dev/null; then
    echo "ERROR: '$cmd' is not installed or not in PATH." >&2
    exit 1
  fi
done

if ! kubectl cluster-info &>/dev/null; then
  echo "ERROR: Cannot reach a Kubernetes cluster. Check your kubeconfig." >&2
  exit 1
fi

# ── Build Docker images ─────────────────────────────────────────────
if [[ "$SKIP_BUILD" == false ]]; then
  echo "==> Building API image: ${API_IMAGE}"
  docker build -t "$API_IMAGE" "$SCRIPT_DIR/api-application"

  echo "==> Building Web Client image: ${WEB_CLIENT_IMAGE}"
  docker build -t "$WEB_CLIENT_IMAGE" "$SCRIPT_DIR/multi-client"

  if [[ -n "$REGISTRY" ]]; then
    echo "==> Pushing images to registry..."
    docker push "$API_IMAGE"
    docker push "$WEB_CLIENT_IMAGE"
  fi
else
  echo "==> Skipping Docker builds (--skip-build)"
fi

# ── Load images into minikube if applicable ──────────────────────────
if [[ -z "$REGISTRY" ]] && command -v minikube &>/dev/null && minikube status &>/dev/null; then
  echo "==> Loading images into minikube..."
  minikube image load "$API_IMAGE"
  minikube image load "$WEB_CLIENT_IMAGE"
fi

# ── Update manifests if using a custom registry/tag ──────────────────
if [[ -n "$REGISTRY" || "$TAG" != "latest" ]]; then
  echo "==> Patching image references in manifests..."

  # Use temp copies so we don't modify the originals
  TMPDIR_K8S=$(mktemp -d)
  cp "$SCRIPT_DIR/k8s/"*.yaml "$TMPDIR_K8S/"

  sed -i.bak "s|image: .*api-application.*|image: ${API_IMAGE}|" "$TMPDIR_K8S/api-deployment.yaml"
  sed -i.bak "s|image: .*web-client.*|image: ${WEB_CLIENT_IMAGE}|" "$TMPDIR_K8S/web-client-deployment.yaml"
  rm -f "$TMPDIR_K8S/"*.bak

  DEPLOY_DIR="$TMPDIR_K8S"
else
  DEPLOY_DIR="$SCRIPT_DIR/k8s"
fi

# ── Deploy to Kubernetes ─────────────────────────────────────────────
echo "==> Applying Kubernetes manifests..."
kubectl apply -f "$DEPLOY_DIR/"

# Force pods to restart so they pick up freshly built images
# (necessary when the tag, e.g. 'latest', hasn't changed)
if [[ "$SKIP_BUILD" == false ]]; then
  echo "==> Restarting deployments to pick up new images..."
  kubectl rollout restart deployment/api
  kubectl rollout restart deployment/web-client
fi

echo "==> Waiting for API deployment to roll out..."
kubectl rollout status deployment/api --timeout=120s

echo "==> Waiting for Web Client deployment to roll out..."
kubectl rollout status deployment/web-client --timeout=120s

# Clean up temp dir if used
if [[ "${DEPLOY_DIR}" != "$SCRIPT_DIR/k8s" ]]; then
  rm -rf "$DEPLOY_DIR"
fi

# ── Post-deploy info ─────────────────────────────────────────────────
echo ""
echo "==> Deployment complete!"
kubectl get pods -l 'app in (api,web-client)'
echo ""
kubectl get services -l 'app in (api,web-client)'

if [[ "$PORT_FORWARD" == true ]]; then
  echo ""
  echo "==> Port-forwarding web service to localhost:${LOCAL_PORT}..."
  echo "    Press Ctrl+C to stop."
  kubectl port-forward service/web-client "${LOCAL_PORT}:80"
else
  echo ""
  echo "Access the application via the web-client service's EXTERNAL-IP on port 80."
  echo "For local clusters (minikube/kind), re-run with --port-forward or use:"
  echo "  kubectl port-forward service/web-client 8080:80"
fi
