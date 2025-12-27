# DevDCP Monorepo

工业数据采集与处理平台（Industrial Data Acquisition & Processing Platform）。

## 概述
面向多源工业设备数据采集、清洗、聚合、查询与分发的统一平台：
- 边缘采集：统一插件式适配器、可靠上传、幂等保证。
- 中心处理：消息解耦、窗口/聚合、清洗、指标存储（Influx / PostgreSQL）。
- 查询与推送：统一 REST/gRPC/消息契约，支持实时订阅、重放、节流。
- 安全与治理：JWT + JWKS、RBAC+ABAC、审计、连接与资源治理。
- 统一契约：单一来源 (gRPC + OpenAPI + JSON Schema) 与迁移策略。

## 技术栈
- 语言/运行时：.NET 8、.NET Framework 4.7.2（Legacy Edge）、TypeScript
- 前端：Vue3 + Vite + Element Plus (基于 soybean-admin ElementPlus 版本)
- 消息：RabbitMQ（发布/确认/死信/Prefetch）
- 存储：PostgreSQL (元数据/审计/幂等)、InfluxDB (时序)、Redis (缓存)
- 协议：gRPC、REST、WebSocket、消息(JSON)

## 目录结构
```
02_Work/02_Code/
├─ contracts/                # 统一契约中心（gRPC / OpenAPI / JSON Schema）
│  ├─ grpc/                  # gRPC .proto 文件（与 12.x 对齐）
│  ├─ openapi/               # OpenAPI 规范
│  └─ schemas/               # 消息与事件 JSON Schema
├─ platform/
│  ├─ center/                # 中心服务与处理组件
│  │  ├─ Admin.Api/          # 前端业务聚合服务（设备/分组/节点管理）
│  │  ├─ Auth.Api/           # 认证鉴权（JWKS/RBAC/用户/角色/菜单）
│  │  ├─ Gateway.Api/        # API网关（YARP反向代理/路由/限流）
│  │  ├─ Query.Api/          # 查询与实时订阅 API（占位）
│  │  ├─ Monitor.Api/        # 监控 / 健康（占位）
│  │  ├─ Processor.Worker/   # 消费消息进行清洗/聚合（占位）
│  │  ├─ Scheduler.Worker/   # 定时/窗口任务（占位）
│  │  └─ SharedAuth.Library/ # 共享库（数据库上下文/实体/扩展）
│  ├─ edge/                  # 现代边缘采集代理 (.NET 8)
│  │  └─ Collector.Agent/    # 采集 + 上传（占位）
│  └─ edge-legacy/           # Legacy 边缘 (.NET Framework)
│     └─ Collector.Agent.Legacy/
├─ web/
│  └─ frontend/              # 前端门户 (soybean-admin ElementPlus 定制)
├─ infra/                    # docker compose 模板（dev/prod）
├─ scripts/                  # 运维与初始化脚本（RabbitMQ 等）
├─ docs/                     # 实现映射、路线图等补充说明
├─ Directory.Build.props     # 全局 .NET 编译配置
└─ data-acq-prototype/       # 旧原型（待迁移/参考）
```

### 目录说明
- `contracts/`：所有上下游服务均引用此处生成客户端/服务端代码，避免契约漂移。
- `platform/center`：核心业务逻辑组件；通过消息与存储实现清洗、聚合、查询。
- `platform/edge`：新一代可扩展采集代理；后续接入插件化适配器体系。
- `platform/edge-legacy`：旧式设备或仅支持 .NET Framework 的场景，逐步抽离至现代代理。
- `web/frontend`：基于 soybean-admin (Element Plus) 的运营与运维控制台，实现设备/数据/规则/监控可视化。
- `infra/`：运行所需的基础设施服务编排；后续可扩展 Observability（Prometheus/Grafana）。
- `scripts/`：初始化、迁移、运维脚本；Kafka 脚本已废弃（改用 RabbitMQ）。
- `docs/`：与 LLD 的章节映射、Roadmap、后续设计差异说明。
- `data-acq-prototype/`：临时参考，分阶段拆分入新结构后删除。

## 快速开始
### 基础设施（开发）
```
cd 02_Work/02_Code/infra
# 启动核心依赖
# (需已安装 Docker Desktop)
docker compose -f docker-compose.dev.yml up -d
```
### 前端
```
cd 02_Work/02_Code/web/frontend
# 推荐使用 pnpm (若无则: npm i -g pnpm)
pnpm install
pnpm dev
```
### 构建中心与边缘服务
```
cd 02_Work/02_Code/platform/center/Query.Api
dotnet run
```
或在根目录：
```
dotnet build 02_Work/02_Code/platform/center/Processor.Worker/Processor.Worker.csproj
```
### RabbitMQ 初始化
```
# Linux / WSL
bash scripts/rabbitmq-init.sh
# Windows PowerShell
powershell -ExecutionPolicy Bypass -File scripts/rabbitmq-init.ps1
```

## 迁移策略（契约与数据）
- 旧字段：`deviceId/pointName/timestamp` 标记 deprecated；新字段 `tenantId/tagId/eventTime` 已生效。
- 幂等：通过 `envelopeId + seq (+ tenantId/device/tag)` 组合保证幂等插入。
- 分阶段：
  1) 双写：新旧字段并存（当前）
  2) 监测：统计旧字段使用率 < 阈值
  3) 禁用旧字段写入（配置开关）
  4) 清理旧字段与兼容逻辑

## 后续里程碑（Roadmap 摘要）
1. Ingestion 流程接入（gRPC/REST/消息统一 → Processor）
2. Processor 写入 Influx/PostgreSQL + 聚合窗口实现
3. Query.Api 聚合/订阅/重放，对接前端实时面板
4. Auth.Api: JWKS、RBAC+ABAC、审计流水
5. 监控：指标、Tracing、告警策略

完整见：`docs/ROADMAP.md` 与 `docs/LLD-mapping.md`。

## 约定
- 所有跨服务数据模型以 `contracts/` 为单一来源 (SSOT)。
- 严禁在单独项目中复制/粘贴 proto 或 schema。引用生成方式后续在 CI 中强制检查。
- 新特性优先更新 LLD → 契约 → 实现代码。

## 许可
内部项目（License 待定）。
