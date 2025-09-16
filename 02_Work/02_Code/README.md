# DevDCP Monorepo

本仓库承载：边缘(现代/.NET8)+边缘(legacy/.NETFx)+中心服务+前端+统一契约。

- contracts/: gRPC/OpenAPI/JSON Schema（与 LLD 12.x 对齐）
- platform/: center|edge|edge-legacy 服务骨架
- web/portal: Vite + Vue3 + TypeScript + Element Plus
- infra/: docker compose 模板（dev/prod）
- scripts/: 中间件初始化脚本（RabbitMQ）

迁移：保留 data-acq-prototype 目录作为临时参考，后续逐步迁移至新结构。
