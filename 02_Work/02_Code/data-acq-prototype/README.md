# Core suite infra package

docker compose up -d → 启动轻量 infra（根 compose），适合在主机上用 dotnet run 调试服务。

docker compose -f infra/docker-compose.dev.yml up --build -d → 启动全部容器（包含 ingestion/collector），适合做集成测试。
See infra/docker-compose.dev.yml

