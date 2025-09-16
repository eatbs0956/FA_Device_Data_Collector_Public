# LLD 映射与实现清单

- 7.x Messaging -> Processor.Worker, scripts/rabbitmq-init.* , contracts/schemas
- 8.x 查询/推送 -> Query.Api (占位), Web Portal (订阅接口待接)
- 9.x 安全 -> Auth.Api (占位，后续接入 JWKS/RBAC/ABAC)
- 11.x 数据模型 -> Postgres/Influx 由 infra/compose 提供，DDL 待补
- 12.x 契约 -> contracts/grpc|openapi|schemas 已建立；迁移策略见 README

后续：
- 生成 gRPC 代码并接入 Ingestion/Upload 流。
- Query.Api 接入 Influx/PG 并实现聚合/订阅。
- Auth.Api 接入 JWT/JWKS 与策略。
