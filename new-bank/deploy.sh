n/

# Internet Bank Deployment Script
# Deploys Java API and .NET web client to Kubernetes with automated setup

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}🚀 Internet Bank Deployment Script${NC}\n"

# Function to check if Docker is running
check_docker() {
    if ! docker ps > /dev/null 2>&1; then
        echo -e "${RED}❌ Docker is not running${NC}"
        echo -e "${YELLOW}Opening Docker Desktop...${NC}"
        open /Applications/Docker.app
        echo -e "${YELLOW}⏳ Waiting for Docker to start (60 seconds)...${NC}"
        for i in {1..60}; do
            if docker ps > /dev/null 2>&1; then
                echo -e "${GREEN}✅ Docker is running${NC}\n"
                return 0
            fi
            sleep 1
        done
        echo -e "${RED}❌ Docker failed to start${NC}"
        exit 1
    fi
    echo -e "${GREEN}✅ Docker is running${NC}\n"
}

# Function to check if minikube is running
check_minikube() {
    if ! kubectl cluster-info > /dev/null 2>&1; then
        echo -e "${YELLOW}🔄 Starting minikube...${NC}"
        minikube start
    fi
    echo -e "${GREEN}✅ Kubernetes cluster ready${NC}\n"
}

# Function to build and deploy service
deploy_service() {
    local service_name=$1
    local service_path=$2
    local image_name=$3
    local namespace=${4:-internet-bank}
    
    echo -e "${BLUE}📦 Building ${service_name}...${NC}"
    cd "${service_path}"
    
    # For Java services, run Maven build first
    if [ -f "pom.xml" ]; then
        echo -e "${YELLOW}🔨 Compiling with Maven...${NC}"
        mvn clean package -DskipTests -q
    fi
    
    cd /Users/tb/Dev/internet-bank-case-study/new-bank
    docker build -t "${image_name}:latest" "${service_path}"
    
    echo -e "${YELLOW}📤 Loading image into minikube...${NC}"
    minikube image load "${image_name}:latest"
    
    echo -e "${GREEN}✅ ${service_name} deployed${NC}\n"
}

# Function to setup port forwarding
setup_port_forward() {
    local service=$1
    local port=$2
    local namespace=${3:-internet-bank}
    
    echo -e "${YELLOW}🔗 Setting up port forwarding for ${service}...${NC}"
    kubectl port-forward -n ${namespace} svc/${service} ${port}:${port} &
    sleep 2
    echo -e "${GREEN}✅ Port forwarding ready at localhost:${port}${NC}\n"
}

# Main deployment flow
check_docker
check_minikube

# Build and deploy Java API
deploy_service "Java API" "api-application" "internet-bank/api-application"

# Update Kubernetes deployment
echo -e "${BLUE}🔄 Updating Kubernetes deployment...${NC}"
kubectl set image deployment/api api=internet-bank/api-application:latest -n internet-bank --record 2>/dev/null || true
kubectl rollout status deployment/api -n internet-bank

# Verify pods are running
echo -e "${BLUE}📊 Checking pod status...${NC}"
kubectl get pods -n internet-bank
echo ""

# Setup port forwarding
echo -e "${BLUE}🔌 Setting up port forwarding...${NC}"
kubectl port-forward -n internet-bank svc/api 8080:8282 > /dev/null 2>&1 &
sleep 2

# Test API endpoints
echo -e "${BLUE}🧪 Testing API endpoints...${NC}"
echo ""

echo -e "${YELLOW}📌 Testing /api/mathematician/random${NC}"
curl -s http://localhost:8080/api/mathematician/random | jq . || echo "Request failed"
echo ""

echo -e "${YELLOW}📌 Testing /api/mathematicians${NC}"
curl -s http://localhost:8080/api/mathematicians | jq . || echo "Request failed"
echo ""

echo -e "${YELLOW}📌 Testing /api/math/factorial/5${NC}"
curl -s http://localhost:8080/api/fac/5 | jq . || echo "Request failed"
echo ""

echo -e "${YELLOW}📌 Testing /api/fib/10${NC}"
curl -s http://localhost:8080/api/fib/10 | jq . || echo "Request failed"
echo ""

echo -e "${GREEN}✅ Deployment complete!${NC}"
echo -e "${BLUE}📍 Port forwarding running on localhost:8080${NC}"
echo -e "${BLUE}🔗 API Documentation:${NC}"

# ANSI OSC 8 hyperlink: \e]8;;URL\e\\TEXT\e]8;;\e\\
link() { printf '\e]8;;%s\e\\%s\e]8;;\e\\\n' "$1" "   $2"; }

link "http://localhost:8080/api/mathematician/random"       "Random mathematician:  GET http://localhost:8080/api/mathematician/random"
link "http://localhost:8080/api/mathematicians"             "All mathematicians:    GET http://localhost:8080/api/mathematicians"
link "http://localhost:8080/api/mathematician/1"            "By ID:                 GET http://localhost:8080/api/mathematician/{id}"
link "http://localhost:8080/api/fac/5"                      "Factorial:             GET http://localhost:8080/api/fac/{n}"
link "http://localhost:8080/api/fib/10"                     "Fibonacci:             GET http://localhost:8080/api/fib/{n}"
link "http://localhost:8080/api/primes/sieve/30"            "Primes (SSE stream):   GET http://localhost:8080/api/primes/sieve/{limit}"
echo ""
echo -e "${YELLOW}💡 Tip: Kill port forwarding with 'pkill -f \"port-forward\"'${NC}"
