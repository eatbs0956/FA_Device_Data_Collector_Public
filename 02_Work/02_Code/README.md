# DevDCP — 工业数据采集与处理平台

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
  <img src="https://img.shields.io/badge/License-MIT-green" alt="License"/>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/⚠_开发阶段-Alpha-orange?style=for-the-badge" alt="Alpha"/>
</p>

> **⚠️ 项目状态：开发中（Alpha）**
>
> 目前已完成主体架构搭建与核心模块开发，包括：认证鉴权服务、API 网关、管理后台服务、数据处理 Worker、前端管理平台、边缘采集客户端框架及 Modbus TCP 驱动。
> 更多协议驱动（OPC UA、MQTT、Siemens S7、Mitsubishi MC 等）和高级功能（告警规则、数据聚合、报表导出等）正在持续开发中。
> API 和数据结构可能在后续版本中发生变更。

> **[English Version (README_en.md)](./README_en.md)**

---

## 📋 目录

- [项目简介](#-项目简介)
- [系统架构](#-系统架构)
- [技术栈](#-技术栈)
- [目录结构](#-目录结构)
- [快速开始](#-快速开始)
- [服务端口清单](#-服务端口清单)
- [致谢](#-致谢)
- [许可证](#-许可证)

---

## 📖 项目简介

DevDCP 是一个面向**制造业单工厂内部**的工业数据采集通用后台系统，解决以下核心问题：

- **多协议统一接入** — 支持 OPC UA、Modbus TCP/RTU、MQTT、三菱 MC、西门子 S7 等主流工业协议
- **分布式边缘采集** — 可在车间/产线级部署边缘采集节点，秒级数据采集
- **实时数据处理** — 消息驱动架构，支持数据清洗、聚合、时序存储
- **可视化管理** — Web 管理平台实现设备配置、实时监控、历史查询、告警管理
- **标准 API 集成** — 为 MES 等上层系统提供 RESTful API 和 WebSocket 实时推送

### 系统组成

| 层级 | 组件 | 说明 |
|------|------|------|
| **前端** | Web Frontend | Vue3 管理控制台（设备/数据/规则/监控） |
| **网关** | Gateway.Api | YARP 反向代理、路由、限流、SignalR Hub |
| **中心服务** | Auth.Api | JWT 认证、RBAC 权限、用户/角色/菜单管理 |
| | Admin.Api | 设备管理、分组、节点、查询聚合、SignalR 实时推送 |
| | Processor.Worker | 消费 RabbitMQ 消息，数据清洗后写入 InfluxDB |
| | Monitor.Api | 监控与健康检查（预留） |
| **边缘采集** | Collector.Agent | .NET 8 + Avalonia 桌面客户端，多协议采集引擎 |
| | Collector.Agent.Legacy | .NET Framework 4.7.2 旧版采集（向下兼容） |
| **基础设施** | Docker Compose | PostgreSQL + RabbitMQ + InfluxDB + Redis |

---

## 🏗 系统架构

```
┌──────────────────────────────────────────────────────────────┐
│                    Web Frontend (Vue3)                        │
│                   http://localhost:9527                       │
└──────────────────────┬───────────────────────────────────────┘
                       │
┌──────────────────────▼───────────────────────────────────────┐
│              Gateway.Api (YARP + SignalR Hub)                 │
│                   http://localhost:60620                      │
├──────────────┬──────────────┬────────────────────────────────┤
│  Auth.Api    │  Admin.Api   │  Processor.Worker              │
│  :60621      │  :60623      │  :60624                        │
│  JWT/RBAC    │  设备/节点    │  消息消费/写入 InfluxDB         │
└──────┬───────┴──────┬───────┴──────────┬─────────────────────┘
       │              │                  │
┌──────▼──────────────▼──────────────────▼─────────────────────┐
│  PostgreSQL    RabbitMQ     InfluxDB     Redis                │
│  :5432         :5672/:15672 :8086        :6379                │
└──────────────────────▲───────────────────────────────────────┘
                       │  AMQP
┌──────────────────────┴───────────────────────────────────────┐
│           Collector.Agent (Avalonia 桌面客户端)                │
│         边缘采集节点 — 部署在车间/产线现场                      │
└──────────────────────────────────────────────────────────────┘
```

---

## 🛠 技术栈

| 分类 | 技术 |
|------|------|
| **后端运行时** | .NET 8 (ASP.NET Core) |
| **桌面客户端** | .NET 8 + Avalonia UI 11 + CommunityToolkit.Mvvm |
| **旧版采集** | .NET Framework 4.7.2 |
| **前端框架** | Vue 3 + Vite + TypeScript + Element Plus + UnoCSS |
| **API 网关** | YARP (Yet Another Reverse Proxy) |
| **认证授权** | JWT + JWKS + RBAC |
| **消息队列** | RabbitMQ（发布确认 / 死信队列 / Prefetch） |
| **关系数据库** | PostgreSQL 14（元数据 / 审计 / 幂等） |
| **时序数据库** | InfluxDB 2.x（高频采集数据存储） |
| **缓存** | Redis 7（会话 / 实时推送 Pub/Sub） |
| **实时通信** | SignalR（WebSocket 全双工推送） |
| **日志** | Serilog |
| **容器化** | Docker + Docker Compose |

---

## 📁 目录结构

```
02_Work/02_Code/
├── infra/                        # Docker Compose 基础设施编排
│   ├── docker-compose.dev.yml    #   开发环境
│   └── docker-compose.yml        #   生产环境
├── scripts/                      # 初始化与运维脚本
│   ├── influxdb-init.ps1         #   InfluxDB 初始化
│   ├── rabbitmq-init.ps1         #   RabbitMQ 初始化 (Windows)
│   └── rabbitmq-init.sh          #   RabbitMQ 初始化 (Linux)
├── platform/
│   ├── center/                   # ── 中心服务 ──
│   │   ├── Auth.Api/             #   认证鉴权服务
│   │   ├── Admin.Api/            #   管理业务聚合服务
│   │   ├── Gateway.Api/          #   API 网关 (YARP)
│   │   ├── Monitor.Api/          #   监控服务（预留）
│   │   ├── Processor.Worker/     #   数据处理 Worker
│   │   ├── Shared.Tsdb/          #   时序数据库共享库
│   │   ├── Shared.Realtime/      #   实时推送共享库
│   │   └── SharedAuth.Library/   #   认证共享库
│   ├── edge/                     # ── 现代边缘采集 (.NET 8) ──
│   │   ├── Collector.Agent/      #   Avalonia 桌面采集客户端
│   │   └── Collector.Core/       #   采集引擎核心库
│   └── edge-legacy/              # ── 旧版采集 (.NET Framework) ──
│       └── Collector.Agent.Legacy/
├── web/
│   └── frontend/                 # Vue3 前端管理平台
├── start-all-services.ps1        # 一键启动所有服务
├── stop-all-services.ps1         # 一键停止所有服务
├── check-services.ps1            # 检查服务状态
└── Directory.Build.props         # 全局 .NET 编译配置
```

---

## 🚀 快速开始

### 环境要求

| 软件 | 最低版本 | 用途 |
|------|----------|------|
| **Docker Desktop** | 4.x | 运行基础设施 (PostgreSQL / RabbitMQ / InfluxDB / Redis) |
| **.NET SDK** | 8.0 | 编译运行后端服务 |
| **Node.js** | 20.19+ | 前端构建 |
| **pnpm** | 8.7+ | 前端包管理 |

### 第一步：启动基础设施

```powershell
cd 02_Work/02_Code/infra
docker compose -f docker-compose.dev.yml up -d
```

验证所有容器正常运行：

```powershell
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

应看到 `devdcp-postgres`、`devdcp-rabbitmq`、`devdcp-influx`、`devdcp-redis` 均为 running。

### 第二步：初始化数据

```powershell
cd 02_Work/02_Code/scripts

# InfluxDB 初始化（创建 bucket、token 等）
.\influxdb-init.ps1

# RabbitMQ 初始化（创建 exchange、queue、binding）
.\rabbitmq-init.ps1
```

### 第三步：一键启动所有服务

```powershell
cd 02_Work/02_Code
.\start-all-services.ps1
```

该脚本会按依赖顺序启动：
1. ✅ 检查 Docker 基础设施
2. ✅ Auth.Api（认证服务）
3. ✅ Admin.Api（管理服务）
4. ✅ Gateway.Api（API 网关）
5. ✅ Processor.Worker（数据处理）
6. ✅ Frontend（前端开发服务器）

### 第四步：启动边缘采集客户端

```powershell
cd 02_Work/02_Code/platform/edge/Collector.Agent
dotnet run
```

### 第五步：访问系统

打开浏览器访问前端：**http://localhost:9527**

---

### 手动启动单个服务

如果不使用一键脚本，也可以单独启动各服务：

```powershell
# 认证服务
cd platform/center/Auth.Api
dotnet run --urls http://localhost:60621

# 管理服务
cd platform/center/Admin.Api
dotnet run --urls http://localhost:60623

# API 网关
cd platform/center/Gateway.Api
dotnet run --urls http://localhost:60620

# 数据处理
cd platform/center/Processor.Worker
dotnet run --urls http://localhost:60624

# 前端
cd web/frontend
pnpm install
pnpm dev
```

---

## 🌐 服务端口清单

| 服务 | 端口 | 说明 |
|------|------|------|
| **Gateway.Api** | `60620` | API 网关入口，前端所有请求经由此转发 |
| **Auth.Api** | `60621` | 认证鉴权，Swagger: `/swagger` |
| **Admin.Api** | `60623` | 管理业务，Swagger: `/swagger` |
| **Processor.Worker** | `60624` | 数据处理，健康检查: `/health` |
| **Frontend** | `9527` | Vue3 前端开发服务器 |
| **PostgreSQL** | `5432` | 关系数据库 (用户: `devdcp` / 密码: `devdcp`) |
| **RabbitMQ** | `5672` / `15672` | 消息队列 / 管理面板 (用户: `devdcp` / 密码: `devdcp`) |
| **InfluxDB** | `8086` | 时序数据库 (Token: `devdcp-token`) |
| **Redis** | `6379` | 缓存 |

---

## 🙏 致谢

- **[Soybean Admin](https://github.com/soybeanjs/soybean-admin)** — 本项目前端基于 Soybean Admin (Element Plus 版本) 二次开发。感谢 [Soybean](https://github.com/soybeanjs) 团队提供的清新优雅的开源中后台框架！
- **[Avalonia UI](https://avaloniaui.net/)** — 跨平台桌面 UI 框架，用于边缘采集客户端
- **[YARP](https://github.com/microsoft/reverse-proxy)** — 微软开源反向代理，用于 API 网关

---

## 📄 许可证

本项目采用 [MIT License](./LICENSE) 开源。

前端部分基于 [Soybean Admin (MIT License)](https://github.com/soybeanjs/soybean-admin/blob/main/LICENSE) 进行定制开发。
