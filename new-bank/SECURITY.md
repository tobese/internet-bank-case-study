# Security Hardening

## Container image hardening

| File                             | Change                                                                        | Security rationale                                                                                          |
| -------------------------------- | ----------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| `multi-client/Dockerfile`        | `nginx:stable-alpine` → `nginxinc/nginx-unprivileged:stable-alpine`           | Runs as uid 101 (non-root) — no privilege escalation path                                                   |
| `multi-client/nginx.conf`        | `listen 80` → `listen 8080`                                                   | Port < 1024 requires root; 8080 does not                                                                    |
| `multi-client/Dockerfile`        | `EXPOSE 80` → `EXPOSE 8080`                                                   | Matches runtime                                                                                             |
| `api-application/Dockerfile`     | `eclipse-temurin:21-jre` → `gcr.io/distroless/java21-debian12:nonroot`        | No shell, no package manager, no apt, no coreutils — attacker can't exec a shell even with container access |
| `api-application/Dockerfile`     | `maven:3.9-eclipse-temurin-21-alpine` → `cgr.dev/chainguard/maven:latest-dev` | Build stage: Wolfi-based Chainguard image — 0 known CVEs vs 15 in Alpine variant                            |
| `api-application/Dockerfile`     | `maven:3.9-eclipse-temurin-21` → `maven:3.9-eclipse-temurin-21-alpine`        | Build stage: Alpine reduces attack surface by ~200MB of Debian packages                                     |
| `api-application/Dockerfile`     | `ENTRYPOINT ["/usr/bin/java", ...]`                                           | Full path required — no PATH resolution without a shell                                                     |
| `docker-compose.yml`             | `"80:80"` → `"80:8080"`                                                       | Port mapping for unprivileged nginx                                                                         |
| `k8s/web-client-deployment.yaml` | `containerPort`/probes `80` → `8080`                                          | Kubernetes probes target the actual container port                                                          |
| `k8s/web-client-service.yaml`    | `targetPort: 80` → `targetPort: 8080`                                         | Service routes to correct container port (ingress still targets service port 80, unchanged)                 |
| `api-application/pom.xml`        | Spring Boot `3.5.1` → `3.5.13`                                                | Patches 2 HIGH CVEs in `spring-boot-starter-actuator`                                                       |

## PostgreSQL hardening

| File                            | Change                                              | Security rationale                                                                                   |
| ------------------------------- | --------------------------------------------------- | ---------------------------------------------------------------------------------------------------- |
| `k8s/postgres-secret.yaml`      | `bankpass123` → 32-char random password             | Default/weak password trivially guessable; rotated to a cryptographically random credential          |
| `k8s/postgres-configmap.yaml`   | Removed `POSTGRES_USER` field                       | User was duplicated from Secret into ConfigMap (unencrypted); now sourced from Secret only           |
| `k8s/postgres-statefulset.yaml` | Added pod + container `securityContext`             | `runAsNonRoot`, `allowPrivilegeEscalation: false`, `capabilities: drop: ALL` (uid 999)               |
| `k8s/network-policy.yaml`       | Added `restrict-postgres` NetworkPolicy             | Only `api` and `postgres-exporter` pods may reach port 5432; all other intra-cluster traffic blocked |
| `docker-compose.yml`            | Rotated `bankpass123` in all three service env vars | Matches k8s Secret; consistent across local dev and production                                       |
