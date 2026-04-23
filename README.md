# FA Device Data Collector (DCP) — 工业数据采集与处理平台

<p align="center">
  <b>Industrial Data Collection & Processing Platform</b><br/>
  面向制造业的多协议设备数据采集、处理、存储与可视化统一平台
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet" alt=".NET 8"/>
  <img src="https://img.shields.io/badge/Vue-3.x-4FC08D?logo=vuedotjs" alt="Vue 3"/>
  <img src="https://img.shields.io/badge/Avalonia-11-8B5CF6" alt="Avalonia"/>
  <img src="https://img.shields.io/badge/PostgreSQL-14-336791?logo=postgresql" alt="PostgreSQL"/>
  <img src="https://img.shields.io/badge/InfluxDB-2.x-22ADF6?logo=influxdb" alt="InfluxDB"/>
  <img src="https://img.shields.io/badge/RabbitMQ-3.x-FF6600?logo=rabbitmq" alt="RabbitMQ"/>
  <img src="https://img.shields.io/badge/License-Apache--2.0-blue.svg" alt="License"/>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/⚠_开发阶段-Alpha-orange?style=for-the-badge" alt="Alpha"/>
  <a href="https://github.com/eatbs0956/FA_Device_Data_Collector_Public/releases/latest">
    <img src="https://img.shields.io/github/v/release/eatbs0956/FA_Device_Data_Collector_Public?label=latest%20release&style=for-the-badge" alt="Release"/>
  </a>
</p>

> **⚠️ 项目状态：开发中（Alpha）**
>
> 已完成主体架构与核心模块：认证鉴权、API 网关、管理后台、数据处理 Worker、前端管理平台、边缘采集客户端及 Modbus TCP/RTU、Siemens S7 驱动。
> 更多协议驱动（OPC UA、MQTT、三菱 MC 等）和高级功能（数据聚合、报表导出等）持续开发中，API 与数据结构可能变更。

> **[English Version (README_en.md)](./README_en.md)**

> **本仓库为公开资产仓库**
> 仅包含文档、部署编排、LICENSE、Issue 模板等面向终端用户的内容。
> 完整源代码位于私有仓库，不对外开放。
> 二进制通过 [GitHub Releases](https://github.com/eatbs0956/FA_Device_Data_Collector_Public/releases) 与 [GHCR 镜像](https://github.com/eatbs0956?tab=packages) 分发。

---

## 📋 目录

- [项目简介](#-项目简介)
- [系统架构](#-系统架构)
- [技术栈](#-技术栈)
- [快速部署](#-快速部署)
- [服务端口清单](#-服务端口清单)
- [边端采集器](#-边端采集器windows)
- [版本与升级](#-版本与升级)
- [文档索引](#-文档索引)
- [致谢](#-致谢)
- [许可证](#-许可证)

---

## 📖 项目简介

FA Device Data Collector（下称 **DCP**）是一个面向**制造业单工厂内部**的工业数据采集通用后台系统，解决以下核心问题：

- **多协议统一接入** — OPC UA、Modbus TCP/RTU、MQTT、三菱 MC、西门子 S7 等
- **分布式边缘采集** — 车间 / 产线级部署边缘节点，秒级数据采集
- **实时数据处理** — 消息驱动，支持数据清洗、聚合、时序存储
- **可视化管理** — Web 管理平台：设备配置、实时监控、历史查询、告警管理
- **标准 API 集成** — 为 MES 等上层系统提供 RESTful API 与 WebSocket 实时推送

### 系统组成

| 层级 | 组件 | 镜像 / 产物 |
|------|------|-------------|
| **前端** | Web 管理控制台（Vue 3 + Element Plus） | `ghcr.io/eatbs0956/fa-dc-web` |
| **网关** | Gateway.Api（YARP 反向代理 + SignalR Hub） | `ghcr.io/eatbs0956/fa-dc-gateway` |
| **中心服务** | Auth.Api（JWT / RBAC） | `ghcr.io/eatbs0956/fa-dc-auth` |
|  | Admin.Api（设备 / 节点 / 查询聚合） | `ghcr.io/eatbs0956/fa-dc-admin` |
|  | Processor.Worker（消费 RabbitMQ、写 InfluxDB） | `ghcr.io/eatbs0956/fa-dc-processor` |
|  | Monitor.Api（监控与健康检查） | `ghcr.io/eatbs0956/fa-dc-monitor` |
| **边缘采集** | Collector.Agent（.NET 8 + Avalonia 桌面端） | `Collector.Agent-vX.Y.Z-win-x64.zip` |
| **基础设施** | PostgreSQL + RabbitMQ + InfluxDB + Redis | 官方镜像 |

---

## 🏗 系统架构

```
┌──────────────────────────────────────────────────────────────┐
│                 Web Frontend (Vue 3, nginx)                   │
│                        ports: 80                              │
└──────────────────────┬───────────────────────────────────────┘
                       │ (/api, /hubs)
┌──────────────────────▼───────────────────────────────────────┐
│             Gateway.Api (YARP + SignalR Hub)                  │
│                      ports: 18020                             │
├──────────────┬──────────────┬────────────────────────────────┤
│  Auth.Api    │  Admin.Api   │  Processor.Worker              │
│  JWT/RBAC    │  设备/节点    │  消息消费 → 写 InfluxDB         │
└──────┬───────┴──────┬───────┴──────────┬─────────────────────┘
       │              │                  │
┌──────▼──────────────▼──────────────────▼─────────────────────┐
│   PostgreSQL    RabbitMQ      InfluxDB      Redis             │
│     :5432       :5672/:15672  :8086         :6379             │
└──────────────────────▲───────────────────────────────────────┘
                       │  AMQP / HTTPS
┌──────────────────────┴───────────────────────────────────────┐
│         Collector.Agent (Windows x64 桌面客户端)               │
│         边缘采集节点 — 部署在车间/产线现场                      │
└──────────────────────────────────────────────────────────────┘
```

> 默认仅对外暴露 `80`（Web）和 `18020`（Gateway），其余服务只在内部网络通信。

---

## 🛠 技术栈

| 分类 | 技术 |
|------|------|
| **后端运行时** | .NET 8（ASP.NET Core） |
| **桌面客户端** | .NET 8 + Avalonia UI 11 + CommunityToolkit.Mvvm |
| **前端框架** | Vue 3 + Vite + TypeScript + Element Plus + UnoCSS |
| **API 网关** | YARP（Yet Another Reverse Proxy） |
| **认证授权** | JWT + JWKS + RBAC |
| **消息队列** | RabbitMQ（发布确认 / 死信队列 / Prefetch） |
| **关系数据库** | PostgreSQL 14（元数据 / 审计 / 幂等） |
| **时序数据库** | InfluxDB 2.x（高频采集数据） |
| **缓存** | Redis 7（会话 / 实时推送 Pub/Sub） |
| **实时通信** | SignalR（WebSocket 全双工） |
| **日志** | Serilog |
| **容器化** | Docker + Docker Compose |

---

## 🚀 快速部署

### 环境要求

| 软件 | 最低版本 | 用途 |
|------|----------|------|
| **Linux 服务器** | Ubuntu 22.04+ / RHEL 8+ | 运行中心服务（Windows Server 2022 + WSL2 亦可） |
| **Docker Engine** | 24.x | 运行容器 |
| **Docker Compose** | v2 | 多容器编排 |

对外端口：`80`（Web）、`18020`（Gateway API）。

### 第一步：下载部署文件

```bash
mkdir dcp && cd dcp

# 下载 compose 编排文件与环境变量模板
curl -LO https://github.com/eatbs0956/FA_Device_Data_Collector_Public/releases/latest/download/docker-compose.prod.yml
curl -LO https://github.com/eatbs0956/FA_Device_Data_Collector_Public/releases/latest/download/.env.example
cp .env.example .env
```

### 第二步：编辑 `.env`

**必须修改**的几个变量：

| 变量 | 说明 |
|------|------|
| `IMAGE_TAG` | 锁定镜像版本，如 `v0.1.0`（不建议 `latest`） |
| `PUBLIC_API_BASE` | **浏览器可达**的网关地址，如 `https://dcp.example.com` 或 `http://10.0.0.5:18020` |
| `PG_PASSWORD` / `RABBITMQ_PASSWORD` / `INFLUX_PASSWORD` / `INFLUX_TOKEN` / `REDIS_PASSWORD` | 强随机密码 |
| `JWT_SIGNING_KEY` | JWT 签名密钥，建议 `openssl rand -base64 48` |

### 第三步：拉取镜像并启动

```bash
# 若镜像为 private，先登录 GHCR（需要带 read:packages 权限的 PAT）
docker login ghcr.io

docker compose -f docker-compose.prod.yml --env-file .env pull
docker compose -f docker-compose.prod.yml --env-file .env up -d

# 查看服务状态（等待全部 healthy）
docker compose -f docker-compose.prod.yml ps
```

首次启动过程：
1. `postgres` / `influxdb` / `rabbitmq` 初始化（30 – 60 秒）
2. `auth-api` / `admin-api` / `processor-worker` 启动时**自动执行数据库迁移**（约 5 秒，无需手动干预）
3. `gateway-api` 绑定端口 18020，`web` 绑定端口 80

### 第四步：访问系统

浏览器打开：**`http://<server-ip>/`** 或你配置的域名。

默认管理员账号请参阅 Release Notes（首次部署会强制要求修改密码）。

---

### 进阶：反向代理与 HTTPS（生产推荐）

使用 Caddy / Nginx / Traefik 统一代理 `80/443` → `web:80` 与 `gateway-api:18020`，示例 Caddyfile：

```Caddyfile
dcp.example.com {
    reverse_proxy /api/*  gateway:18020
    reverse_proxy /hubs/* gateway:18020
    reverse_proxy /*      web:80
}
```

完整部署说明见 [docs/DEPLOY.md](./docs/DEPLOY.md)。

---

## 🌐 服务端口清单

| 服务 | 默认端口 | 对外暴露 | 说明 |
|------|:--------:|:--------:|------|
| **web** | `80` | ✓ | Vue 3 管理控制台（nginx 托管） |
| **gateway-api** | `18020` | ✓ | API 网关入口，所有前端请求由此转发；SignalR 实时推送 |
| **auth-api** | 8080 | ✗ | JWT 认证、用户 / 角色 / 菜单 |
| **admin-api** | 8080 | ✗ | 设备、节点、查询聚合 |
| **processor-worker** | 8080 | ✗ | 消费 RabbitMQ，写 InfluxDB |
| **monitor-api** | 8080 | ✗ | 监控与健康检查 |
| **postgres** | 5432 | 可选 | 建议仅内网访问 |
| **influxdb** | 8086 | 可选 | 同上 |
| **rabbitmq** | 5672 / 15672 | 可选 | 5672=AMQP；15672=管理面板 |
| **redis** | 6379 | ✗ | 仅内部 |

---

## 💻 边端采集器（Windows）

1. 从 [Releases](https://github.com/eatbs0956/FA_Device_Data_Collector_Public/releases) 下载对应版本的 `Collector.Agent-vX.Y.Z-win-x64.zip`
2. 解压到任意目录（如 `C:\dcp\agent`）
3. 编辑 `appsettings.json`：
   - 网关地址 `Gateway:BaseUrl` = `http://<server-ip>:18020`
   - 注册码 / 采集点配置等
4. 运行方式：
   - **开发调试**：双击 `Collector.Agent.exe`
   - **生产部署**：注册为 Windows 服务（可用 `sc create` 或 NSSM）

---

## 🔄 版本与升级

版本遵循 [语义化版本](https://semver.org/lang/zh-CN/)：`vMAJOR.MINOR.PATCH`。

| 标签 | 含义 |
|------|------|
| `latest` | 最新正式版（不推荐生产直接引用） |
| `vX.Y` | 滚动到该小版本的最新补丁 |
| `vX.Y.Z` | 精确版本，**推荐生产锁定** |
| `vX.Y.Z-rc.N` | 预发布版本（RC） |

升级步骤：

```bash
# 1. 编辑 .env，更新 IMAGE_TAG
# 2. 拉新镜像并滚动重启
docker compose -f docker-compose.prod.yml --env-file .env pull
docker compose -f docker-compose.prod.yml --env-file .env up -d

# （建议）升级前备份 PostgreSQL
docker exec devdcp-postgres pg_dump -U devdcp devdcp > backup-$(date +%F).sql
```

数据库迁移由服务启动时自动执行，无需手动操作。

---

## 📚 文档索引

| 文档 | 说明 |
|------|------|
| [docs/DEPLOY.md](./docs/DEPLOY.md) | 详细部署指南（端口规划、反向代理、备份、常见问题） |
| [docs/THIRD_PARTY_NOTICES.md](./docs/THIRD_PARTY_NOTICES.md) | 第三方组件许可证声明 |
| [docs/01_PJDesign/](./docs/01_PJDesign/) | 项目设计文档（PRD / FSD / SSA / LLD） |
| [deploy/docker-compose.prod.yml](./deploy/docker-compose.prod.yml) | 生产部署编排文件 |
| [deploy/.env.example](./deploy/.env.example) | 环境变量模板 |

---

## 🙏 致谢

- **[Soybean Admin](https://github.com/soybeanjs/soybean-admin)** — 前端基于 Soybean Admin（Element Plus 版本）二次开发。感谢 [Soybean](https://github.com/soybeanjs) 团队提供的清新优雅的开源中后台框架！
- **[Avalonia UI](https://avaloniaui.net/)** — 跨平台桌面 UI 框架，用于边缘采集客户端
- **[YARP](https://github.com/microsoft/reverse-proxy)** — 微软开源反向代理，用于 API 网关

---

## 📄 许可证

本项目使用 **Apache License 2.0** 发布，详见 [LICENSE](./LICENSE) 与 [NOTICE](./NOTICE)。

### 反馈与支持

- Bug / 功能建议：[Issues](https://github.com/eatbs0956/FA_Device_Data_Collector_Public/issues)
- 安全漏洞：请通过邮件 `eatbs0956@163.com` **私下报告**，不要公开提交 Issue
