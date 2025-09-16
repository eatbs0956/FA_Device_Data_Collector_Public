# gRPC Contracts

本目录存放统一的 gRPC 契约（与 LLD 第12章一致）。

- DataPoint 增加 tenant_id/tag_id/event_time/quality/envelope_id/correlation_id/seq/source/headers_json
- 保留兼容字段 device_id/point_name/timestamp 并标注 deprecated，用于迁移期。
- 生成代码时，各服务从此处引用，避免分散定义。
