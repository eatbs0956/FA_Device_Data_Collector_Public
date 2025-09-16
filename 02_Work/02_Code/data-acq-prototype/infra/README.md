# infra (dev) - files

This folder contains developer-focused infrastructure files to run the core suite entirely inside Docker containers.

Files included:
- `docker-compose.dev.yml` : Compose file that runs infra (Kafka, Zookeeper, Postgres, Redis, Keycloak, Prometheus, Grafana) and also builds+runs the `ingestion` and `collector` services from the local source tree.
- `../` relationship: The compose file expects to be started from the project root (same level as `collector/` and `ingestion/`). Example:
  ```bash
  docker compose -f infra/docker-compose.dev.yml up --build -d
  ```

Notes:
- When running ingestion & collector inside the compose network, ingestion can connect to Kafka using `kafka:9092`.
- For local local-debug (dotnet run on host), prefer the root `docker-compose.yml` and run ingestion/collector on host to debug with VS Code; or change the producer `BootstrapServers` accordingly.
- This compose file is intended for development and integration testing only — not for production.
