# AWS Deployment — Todo

## Glossary

- **ECR** (Elastic Container Registry) — AWS-managed Docker image registry; stores and serves your container images (API, web-client) privately.
- **EKS** (Elastic Kubernetes Service) — AWS-managed Kubernetes control plane; runs your k8s workloads without you managing etcd/API server.
- **ALB** (Application Load Balancer) — AWS layer-7 load balancer; the ALB Ingress Controller maps k8s `Ingress` resources to an ALB that routes HTTP/HTTPS traffic into the cluster.

---

## Tasks

- [ ] Create ECR repos for API + web-client
- [ ] Provision EKS cluster
- [ ] Replace PostgreSQL with RDS
- [ ] Replace Redis with ElastiCache
- [ ] Update k8s image refs to ECR URLs
- [ ] Populate k8s Secrets with real credentials
- [ ] Add Redis k8s manifests (currently missing)
- [ ] Configure ALB Ingress Controller
- [ ] Build CI/CD pipeline (GitHub Actions → ECR → EKS)
- [ ] Deploy observability stack to EKS
- [ ] Update README with AWS deployment steps
