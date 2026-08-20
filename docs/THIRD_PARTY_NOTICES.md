# 第三方组件声明 / Third-Party Notices

本项目依赖以下第三方开源组件。按 LICENSE 要求在此列出其版权声明及许可证。

| 组件 | 版本 | 许可证 | 来源 |
|------|------|--------|------|
| .NET / ASP.NET Core / EF Core | 8.0.x | MIT | https://github.com/dotnet |
| Npgsql / Npgsql.EntityFrameworkCore.PostgreSQL | 8.0.x | PostgreSQL License | https://github.com/npgsql/npgsql |
| YARP (Yarp.ReverseProxy) | 2.3.x | MIT | https://github.com/microsoft/reverse-proxy |
| Serilog | 8.x / 10.x | Apache-2.0 | https://github.com/serilog/serilog |
| FluentValidation | 11.x | Apache-2.0 | https://github.com/FluentValidation/FluentValidation |
| RabbitMQ.Client | 6.8.x | Apache-2.0 / MPL-2.0 | https://github.com/rabbitmq/rabbitmq-dotnet-client |
| StackExchange.Redis | 2.7.x | MIT | https://github.com/StackExchange/StackExchange.Redis |
| Swashbuckle.AspNetCore | 6.5.x | MIT | https://github.com/domaindrivendev/Swashbuckle.AspNetCore |
| NModbus | — | MIT | https://github.com/NModbus/NModbus |
| S7netplus | — | MIT | https://github.com/S7NetPlus/s7netplus |
| Avalonia UI | — | MIT | https://github.com/AvaloniaUI/Avalonia |
| Vue | 3.x | MIT | https://github.com/vuejs/core |
| Vite | 7.x | MIT | https://github.com/vitejs/vite |
| Element Plus | — | MIT | https://github.com/element-plus/element-plus |
| Soybean Admin (Element Plus) | — | MIT | https://github.com/soybeanjs/soybean-admin-element-plus |
| ECharts | 5.x | Apache-2.0 | https://github.com/apache/echarts |
| nginx | 1.27 (alpine) | BSD-2-Clause | https://nginx.org |
| PostgreSQL | 14 | PostgreSQL License | https://www.postgresql.org |
| InfluxDB | 2.x | MIT (client) / MIT (server) | https://github.com/influxdata/influxdb |
| Redis | 7.x | BSD-3-Clause (≤7.2) / RSALv2+SSPLv1 (≥7.4) | https://redis.io |
| RabbitMQ | 3.x | Apache-2.0 / MPL-2.0 | https://www.rabbitmq.com |

> 本列表以发布日快照为准，详见 `package.json` / `*.csproj` 与各镜像的软件物料清单（SBOM）。
> 未列出但被间接引用的传递依赖同样沿用其各自许可证。

## 如何获取对应源码

- MIT / Apache-2.0 / BSD 等宽松许可组件：可通过上述 URL 获得其源码。
- 本项目自身源码未对外开放，但本 NOTICE 列出的所有依赖均由开发者直接从上游获取，未作修改或派生。

如有任何合规性疑问，请邮件 `eatbs0956@163.com`。
