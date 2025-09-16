# ROADMAP / 迁移计划

## 决策
- Monorepo: 承载 中心+边缘+Legacy+前端+契约
- Messaging: RabbitMQ（Kafka 废弃）
- 前端: Vue3 + TypeScript + Element Plus + Vite
- Legacy: 同仓保留 .NET Framework 4.7.2 边缘代理

## 阶段
1) 契约集中与骨架落地（当前）
2) 统一 Ingestion（gRPC/REST/消息）接入与 Processor 消费
3) Query.Api 聚合/订阅 + Web Portal 集成
4) Auth.Api 接入 JWKS/RBAC/ABAC + 审计
5) 数据层 DDL/迁移 + 指标与监控

## 服务清单
- center: Query.Api, Auth.Api, Monitor.Api, Processor.Worker, Scheduler.Worker
- edge: Collector.Agent (.NET 8)
- edge-legacy: Collector.Agent.Legacy (.NET Fx 4.7.2)
- contracts: grpc/openapi/schemas

## 参考
- LLD 映射: docs/LLD-mapping.md
- RabbitMQ 初始化: scripts/rabbitmq-init.*
- Compose 模板: infra/docker-compose*.yml
