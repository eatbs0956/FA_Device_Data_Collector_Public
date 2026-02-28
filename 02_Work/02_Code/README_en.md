# DevDCP — Industrial Data Collection & Processing Platform

<p align="center">
  <b>A unified platform for multi-protocol industrial device data collection, processing, storage, and visualization</b>
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
  <img src="https://img.shields.io/badge/⚠_Status-Alpha-orange?style=for-the-badge" alt="Alpha"/>
</p>

> **⚠️ Project Status: In Development (Alpha)**
>
> The core architecture and key modules have been completed, including: authentication service, API gateway, admin management service, data processing worker, web management frontend, edge collection client framework, and Modbus TCP driver.
> Additional protocol drivers (OPC UA, MQTT, Siemens S7, Mitsubishi MC, etc.) and advanced features (alarm rules, data aggregation, report export, etc.) are under active development.
> APIs and data structures may change in future versions.

> **[中文版 (README.md)](./README.md)**

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Service Ports](#-service-ports)
- [Acknowledgements](#-acknowledgements)
- [License](#-license)

---

## 📖 Overview

DevDCP is an industrial data collection platform designed for **single-factory manufacturing environments**. It addresses the following core challenges:

- **Unified Multi-Protocol Access** — Supports OPC UA, Modbus TCP/RTU, MQTT, Mitsubishi MC, Siemens S7, and other mainstream industrial protocols
- **Distributed Edge Collection** — Deploy edge collection nodes at workshop/production-line level with sub-second data acquisition
- **Real-time Data Processing** — Message-driven architecture with data cleansing, aggregation, and time-series storage
- **Visual Management** — Web management console for device configuration, real-time monitoring, historical queries, and alarm management
- **Standard API Integration** — RESTful APIs and WebSocket real-time push for MES and other upstream systems

### System Components

| Layer | Component | Description |
|-------|-----------|-------------|
| **Frontend** | Web Frontend | Vue3 management console (devices / data / rules / monitoring) |
| **Gateway** | Gateway.Api | YARP reverse proxy, routing, rate limiting, SignalR Hub |
| **Center Services** | Auth.Api | JWT authentication, RBAC authorization, user/role/menu management |
| | Admin.Api | Device management, grouping, node management, query aggregation, SignalR real-time push |
| | Processor.Worker | Consumes RabbitMQ messages, cleanses data, writes to InfluxDB |
| | Monitor.Api | Monitoring & health checks (reserved) |
| **Edge Collection** | Collector.Agent | .NET 8 + Avalonia desktop client with multi-protocol collection engine |
| | Collector.Agent.Legacy | .NET Framework 4.7.2 legacy collector (backward compatibility) |
| **Infrastructure** | Docker Compose | PostgreSQL + RabbitMQ + InfluxDB + Redis |

---

## 🏗 Architecture

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
│  JWT/RBAC    │  Devices/    │  Message consumer /            │
│              │  Nodes       │  Write to InfluxDB             │
└──────┬───────┴──────┬───────┴──────────┬─────────────────────┘
       │              │                  │
┌──────▼──────────────▼──────────────────▼─────────────────────┐
│  PostgreSQL    RabbitMQ     InfluxDB     Redis                │
│  :5432         :5672/:15672 :8086        :6379                │
└──────────────────────▲───────────────────────────────────────┘
                       │  AMQP
┌──────────────────────┴───────────────────────────────────────┐
│           Collector.Agent (Avalonia Desktop Client)           │
│         Edge collection node — deployed on shop floor         │
└──────────────────────────────────────────────────────────────┘
```

---

## 🛠 Tech Stack

| Category | Technology |
|----------|-----------|
| **Backend Runtime** | .NET 8 (ASP.NET Core) |
| **Desktop Client** | .NET 8 + Avalonia UI 11 + CommunityToolkit.Mvvm |
| **Legacy Collector** | .NET Framework 4.7.2 |
| **Frontend Framework** | Vue 3 + Vite + TypeScript + Element Plus + UnoCSS |
| **API Gateway** | YARP (Yet Another Reverse Proxy) |
| **Auth** | JWT + JWKS + RBAC |
| **Message Queue** | RabbitMQ (publisher confirms / dead-letter / prefetch) |
| **Relational DB** | PostgreSQL 14 (metadata / audit / idempotency) |
| **Time-Series DB** | InfluxDB 2.x (high-frequency collection data) |
| **Cache** | Redis 7 (session / real-time Pub/Sub) |
| **Real-time Comm** | SignalR (WebSocket full-duplex push) |
| **Logging** | Serilog |
| **Containerization** | Docker + Docker Compose |

---

## 📁 Project Structure

```
02_Work/02_Code/
├── infra/                        # Docker Compose infrastructure
│   ├── docker-compose.dev.yml    #   Development environment
│   └── docker-compose.yml        #   Production environment
├── scripts/                      # Initialization & ops scripts
│   ├── influxdb-init.ps1         #   InfluxDB initialization
│   ├── rabbitmq-init.ps1         #   RabbitMQ initialization (Windows)
│   └── rabbitmq-init.sh          #   RabbitMQ initialization (Linux)
├── platform/
│   ├── center/                   # ── Center Services ──
│   │   ├── Auth.Api/             #   Authentication service
│   │   ├── Admin.Api/            #   Management & aggregation service
│   │   ├── Gateway.Api/          #   API Gateway (YARP)
│   │   ├── Monitor.Api/          #   Monitoring service (reserved)
│   │   ├── Processor.Worker/     #   Data processing worker
│   │   ├── Shared.Tsdb/          #   Time-series DB shared library
│   │   ├── Shared.Realtime/      #   Real-time push shared library
│   │   └── SharedAuth.Library/   #   Auth shared library
│   ├── edge/                     # ── Modern Edge Collection (.NET 8) ──
│   │   ├── Collector.Agent/      #   Avalonia desktop collection client
│   │   └── Collector.Core/       #   Collection engine core library
│   └── edge-legacy/              # ── Legacy Collection (.NET Framework) ──
│       └── Collector.Agent.Legacy/
├── web/
│   └── frontend/                 # Vue3 frontend management platform
├── start-all-services.ps1        # Start all services (one-click)
├── stop-all-services.ps1         # Stop all services (one-click)
├── check-services.ps1            # Check service status
└── Directory.Build.props         # Global .NET build configuration
```

---

## 🚀 Getting Started

### Prerequisites

| Software | Minimum Version | Purpose |
|----------|----------------|---------|
| **Docker Desktop** | 4.x | Run infrastructure (PostgreSQL / RabbitMQ / InfluxDB / Redis) |
| **.NET SDK** | 8.0 | Build & run backend services |
| **Node.js** | 20.19+ | Frontend build |
| **pnpm** | 8.7+ | Frontend package manager |

### Step 1: Start Infrastructure

```powershell
cd 02_Work/02_Code/infra
docker compose -f docker-compose.dev.yml up -d
```

Verify all containers are running:

```powershell
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

You should see `devdcp-postgres`, `devdcp-rabbitmq`, `devdcp-influx`, and `devdcp-redis` all in running state.

### Step 2: Initialize Data Stores

```powershell
cd 02_Work/02_Code/scripts

# InfluxDB initialization (create bucket, token, etc.)
.\influxdb-init.ps1

# RabbitMQ initialization (create exchange, queue, binding)
.\rabbitmq-init.ps1
```

### Step 3: Start All Services (One-Click)

```powershell
cd 02_Work/02_Code
.\start-all-services.ps1
```

This script starts services in dependency order:
1. ✅ Check Docker infrastructure
2. ✅ Auth.Api (authentication service)
3. ✅ Admin.Api (management service)
4. ✅ Gateway.Api (API gateway)
5. ✅ Processor.Worker (data processing)
6. ✅ Frontend (dev server)

### Step 4: Start the Edge Collection Client

```powershell
cd 02_Work/02_Code/platform/edge/Collector.Agent
dotnet run
```

### Step 5: Access the System

Open your browser and navigate to: **http://localhost:9527**

---

### Start Individual Services Manually

If you prefer not to use the one-click script:

```powershell
# Authentication service
cd platform/center/Auth.Api
dotnet run --urls http://localhost:60621

# Management service
cd platform/center/Admin.Api
dotnet run --urls http://localhost:60623

# API Gateway
cd platform/center/Gateway.Api
dotnet run --urls http://localhost:60620

# Data processing worker
cd platform/center/Processor.Worker
dotnet run --urls http://localhost:60624

# Frontend
cd web/frontend
pnpm install
pnpm dev
```

---

## 🌐 Service Ports

| Service | Port | Description |
|---------|------|-------------|
| **Gateway.Api** | `60620` | API gateway entry point; all frontend requests are routed through here |
| **Auth.Api** | `60621` | Authentication; Swagger: `/swagger` |
| **Admin.Api** | `60623` | Management API; Swagger: `/swagger` |
| **Processor.Worker** | `60624` | Data processing; health check: `/health` |
| **Frontend** | `9527` | Vue3 frontend dev server |
| **PostgreSQL** | `5432` | Relational database (user: `devdcp` / password: `devdcp`) |
| **RabbitMQ** | `5672` / `15672` | Message queue / management panel (user: `devdcp` / password: `devdcp`) |
| **InfluxDB** | `8086` | Time-series database (Token: `devdcp-token`) |
| **Redis** | `6379` | Cache |

---

## 🙏 Acknowledgements

- **[Soybean Admin](https://github.com/soybeanjs/soybean-admin)** — The frontend of this project is built upon Soybean Admin (Element Plus edition). Huge thanks to the [Soybean](https://github.com/soybeanjs) team for providing such an elegant and well-crafted open-source admin framework!
- **[Avalonia UI](https://avaloniaui.net/)** — Cross-platform desktop UI framework powering the edge collection client
- **[YARP](https://github.com/microsoft/reverse-proxy)** — Microsoft's open-source reverse proxy used for the API gateway

---

## 📄 License

This project is licensed under the [MIT License](./LICENSE).

The frontend is customized from [Soybean Admin (MIT License)](https://github.com/soybeanjs/soybean-admin/blob/main/LICENSE).
