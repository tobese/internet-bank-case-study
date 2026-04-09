# Docker Compose Miniguide

## Common Commands

- **Start services in background:**
  docker compose up -d

- **Start services with logs in foreground:**
  docker compose up

- **Stop services:**
  docker compose down

- **View logs:**
  docker compose logs

- **View logs for a specific service:**
  docker compose logs <service>

- **List running containers:**
  docker compose ps

- **Rebuild images and restart:**
  docker compose up --build

- **Run a one-off command in a service:**
  docker compose run <service> <command>

- **Execute a shell in a running container:**
  docker compose exec <service> sh

- **Show config as Docker sees it:**
  docker compose config

- **Remove stopped containers, networks, images:**
  docker system prune

---

For more, see: https://docs.docker.com/compose/reference/
