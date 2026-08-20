# FA Device Data Collector (DCP) — Industrial Data Collection & Processing Platform

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
  <img src="https://img.shields.io/badge/License-Apache--2.0-blue.svg" alt="License"/>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/⚠_Status-Alpha-orange?style=for-the-badge" alt="Alpha"/>
  <a href="https://github.com/eatbs0956/FA_Device_Data_Collector_Public/releases/latest">
    <img src="https://img.shields.io/github/v/release/eatbs0956/FA_Device_Data_Collector_Public?label=latest%20release&style=for-the-badge" alt="Release"/>
  </a>
</p>

> **⚠️ Project Status: In Development (Alpha)**
>
> The core architecture and key modules are complete: authentication service, API gateway, admin service, data processing worker, web frontend, edge collection client, and **10 protocol drivers** (Modbus TCP/RTU, OPC UA, OPC DA, Siemens S7, Mitsubishi MC, MQTT, HTTP, EtherNet/IP (CIP), Beckhoff ADS).
> Advanced features (aggregation, report export, etc.) are under active development. APIs and data structures may change across versions.

> **[中文版 (README.md)](./README.md)**

> **This is the public assets repository.**
> It contains only documentation, deployment artifacts, LICENSE, and issue templates for end users.
> The full source code lives in a private repository and is not publicly available.
> Binaries are distributed via [GitHub Releases](https://github.com/eatbs0956/FA_Device_Data_Collector_Public/releases) and [GHCR images](https://github.com/eatbs0956?tab=packages).

---

## 📋 Table of Contents

- [Overview](#-overview)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Quick Deploy](#-quick-deploy)
- [Service Ports](#-service-ports)
- [Edge Collector (Windows)](#-edge-collector-windows)
- [Versioning & Upgrades](#-versioning--upgrades)
- [Documentation](#-documentation)
- [Acknowledgements](#-acknowledgements)
- [License](#-license)

---

## 📖 Overview

FA Device Data Collector (**DCP**) is an industrial data collection platform designed for **single-factory manufacturing environments**. It addresses the following challenges:

- **Unified multi-protocol access** — Modbus TCP/RTU, OPC UA / OPC DA, Siemens S7, Mitsubishi MC, MQTT, HTTP REST, EtherNet/IP (CIP), Beckhoff ADS (10 mainstream industrial protocols)
- **Distributed edge collection** — Deploy edge nodes at workshop / production-line level with sub-second sampling
- **Real-time processing** — Message-driven architecture with data cleansing, aggregation, and time-series storage
- **Visual management** — Web console for device configuration, real-time monitoring, historical queries, and alarms
- **Standard API integration** — RESTful APIs and WebSocket real-time push for MES and upstream systems

### System components

| Layer | Component | Image / Artifact |
|-------|-----------|------------------|
| **Frontend** | Web console (Vue 3 + Element Plus) | `ghcr.io/eatbs0956/fa-dc-web` |
| **Gateway** | Gateway.Api (YARP reverse proxy + SignalR Hub) | `ghcr.io/eatbs0956/fa-dc-gateway` |
| **Center services** | Auth.Api (JWT / RBAC) | `ghcr.io/eatbs0956/fa-dc-auth` |
|  | Admin.Api (devices / nodes / query aggregation / monitoring) | `ghcr.io/eatbs0956/fa-dc-admin` |
|  | Processor.Worker (consume RabbitMQ → InfluxDB) | `ghcr.io/eatbs0956/fa-dc-processor` |
| **Edge** | Collector.Agent (.NET 8 + Avalonia desktop) | `Collector.Agent-vX.Y.Z-win-x64.zip` |
| **Infrastructure** | PostgreSQL + RabbitMQ + InfluxDB + Redis | Official images |

---

## 🏗 Architecture

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
│  JWT/RBAC    │  Devices/    │  Consume RabbitMQ →            │
│              │  Nodes       │  Write InfluxDB                │
└──────┬───────┴──────┬───────┴──────────┬─────────────────────┘
       │              │                  │
┌──────▼──────────────▼──────────────────▼─────────────────────┐
│   PostgreSQL    RabbitMQ      InfluxDB      Redis             │
│     :5432       :5672/:15672  :8086         :6379             │
└──────────────────────▲───────────────────────────────────────┘
                       │  AMQP / HTTPS
┌──────────────────────┴───────────────────────────────────────┐
│        Collector.Agent (Windows x64 desktop client)           │
│        Edge node — deployed on the shop floor                 │
└──────────────────────────────────────────────────────────────┘
```

> Only ports `80` (web) and `18020` (gateway) are exposed externally by default; other services communicate on the internal network only.

---

## 🛠 Tech Stack

| Category | Technology |
|----------|-----------|
| **Backend runtime** | .NET 8 (ASP.NET Core) |
| **Desktop client** | .NET 8 + Avalonia UI 11 + CommunityToolkit.Mvvm |
| **Frontend** | Vue 3 + Vite + TypeScript + Element Plus + UnoCSS |
| **API gateway** | YARP (Yet Another Reverse Proxy) |
| **Auth** | JWT + JWKS + RBAC |
| **Message queue** | RabbitMQ (publisher confirms / dead-letter / prefetch) |
| **Relational DB** | PostgreSQL 14 (metadata / audit / idempotency) |
| **Time-series DB** | InfluxDB 2.x (high-frequency collection data) |
| **Cache** | Redis 7 (session / real-time Pub/Sub) |
| **Real-time comm** | SignalR (WebSocket full-duplex) |
| **Logging** | Serilog |
| **Containerization** | Docker + Docker Compose |

---

## 🚀 Quick Deploy

### Prerequisites

| Software | Minimum | Purpose |
|----------|---------|---------|
| **Linux server** | Ubuntu 22.04+ / RHEL 8+ | Runs center services (Windows Server 2022 + WSL2 also works) |
| **Docker Engine** | 24.x | Container runtime |
| **Docker Compose** | v2 | Multi-container orchestration |

Exposed ports: `80` (web), `18020` (gateway API).

### Step 1: Download deployment files

```bash
mkdir dcp && cd dcp

curl -LO https://github.com/eatbs0956/FA_Device_Data_Collector_Public/releases/latest/download/docker-compose.prod.yml
curl -LO https://github.com/eatbs0956/FA_Device_Data_Collector_Public/releases/latest/download/.env.example
cp .env.example .env
```

### Step 2: Edit `.env`

Variables you **must** change:

| Variable | Description |
|----------|-------------|
| `IMAGE_TAG` | Pin the image version, e.g. `v0.1.0` (avoid `latest` in production) |
| `PUBLIC_API_BASE` | Gateway URL **reachable from the browser**, e.g. `https://dcp.example.com` or `http://10.0.0.5:18020` |
| `PG_PASSWORD` / `RABBITMQ_PASSWORD` / `INFLUX_PASSWORD` / `INFLUX_TOKEN` / `REDIS_PASSWORD` | Strong random passwords |
| `JWT_PRIVATE_KEY_FILE` | Path to a PKCS#8/PKCS#1 PEM RSA private key; generate one with `openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:3072 -out ./secrets/jwt-private.pem` |

### Step 3: Pull images and start

```bash
# Login to GHCR if images are private (PAT with read:packages scope)
docker login ghcr.io

docker compose -f docker-compose.prod.yml --env-file .env pull
docker compose -f docker-compose.prod.yml --env-file .env up -d

# Wait for all containers to become healthy
docker compose -f docker-compose.prod.yml ps
```

First-time startup order:
1. `postgres` / `influxdb` / `rabbitmq` initialize (30 – 60 s)
2. `auth-api` / `admin-api` / `processor-worker` **auto-run database migrations** at startup (~5 s, no manual action needed)
3. `gateway-api` binds 18020, `web` binds 80

### Step 4: Access the system

Open the browser at **`http://<server-ip>/`** or your configured domain.

The default admin account is listed in the Release Notes; you will be forced to change the password on first login.

---

### Advanced: reverse proxy & HTTPS (production recommended)

Use Caddy / Nginx / Traefik to terminate TLS on `443` and proxy to `web:80` and `gateway-api:18020`. Example Caddyfile:

```Caddyfile
dcp.example.com {
    reverse_proxy /api/*  gateway:18020
    reverse_proxy /hubs/* gateway:18020
    reverse_proxy /*      web:80
}
```

Full deployment guide: [docs/DEPLOY.md](./docs/DEPLOY.md).

---

## 🌐 Service Ports

| Service | Default port | Exposed | Description |
|---------|:------------:|:-------:|-------------|
| **web** | `80` | ✓ | Vue 3 console (served by nginx) |
| **gateway-api** | `18020` | ✓ | API gateway entry, SignalR real-time push |
| **auth-api** | 8080 | ✗ | JWT auth, user / role / menu |
| **admin-api** | 8080 | ✗ | Devices, nodes, query aggregation, monitoring APIs |
| **processor-worker** | 8080 | ✗ | Consumes RabbitMQ, writes InfluxDB |
| **postgres** | 5432 | optional | Keep internal in production |
| **influxdb** | 8086 | optional | Keep internal in production |
| **rabbitmq** | 5672 / 15672 | optional | 5672=AMQP; 15672=management UI |
| **redis** | 6379 | ✗ | Internal only |

---

## 💻 Edge Collector (Windows)

1. Download the matching `Collector.Agent-vX.Y.Z-win-x64.zip` from [Releases](https://github.com/eatbs0956/FA_Device_Data_Collector_Public/releases)
2. Extract to any directory, e.g. `C:\dcp\agent`
3. Edit `appsettings.json`:
   - Set `Gateway:BaseUrl` to `http://<server-ip>:18020`
   - Fill in registration code / tag definitions
4. Run:
   - **Dev / debug**: double-click `Collector.Agent.exe`
   - **Production**: register as a Windows Service (`sc create`, NSSM, etc.)

---

## 🔄 Versioning & Upgrades

Versions follow [Semantic Versioning](https://semver.org/): `vMAJOR.MINOR.PATCH`.

| Tag | Meaning |
|-----|---------|
| `latest` | Most recent stable release (not recommended to pin in production) |
| `vX.Y` | Rolling to the latest patch of that minor line |
| `vX.Y.Z` | Exact immutable version (**recommended for production**) |
| `vX.Y.Z-rc.N` | Pre-release (RC) |

Upgrade flow:

```bash
# 1. Edit .env and bump IMAGE_TAG
# 2. Pull and rolling restart
docker compose -f docker-compose.prod.yml --env-file .env pull
docker compose -f docker-compose.prod.yml --env-file .env up -d

# (Recommended) back up PostgreSQL before upgrading
docker exec devdcp-postgres pg_dump -U devdcp devdcp > backup-$(date +%F).sql
```

Database migrations run automatically at service startup; no manual step is required.

---

## 📚 Documentation

| Document | Description |
|----------|-------------|
| [docs/DEPLOY.md](./docs/DEPLOY.md) | Detailed deployment (ports, reverse proxy, backup, FAQ) |
| [docs/THIRD_PARTY_NOTICES.md](./docs/THIRD_PARTY_NOTICES.md) | Third-party component licenses |
| [docs/01_PJDesign/](./docs/01_PJDesign/) | Design docs (PRD / FSD / SSA / LLD) |
| [deploy/docker-compose.prod.yml](./deploy/docker-compose.prod.yml) | Production compose file |
| [deploy/.env.example](./deploy/.env.example) | Environment variable template |

---

## 🙏 Acknowledgements

- **[Soybean Admin](https://github.com/soybeanjs/soybean-admin)** — The frontend is built upon Soybean Admin (Element Plus edition). Huge thanks to the [Soybean](https://github.com/soybeanjs) team for their elegant open-source admin framework!
- **[Avalonia UI](https://avaloniaui.net/)** — Cross-platform desktop UI framework powering the edge collection client
- **[YARP](https://github.com/microsoft/reverse-proxy)** — Microsoft's open-source reverse proxy used for the API gateway

---

## 📄 License

This project is released under the **Apache License 2.0**. See [LICENSE](./LICENSE) and [NOTICE](./NOTICE).

### Feedback & Support

- Bugs / feature requests: [Issues](https://github.com/eatbs0956/FA_Device_Data_Collector_Public/issues)
- Security vulnerabilities: please email `eatbs0956@163.com` **privately**; do not file a public issue.
