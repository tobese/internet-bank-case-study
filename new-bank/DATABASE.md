# PostgreSQL Database Setup

The Internet Bank case study now includes PostgreSQL for persistent data storage.

## Quick Start

### Local Development (Docker Compose)

```bash
docker-compose up -d
```

This starts:

- **PostgreSQL** on `localhost:5432`
- **Java API** on `localhost:8282`
- **Web Client** on `localhost:80`

### Kubernetes Deployment

The PostgreSQL database is automatically deployed as part of the Kubernetes cluster:

```bash
export DOCKER_HOST=unix:///var/run/docker.sock
kubectl apply -k k8s/
```

## Connection Details

| Property     | Value                                            |
| ------------ | ------------------------------------------------ |
| **Host**     | `postgres` (k8s) or `localhost` (docker-compose) |
| **Port**     | `5432`                                           |
| **Database** | `internet_bank`                                  |
| **Username** | `bankuser`                                       |
| **Password** | `bankpass123`                                    |

## Database Schema

### Tables

#### `transactions`

Stores transaction records

- `id` (SERIAL PRIMARY KEY)
- `transaction_type` (VARCHAR)
- `amount` (DECIMAL)
- `result` (VARCHAR)
- `created_at` (TIMESTAMP)

#### `calculations`

Stores math operation results

- `id` (SERIAL PRIMARY KEY)
- `operation` (VARCHAR) - e.g., 'FACTORIAL', 'FIBONACCI'
- `operand` (INTEGER)
- `result` (VARCHAR)
- `created_at` (TIMESTAMP)

#### `audit_log`

Tracks system events

- `id` (SERIAL PRIMARY KEY)
- `event_type` (VARCHAR)
- `details` (JSONB)
- `created_at` (TIMESTAMP)

## Useful Commands

### Connect to PostgreSQL (Kubernetes)

```bash
# Set up port forward (runs in background)
pgforward

# Connect with psql
pgconnect

# Kill the port forward
pgkill
```

### Connect to PostgreSQL (Local)

```bash
psql -h localhost -U bankuser -d internet_bank
```

### View Logs

**Docker Compose:**

```bash
docker compose logs postgres
```

**Kubernetes:**

```bash
export DOCKER_HOST=unix:///var/run/docker.sock
kubectl logs -n internet-bank postgres-0
```

### Backup Database

**Docker Compose:**

```bash
docker compose exec postgres pg_dump -U bankuser internet_bank > backup.sql
```

**Kubernetes:**

```bash
kubectl exec -it postgres-0 -n internet-bank -- pg_dump -U bankuser internet_bank > backup.sql
```

### Restore Database

```bash
psql -U bankuser -d internet_bank < backup.sql
```

## Architecture

```
┌─────────────────────────────────────────────┐
│         Kubernetes Cluster                  │
├─────────────────────────────────────────────┤
│                                             │
│  ┌────────────┐  ┌────────────┐            │
│  │ API Pods   │──│ PostgreSQL  │            │
│  │ (x3)       │  │ StatefulSet │            │
│  └────────────┘  │ (1 replica) │            │
│                  │ + PVC (1Gi) │            │
│                  │ + Service   │            │
│                  └────────────┘            │
│                                             │
│  ┌────────────────┐                        │
│  │ Web Client     │────────→ nginx proxy   │
│  │ Deployment     │         to API         │
│  └────────────────┘                        │
└─────────────────────────────────────────────┘
```

## Storage

PostgreSQL uses a `PersistentVolumeClaim` (PVC) with 1Gi storage.

**Configuration:**

- `StorageClass`: Default
- `AccessMode`: `ReadWriteOnce`
- `Capacity`: `1Gi`

## Health Checks

PostgreSQL includes a liveness probe:

- **Command:** `pg_isready -U bankuser -d internet_bank`
- **Initial Delay:** 30 seconds
- **Period:** 10 seconds

If unhealthy, Kubernetes will restart the pod automatically.

## Environment Variables

| Variable            | Used By       | Value                                                                              |
| ------------------- | ------------- | ---------------------------------------------------------------------------------- |
| `POSTGRES_DB`       | PostgreSQL    | `internet_bank`                                                                    |
| `POSTGRES_USER`     | PostgreSQL    | `bankuser`                                                                         |
| `POSTGRES_PASSWORD` | PostgreSQL    | `bankpass123`                                                                      |
| `DATABASE_URL`      | API (secrets) | `jdbc:postgresql://postgres:5432/internet_bank?user=bankuser&password=bankpass123` |

## Next Steps

To connect the Java API to the database:

1. **Add Spring Data JPA dependency** to `pom.xml`:

   ```xml
   <dependency>
       <groupId>org.springframework.boot</groupId>
       <artifactId>spring-boot-starter-data-jpa</artifactId>
   </dependency>
   <dependency>
       <groupId>org.postgresql</groupId>
       <artifactId>postgresql</artifactId>
   </dependency>
   ```

2. **Update `application.properties`:**

   ```properties
   spring.datasource.url=${DATABASE_URL}
   spring.jpa.hibernate.ddl-auto=validate
   ```

3. **Create JPA entities** for `Transaction` and `Calculation`

4. **Rebuild API image:**
   ```bash
   docker build -t internet-bank/api-application:latest api-application/
   ```

## Troubleshooting

**Pod stuck in CrashLoopBackOff:**

```bash
kubectl logs -n internet-bank postgres-0
```

**PVC not binding:**

```bash
kubectl describe pvc postgres-storage-postgres-0 -n internet-bank
```

**Cannot connect from API:**

- Verify service DNS: `postgres.internet-bank.svc.cluster.local`
- Check network policy rules
- Verify DATABASE_URL in secrets

**Performance issues:**

- Monitor resource usage: `kubectl top pod -n internet-bank`
- Check slow query logs in PostgreSQL
- Verify indexes exist on frequently queried columns
