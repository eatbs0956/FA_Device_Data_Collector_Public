# 总体结构

## 顶部的 version: ‘3.8’ 
* 表示使用 Docker Compose 文件格式 v3.8（与 Docker Engine/Compose 的兼容版本有关）。
* services 下定义了若干个容器化服务：zookeeper、kafka、postgres、redis、keycloak、grafana、prometheus。
* volumes 下定义了一个命名卷 pgdata，用于持久化 PostgreSQL 的数据目录。

默认网络与主机名

* Docker Compose 会为这个堆栈创建一个默认网络（bridge），服务间可以直接通过服务名互相访问（例如 kafka 可以通过 hostname kafka 或 zookeeper:2181 访问 zookeeper）。
* 服务名即容器内的 DNS 名（可作为主机名在容器间解析）。

## 逐服务解析

### zookeeper
* image: bitnami/zookeeper:3.8
  * 使用 Bitnami 提供的 Zookeeper 3.8 镜像。
* environment:
  * ALLOW_ANONYMOUS_LOGIN=yes：允许匿名登录（不建议在生产环境使用，安全性低）。
* ports:
  * 2181:2181：将宿主机的 2181 端口映射到容器内的 2181（Zookeeper 默认客户端端口）。这意味着你可以通过 localhost:2181 访问 Zookeeper（如果防火墙/主机策略允许）。
* 作用：为 Kafka 提供元数据协调服务（集群配置信息、选举等）。

### kafka
* image: bitnami/kafka:3.5
  * 使用 Bitnami 的 Kafka 镜像，版本 3.5。
* environment（关键项）:
  * KAFKA_BROKER_ID=1：指定 broker id。
  * KAFKA_ZOOKEEPER_CONNECT=zookeeper:2181：指定 zookeeper 的地址（使用服务名 zookeeper）。
  * ALLOW_PLAINTEXT_LISTENER=yes：允许明文（非 TLS）监听。
  * KAFKA_LISTENERS=PLAINTEXT://:9092：容器内部监听所有接口的 9092 端口（用于客户端连接）。
  * KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://kafka:9092：向客户端广告的 broker 地址为 kafka:9092（注意细节，见下文）。
* ports:
  * 9092:9092：将宿主机的 9092 端口映射到容器的 9092。
* depends_on:
  * zookeeper：表示启动顺序上的依赖（docker-compose 会先启动 zookeeper 再启动 kafka），但这并不保证 zookeeper 已完全可用（不等于健康检查通过）。
* 重要注意（常见坑）：
  * KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://kafka:9092 表示当客户端（尤其是外部/宿主机上的客户端）向 broker 请求元数据时，broker 会告诉客户端连接地址为 kafka:9092。这个 hostname 在容器网络内可解析（其它容器可以通过 kafka:9092 连接），但在宿主机上直接使用 localhost:9092 连接时，客户端拿到的 kafka:9092 可能无法解析/访问，从而导致连接问题。要让宿主机客户端直接连接，通常需要把 advertised listener 设置为主机 IP 或 host.docker.internal（或直接设置为 localhost）或配置两个 listener（内网与外网）。
  * ALLOW_PLAINTEXT_LISTENER=yes 与 ALLOW_ANONYMOUS_LOGIN（ZK）意味着很多认证/加密被关闭，适合本地开发，不适合生产。
  
### postgres (TimescaleDB)
* image: timescale/timescaledb:latest-pg14
  * 使用 TimescaleDB（基于 PostgreSQL 14）镜像，TimescaleDB 是时序数据库扩展。
* environment:
  * POSTGRES_USER=postgres
  * POSTGRES_PASSWORD=postgres
  * POSTGRES_DB=daq
这些环境变量用于初始化数据库、创建用户与默认数据库（仅在数据目录为空、容器第一次启动时生效）。
* ports:
  * 5432:5432：宿主机 5432 映射到容器 5432（Postgres 默认端口）。
* volumes:
  * pgdata:/var/lib/postgresql/data：使用名为 pgdata 的命名卷来持久化数据库数据，保证容器重建后数据仍在。
* 作用：存储业务数据，并提供 Timescale 的时序扩展功能（常用于时序数据、监控数据等）。

### redis
* image: redis:7
* ports:
  * 6379:6379：宿主机 6379 映射到容器 6379（Redis 默认端口）。
* 说明：使用默认配置的 Redis，没有配置持久化卷（因此容器删除会丢失数据，除非 Redis 默认配置开启了 RDB/AOF 并映射了卷）。通常开发环境可以这样，生产建议挂载数据卷并加密码/ACL。

### keycloak
keycloak提供了单点登录(SSO)、身份验证、授权和用户管理等功能。它支持多种标准协议，如OpenID Connect、SAML 和OAuth，使得它能够与其他应用程序集成。
* image: quay.io/keycloak/keycloak:21.1.1
* environment:
  * KC_DB=dev-mem：使用内存型数据库（开发模式），数据不会持久化。
  * KC_HTTP_RELATIVE_PATH=/auth：服务相对路径为 /auth（意味着登录页面等会在 /auth 下）。
* command: start-dev：以开发模式启动 Keycloak（更宽松的安全、内存 DB、热加载等）。
* ports:
  * 8080:8080：宿主机 8080 映射到容器 8080。
* 说明：适合本地开发与测试；生产环境要配置外部数据库、TLS、持久化及其他安全设置。访问地址示例：http://localhost:8080/auth;


### grafana
Grafana 是一个开源的数据可视化和监控平台，主要用于将各种数据源的数据转换为易于理解的图表和仪表板。它支持多种数据源，包括Prometheus、Elasticsearch、InfluxDB 等，并且具有强大的可定制性和扩展性，可以通过插件来扩展功能。﻿
* image: grafana/grafana:9.5
* ports:
  * 3000:3000：宿主机 3000 映射到容器 3000（Grafana 默认 UI 端口）。
* 说明：没有指定持久化卷或管理员密码等默认设置。Grafana 启动后需要配置数据源（例如 Prometheus、Postgres 等）与 dashboard。

### prometheus
Prometheus 是一个开源的系统监控和告警工具集，最初由SoundCloud 开发，现在是云原生计算基金会(CNCF) 托管的项目之一。它主要用于收集和存储时间序列数据，并提供强大的查询语言(PromQL) 用于分析和告警。Prometheus 广泛应用于云原生环境，特别是Kubernetes 集群的监控。
* image: prom/prometheus:v2.47.0
* ports:
  * 9090:9090：宿主机 9090 映射到容器 9090（Prometheus web UI）。
* 说明：没有提供 prometheus.yml 配置文件或数据卷，所以容器的默认配置会被使用（通常不会有你想要的 scrape targets）。要用 Prometheus 定期抓取指标，需要把自定义配置通过 volumes 挂载进去。

### volumes
在Docker 中，"volumes" 特指数据卷，用于持久化和共享容器数据。
* pgdata: 定义了一个命名卷，Docker 会在宿主上创建并管理该卷，用于持久化 PostgreSQL 数据（映射到 /var/lib/postgresql/data）。


## 启动与常用命令

* 在该目录运行：docker-compose up -d
  * 会拉取镜像并在后台启动所有服务。
* 查看日志：docker-compose logs -f <service>
* 停止并删除容器：docker-compose down（默认不会删除命名卷，除非加 -v）

## 安全性与生产注意事项（重要）

* 多个服务都以明文与匿名方式运行（Kafka/Zookeeper 明文、Keycloak 内存 DB、Redis 无认证、Postgres 小白密码），仅适合本地开发或测试环境。生产环境必须：
  * 配置认证/授权（Kafka SASL、Redis ACL、Postgres 强密码/SSL 等）。
  * 使用持久化卷（不仅 PostgreSQL，Redis、Keycloak 的 DB 等也应持久化）。
  * 对 Keycloak 使用外部数据库（例如 PostgreSQL）、开启 TLS、配置备份。
  * 为 Prometheus 提供自定义配置文件（prometheus.yml），并把配置和数据持久化。
  * 配置 restart 策略（restart: unless-stopped 或类似），监控/日志持久化等。

* Kafka advertised listeners 问题：如果你希望宿主机上的客户端能稳定连接 Kafka，KAFKA_ADVERTISED_LISTENERS 应该设置为可从客户端解析的地址（例如宿主机 IP 或 host.docker.internal:9092），或者配置两个 listener（内部容器网络一个、外部访问一个）。否则外部客户端可能因元数据中返回的不可达主机名而连接失败。
* depends_on 只控制启动顺序，不保证服务已“就绪”。可使用健康检查（healthcheck）或脚本等待（wait-for-it、dockerize）来确保依赖服务可用再启动下游服务。

## 改进建议（根据使用场景）

* 若仅本地开发：
  * 可以接受当前配置，但建议为 Postgres/Redis/Kafka 使用更强的密码并把关键配置写入 .env 文件或 docker-compose.override.yml。
* 若要对外暴露 Kafka 给宿主或外网客户端：
  * 调整 KAFKA_ADVERTISED_LISTENERS，或在 kafka 上配置两个 listener（一个对容器网络，一个对外部）。
* 为 Prometheus 挂载自定义 prometheus.yml：
  * volumes:./prometheus.yml:/etc/prometheus/prometheus.yml:ro
* 为 Grafana 挂载数据卷以保留仪表盘和数据源设置：
  * volumes:grafana-data:/var/lib/grafana
* 为 Keycloak 使用外部数据库并持久化数据。
* 为 Redis 添加持久化卷或开启 AOF 并挂载卷。

## 访问示例（假设在本机运行）

* Zookeeper: localhost:2181
* Kafka: localhost:9092（但注意 advertised listener 的问题，见上）
* Postgres: psql -h localhost -U postgres -d daq -p 5432（密码 postgres）
* Redis: redis-cli -h localhost -p 6379
* Keycloak: http://localhost:8080/auth
* Grafana: http://localhost:3000
* Prometheus: http://localhost:9090

## 总结

* 这个 compose 文件定义了一套典型的开发/测试环境栈：Zookeeper + Kafka、TimescaleDB（Postgres）、Redis、Keycloak、Grafana、Prometheus。适合本地搭建数据采集/时序/监控/认证/可视化 的实验环境。
* 当前配置偏向开发方便（开放明文、内存 DB、少量持久化），若用于生产需要大量增强安全性、持久化和正确的网络配置（尤其 Kafka 的 advertised listeners）。