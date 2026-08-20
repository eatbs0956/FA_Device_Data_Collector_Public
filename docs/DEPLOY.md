# 部署指南

> 本文档对应 `deploy/docker-compose.prod.yml`。适用于单机 Docker 部署，面向中小规模
> （≤ 200 台设备、≤ 5000 tag 秒级采样）。更大规模请联系供应商获取 Kubernetes 方案。

## 1. 系统规划

| 组件             | 端口（默认） | 对外暴露 | 说明 |
|------------------|:-----------:|:-------:|------|
| `web` (nginx)    | 80          | ✓       | 管理前端 |
| `gateway-api`    | 18020       | ✓       | 所有 API 请求、SignalR |
| `auth-api`       | 8080        | ✗       | 仅集群内访问 |
| `admin-api`      | 8080        | ✗       | 设备/节点/监控 API，仅集群内访问 |
| `processor-worker` | 8080      | ✗       | 后台 Worker |
| `postgres`       | 5432        | 可选    | 仅本机访问建议 |
| `influxdb`       | 8086        | 可选    |  |
| `redis`          | 6379        | ✗       |  |
| `rabbitmq`       | 5672 / 15672 | 可选   | 15672 为管理 UI |

**关键建议**：仅暴露 `80` 和 `18020`；其余端口不要 publish 到宿主机。

## 2. 配置 `.env`

```bash
cp .env.example .env
mkdir -p secrets
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:3072 -out ./secrets/jwt-private.pem
chmod 600 ./secrets/jwt-private.pem
```

必改项：
- `IMAGE_TAG` — 锁定到具体版本（如 `v0.1.0`），避免 `latest` 导致不可预期升级
- `PUBLIC_API_BASE` — **浏览器访问的网关地址**，如 `https://dcp.example.com`
- `PG_PASSWORD` / `RABBITMQ_PASSWORD` / `INFLUX_TOKEN` / `INFLUX_PASSWORD` / `REDIS_PASSWORD` — 全部改强随机
- `JWT_PRIVATE_KEY_FILE` — 指向上述 PKCS#8/PKCS#1 PEM RSA 私钥；该文件会以只读 Compose secret 挂载，必须纳入安全备份且不可提交

## 3. 启动

```bash
docker login ghcr.io                            # 镜像为 private 时必需
docker compose -f docker-compose.prod.yml --env-file .env pull
docker compose -f docker-compose.prod.yml --env-file .env up -d
docker compose -f docker-compose.prod.yml ps    # 查看状态
docker compose -f docker-compose.prod.yml logs -f gateway-api
```

首次启动过程：
1. `postgres` / `influxdb` 初始化（30~60s）
2. `auth-api` 启动时自动执行 EF Core 迁移（约 5s）
3. `admin-api` / `processor-worker` 同上
4. `gateway-api` 绑定端口，对外提供服务

## 4. 反向代理与 HTTPS（推荐）

使用 Caddy / Nginx / Traefik 统一把 `80/443` 代理到 `web:80` 与 `gateway-api:18020`。Caddy 示例：

```Caddyfile
dcp.example.com {
    reverse_proxy /api/* gateway:18020
    reverse_proxy /hubs/* gateway:18020
    reverse_proxy /* web:80
}
```

将 `.env` 中 `PUBLIC_API_BASE` 改为 `https://dcp.example.com` 并重启 `web` 即可。

## 5. 升级

```bash
# 编辑 .env 中 IMAGE_TAG
docker compose -f docker-compose.prod.yml --env-file .env pull
docker compose -f docker-compose.prod.yml --env-file .env up -d
```

> 升级前建议：`docker exec devdcp-postgres pg_dump -U devdcp devdcp > backup-$(date +%F).sql`

## 6. 备份与恢复

| 资源       | 命令 |
|------------|------|
| PostgreSQL | `docker exec devdcp-postgres pg_dump -U $PG_USER $PG_DATABASE` |
| InfluxDB   | `docker exec devdcp-influx influx backup /var/lib/influxdb2/backup` |
| 卷         | `docker run --rm -v devdcp_pg-data:/data -v $(pwd):/bk alpine tar czf /bk/pg.tgz /data` |
| JWT 私钥   | 离线加密备份 `JWT_PRIVATE_KEY_FILE`；恢复时必须保持原私钥，才能继续验证重启前签发且未过期的令牌 |

## 7. 常见问题

**Q：容器一直重启？**
A：看日志 `docker compose logs <service>`；90% 是密码/Token 漏填或数据库迁移冲突。

**Q：前端访问正常但调 API 失败？**
A：检查 `.env` 中 `PUBLIC_API_BASE` 是否为浏览器可达地址（不能是 `http://gateway-api:8080`）。

**Q：想临时禁用某个服务？**
A：`docker compose -f docker-compose.prod.yml stop <service>`，重启时 `start` 即可。
