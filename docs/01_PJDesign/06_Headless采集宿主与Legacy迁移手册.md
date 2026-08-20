# Headless 采集宿主与 Legacy 迁移手册

## 范围与状态

`Collector.Agent.Legacy` 是 `net472` WinForms 维护模式客户端，只引用 `Collector.Core/netstandard2.0`，因此不支持 OPC UA 与 Beckhoff ADS。新部署应使用 `platform/edge/Collector.Agent.Headless`；桌面 `Collector.Agent` 仅保留交互式运维用途。

Headless 宿主为 .NET 8 Worker/Windows Service，复用配置拉取、SignalR 通知、心跳、SQLite store-and-forward 和 `Collector.Core.Modern`。它支持 Modbus、S7、MC、MQTT、HTTP、EtherNet/IP、OPC UA 与 ADS。

## 迁移步骤

1. 停止 Legacy 客户端，保留其本地设置文件和日志作为回滚依据。
2. 将 `ApiGatewayUrl`、`NodeId`、`NodeName`、RabbitMQ 主机/端口/用户/交换机迁入 Headless `appsettings.json` 或 `DEVDCP_` 环境变量。
3. 不迁移 Legacy 的明文密码。使用 `DEVDCP_RABBITMQ_PASSWORD`、`enc:v1:` 或 `secret://`；使用 `DEVDCP_MASTER_KEY` 解密受保护值。
4. 为 Headless 设置 `DEVDCP_HEADLESS_ACCESS_TOKEN`，或设置 `DEVDCP_HEADLESS_USERNAME` 与 `DEVDCP_HEADLESS_PASSWORD`。凭据仅保留在服务运行环境，不写入 `appsettings.json`/`usersettings.json`。
5. 先以前台模式启动，确认节点注册、心跳、配置版本和任务状态均正常，再注册为 Windows Service。
6. 观察一个采集周期，验证 RabbitMQ 消息、中心历史数据和 `buffer/outbox.db` 的断网回放；确认后再卸载 Legacy。

## 配置映射

| Legacy `LocalSettings` | Headless | 说明 |
|---|---|---|
| `NodeId` / `NodeName` | `DEVDCP_NODE_ID` / `DEVDCP_NODE_NAME` | 留空时回退机器名 |
| `ApiGatewayUrl` | `DEVDCP_API_GATEWAY_URL` | 通过 Gateway 访问 Auth/Admin/SignalR |
| `RabbitMqHost` / `Port` / `User` | `DEVDCP_RABBITMQ_HOST` / `PORT` / `USER` | 可放在 appsettings 默认值 |
| `RabbitMqPassword` | `DEVDCP_RABBITMQ_PASSWORD` | 生产环境不落明文文件 |
| `RabbitMqExchange` | `DEVDCP_RABBITMQ_EXCHANGE` | 默认 `data.collected` |
| 无 | `LocalBufferPath` | SQLite 本地存转发，默认 `buffer/outbox.db` |

## 回滚

停止并禁用 Headless 服务，恢复原 Legacy 程序与其原节点配置。不要在同一 `NodeId` 下同时运行两个采集宿主，避免重复采集与重复上报。