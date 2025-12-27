# DevDCP 微服务管理脚本使用说明

> **快速开始：** 如果您第一次使用，请直接跳转到 [🚀 5分钟快速开始](#-5分钟快速开始) 章节

---

## 📋 目录

- [5分钟快速开始](#-5分钟快速开始)
- [脚本位置](#-脚本位置)
- [前置条件](#-前置条件重要)
- [基本操作](#-基本操作)
- [典型工作流程](#-典型工作流程)
- [常见问题](#-常见问题)
- [高级技巧](#-高级技巧)
- [架构说明](#-架构说明)

---

## 🚀 5分钟快速开始

### 前置条件检查
```powershell
# 1. 确保 Docker Desktop 运行（查看任务栏 Docker 图标）
# 2. 打开 PowerShell，进入项目目录
cd D:\00_QC-share\01_DevDCP\02_work\02_code
```

### 启动 Docker 基础设施
```powershell
# 进入 infra 目录
cd infra

# 启动 PostgreSQL 数据库（必需）
docker-compose -f docker-compose.dev.yml up -d postgres

# 或启动所有基础服务（PostgreSQL + RabbitMQ + InfluxDB + Redis）
docker-compose -f docker-compose.dev.yml up -d

# 返回项目根目录
cd ..
```

### 检查 Docker 服务状态
```powershell
.\check-docker-services.ps1
# 输出应显示：[OK] Docker Desktop is running
#              [OK] PostgreSQL container is running (healthy)
```

### 启动所有微服务
```powershell
.\start-all-services.ps1
# 会自动打开 3 个窗口：Auth.Api、Device.Api、Frontend
# 等待 10-15 秒后访问：http://localhost:3200
```

### 验证服务运行
```powershell
.\check-services.ps1
# 输出应显示所有服务运行正常 (3/3)
```

### 服务地址
- **前端**: http://localhost:3200
- **Auth.Api**: http://localhost:60621
- **Device.Api**: http://localhost:60623
- **数据库**: localhost:5432 (数据库: devdcp, 用户: devdcp, 密码: devdcp)

### 停止所有服务
```powershell
# 停止微服务
.\stop-all-services.ps1

# 停止 Docker 容器（可选）
cd infra
docker-compose -f docker-compose.dev.yml down
```

---

## 📁 脚本位置

所有脚本位于：`d:\00_QC-share\01_DevDCP\02_Work\02_Code\`

```
02_Code/
├── start-all-services.ps1      # 启动所有服务（Auth.Api + Device.Api + Frontend）
├── stop-all-services.ps1       # 停止所有服务（关闭所有 dotnet 和 node 进程）
├── start-service.ps1           # 启动单个服务（支持 Auth.Api / Device.Api / Frontend）
├── check-services.ps1          # 检查微服务状态（应用层：.NET + 前端）
├── check-docker-services.ps1   # 检查 Docker 基础设施（PostgreSQL / RabbitMQ / InfluxDB / Redis）
└── README-服务管理.md          # 本文档
```

### 📊 脚本功能对比表

| 脚本名称 | 功能 | 检查对象 | 使用场景 |
|---------|------|---------|---------|
| `start-all-services.ps1` | 启动所有微服务 | - | 开发启动 |
| `stop-all-services.ps1` | 停止所有微服务 | - | 下班关闭 |
| `start-service.ps1` | 启动单个微服务 | - | 调试单个服务 |
| `check-services.ps1` | 检查微服务状态 | Auth.Api、Device.Api、Frontend | 验证应用是否运行 |
| `check-docker-services.ps1` | 检查 Docker 容器状态 | PostgreSQL、RabbitMQ、InfluxDB、Redis | 验证基础设施是否就绪 |

## ⚠️ 重要说明

**PowerShell 脚本编码问题：**
- 所有 `.ps1` 脚本文件现在使用**英文提示信息**，避免 Windows PowerShell 5.1 的中文编码问题
- 如果看到乱码，请确保使用 PowerShell 7+ (pwsh.exe) 或重新下载脚本文件
- VS Code 中编辑 PowerShell 脚本时，请确保文件编码为 **UTF-8 with BOM**

**第一次使用建议：**
- 如果您是第一次启动项目，建议先阅读 [🚀 5分钟快速开始](#-5分钟快速开始) 章节
- 遇到问题请参考 [🐛 常见问题](#-常见问题) 章节

---

## � 前置条件（重要！）

### 1️⃣ 必须先启动 Docker 环境

**DevDCP 微服务依赖以下 Docker 服务：**
- ✅ **PostgreSQL 14**（必需）- 共享数据库
- ⚠️ RabbitMQ（可选）- 消息队列
- ⚠️ InfluxDB 2.x（可选）- 时序数据
- ⚠️ Redis 7（可选）- 缓存

**快速启动 Docker 环境：**

```powershell
# 1. 进入 infra 目录
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code\infra

# 2. 启动所有 Docker 服务（包括 PostgreSQL、RabbitMQ、InfluxDB、Redis）
docker-compose -f docker-compose.dev.yml up -d

# 3. 仅启动必需的 PostgreSQL
docker-compose -f docker-compose.dev.yml up -d postgres

# 4. 检查服务状态
docker-compose -f docker-compose.dev.yml ps
```

### 2️⃣ 检查 Docker 服务状态（新增功能）

```powershell
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code
.\check-docker-services.ps1
```

**输出示例：**
```
🐳 DevDCP Docker 服务状态检查
================================
[1/5] 检查 Docker Desktop...
✅ Docker Desktop 运行中

[2/5] 检查 PostgreSQL 容器...
✅ PostgreSQL 容器运行中 (健康)

[3/5] 检查 RabbitMQ 容器 (可选)...
✅ RabbitMQ 容器运行中 (健康)

✅ 核心服务检查完成
```

---

## 🎯 基本操作

### 1️⃣ 启动所有服务（开发必备）

```powershell
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code
.\start-all-services.ps1
```

**新特性：**
- ✅ 自动检查 Docker 服务是否运行
- ✅ 等待 Auth.Api 启动完成后再启动 Device.Api
- ✅ 健康检查确保服务就绪
- ✅ 自动打开 3 个 PowerShell 窗口
- ✅ 每个窗口显示实时日志

**服务地址：**
- Auth.Api: http://localhost:60621
- Device.Api: http://localhost:60623
- Frontend: http://localhost:3200

**数据库信息：**
- 数据库: `devdcp` (PostgreSQL 14)
- 地址: localhost:5432
- 用户: devdcp / 密码: devdcp

---

### 2️⃣ 停止所有服务（下班必做）

```powershell
.\stop-all-services.ps1
```

**效果：**
- 关闭所有 dotnet 进程（后端）
- 关闭所有 node 进程（前端）
- 释放所有端口

---

### 3️⃣ 启动单个服务（调试专用）

```powershell
# 只启动 Auth.Api
.\start-service.ps1 Auth.Api

# 只启动 Device.Api
.\start-service.ps1 Device.Api

# 只启动前端
.\start-service.ps1 Frontend
```

**使用场景：**
- 只修改某个服务，不需要启动全部
- 节省系统资源
- 快速测试单个服务

---

### 4️⃣ 检查服务状态

#### 检查微服务状态（Auth.Api、Device.Api、Frontend）
```powershell
.\check-services.ps1
```

**输出示例：**
```
📊 DevDCP 服务状态检查
✅ Auth.Api              运行中 (端口 60621)
✅ Device.Api            运行中 (端口 60623)
✅ Frontend              运行中 (端口 9527)

✅ 所有服务运行正常 (3/3)

📋 进程信息：
   dotnet 进程: 2 个
   node 进程: 1 个
```

#### 检查 Docker 基础设施状态（PostgreSQL、RabbitMQ 等）
```powershell
.\check-docker-services.ps1
```

**输出示例：**
```
DevDCP Docker Service Status Check
================================
[1/5] Checking Docker Desktop...
[OK] Docker Desktop is running

[2/5] Checking PostgreSQL container...
[OK] PostgreSQL container is running (healthy)

...
```

**两个检查脚本的区别：**
- `check-services.ps1`：检查**应用层微服务**（.NET 和前端）
- `check-docker-services.ps1`：检查**基础设施层**（Docker 容器）

---

## 📖 典型工作流程

### 早上开始工作
```powershell
# 1. 确保 Docker Desktop 运行
# 2. 启动 Docker 基础服务
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code\infra
docker-compose -f docker-compose.dev.yml up -d postgres

# 3. 检查 Docker 服务状态
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code
.\check-docker-services.ps1

# 4. 一键启动所有微服务
.\start-all-services.ps1

# 5. 等待 10-15 秒后访问前端
# http://localhost:3200
```

### 只修改前端代码
```powershell
# 1. 启动 Docker 服务
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code\infra
docker-compose -f docker-compose.dev.yml up -d postgres

# 2. 启动后端服务
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code
.\start-service.ps1 Auth.Api
.\start-service.ps1 Device.Api

# 3. 在前端目录手动启动
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code\web\frontend
pnpm dev
```

### 只修改某个后端服务
```powershell
# 1. 启动 Docker 服务
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code\infra
docker-compose -f docker-compose.dev.yml up -d postgres

# 2. 启动依赖的服务（Device.Api 依赖 Auth.Api）
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code
.\start-service.ps1 Auth.Api

# 3. 在 VS Code 中按 F5 调试 Device.Api
# 或使用脚本启动
.\start-service.ps1 Device.Api
```

### 下班前
```powershell
# 1. 停止所有微服务
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code
.\stop-all-services.ps1

# 2. 停止 Docker 服务（可选，不停止也可以）
cd infra
docker-compose -f docker-compose.dev.yml down
```

---

## 🔧 添加新服务

当您开发新的 API 服务（如 Monitor.Api、Query.Api）时：

### 1. 修改 `start-all-services.ps1`

在 "启动后端服务" 部分添加：

```powershell
# 3. 启动 Monitor.Api (监控服务)
Write-Host "`n📌 [3/3] 启动 Monitor.Api (端口 60625/60626)..." -ForegroundColor Yellow
Start-Process pwsh -ArgumentList @(
    "-NoExit",
    "-Command",
    "Set-Location '$baseDir\Monitor.Api'; `$host.ui.RawUI.WindowTitle='DevDCP - Monitor.Api'; Write-Host '📊 Monitor.Api 启动中...' -ForegroundColor Green; dotnet run"
) -WindowStyle Normal
```

### 2. 修改 `start-service.ps1`

在 ValidateSet 中添加新服务：

```powershell
[ValidateSet("Auth.Api", "Device.Api", "Monitor.Api", "Frontend")]
```

在 switch 中添加分支：

```powershell
"Monitor.Api" {
    Write-Host "📊 启动 Monitor.Api..." -ForegroundColor Green
    Start-Process pwsh -ArgumentList @(
        "-NoExit",
        "-Command",
        "Set-Location '$baseDir\Monitor.Api'; `$host.ui.RawUI.WindowTitle='DevDCP - Monitor.Api'; dotnet run"
    ) -WindowStyle Normal
    Write-Host "✅ Monitor.Api 启动中 (http://localhost:60625)" -ForegroundColor Green
}
```

### 3. 修改 `check-services.ps1`

添加端口检查：

```powershell
$monitorRunning = Test-Port -Port 60625 -ServiceName "Monitor.Api"
```

---

## 🐛 常见问题

### Q1: PowerShell 脚本显示乱码或无法执行

**问题现象：**
```
所在位置 D:\...\start-all-services.ps1:40 字符: 53
表达式或语句中包含意外的标记"娆￠噸璇曞悗浠嶆湭灏辩华"。
```

**原因：** 中文字符编码问题（Windows PowerShell 5.1）

**解决方法：**
```powershell
# 方法 1（推荐）：所有脚本已改为英文提示，重新下载或拉取最新代码

# 方法 2：使用 PowerShell 7+
pwsh.exe  # 在新的 PowerShell 7 中运行

# 方法 3：检查文件编码
# 在 VS Code 中：右下角显示编码 → 选择 "UTF-8 with BOM"
```

### Q2: 提示 "无法加载文件，因为在此系统上禁止运行脚本"

**解决方法：**
```powershell
# 以管理员身份运行 PowerShell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Q3: Docker 服务未运行怎么办？

**错误提示：**
```
❌ Docker Desktop 未运行
❌ PostgreSQL 容器未运行
```

**解决方法：**
```powershell
# 1. 启动 Docker Desktop（任务栏图标）

# 2. 等待 Docker Desktop 完全启动（图标不转圈）

# 3. 启动 PostgreSQL 容器
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code\infra
docker-compose -f docker-compose.dev.yml up -d postgres

# 4. 检查状态
cd ..
.\check-docker-services.ps1
```

### Q4: 端口被占用怎么办？

**检查端口占用：**
```powershell
# 检查 60621 端口（Auth.Api）
netstat -ano | findstr :60621

# 检查 5432 端口（PostgreSQL）
netstat -ano | findstr :5432

# 杀死占用进程（PID 是上一步最后一列的数字）
taskkill /PID <PID> /F
```

**或者停止所有服务：**
```powershell
.\stop-all-services.ps1

# 停止 Docker 容器
cd infra
docker-compose -f docker-compose.dev.yml down
```

### Q5: Auth.Api 未就绪，Device.Api 启动失败

**错误提示：**
```
⚠️  Auth.Api 未运行或未就绪
💡 Device.Api 依赖 Auth.Api 的 JWT 验证
```

**解决方法 1（推荐）：**
```powershell
# 使用 start-all-services.ps1 自动等待
.\start-all-services.ps1
```

**解决方法 2：**
```powershell
# 先启动 Auth.Api，等待几秒
.\start-service.ps1 Auth.Api
Start-Sleep -Seconds 10

# 再启动 Device.Api
.\start-service.ps1 Device.Api
```

### Q6: 数据库连接失败

**错误日志：**
```
Npgsql.NpgsqlException: 无法连接到服务器
```

**解决方法：**
```powershell
# 1. 检查 PostgreSQL 容器
docker ps | findstr devdcp-postgres

# 2. 查看容器日志
docker logs devdcp-postgres

# 3. 重启容器
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code\infra
docker-compose -f docker-compose.dev.yml restart postgres

# 4. 测试数据库连接
docker exec -it devdcp-postgres psql -U devdcp -d devdcp
# 输入密码: devdcp
# 测试查询: SELECT version();
# 退出: \q
```

### Q7: 如何重启单个服务？

**方法 1：** 在服务窗口按 `Ctrl+C`，然后输入：
```powershell
dotnet run
```

**方法 2：** 使用脚本：
```powershell
# 找到对应进程并关闭窗口，然后重新启动
.\start-service.ps1 Device.Api
```

### Q8: 前端启动失败

**检查 pnpm 是否安装：**
```powershell
pnpm --version
```

**安装依赖：**
```powershell
cd d:\00_QC-share\01_DevDCP\02_Work\02_Code\web\frontend
pnpm install
```

### Q9: Device.Api 配置的数据库连接字符串错误

**问题：** Device.Api 使用了独立数据库 `device_db`，导致找不到认证相关的表。

**解决方法：**
已在 `appsettings.json` 中修复，确保使用共享数据库：
```json
{
  "ConnectionStrings": {
    "DeviceDatabase": "Host=localhost;Port=5432;Database=devdcp;Username=devdcp;Password=devdcp"
  }
}
```

---

## 💡 高级技巧

### 1. 设置别名（更快启动）

在 PowerShell 配置文件中添加别名：

```powershell
# 打开 PowerShell 配置文件
notepad $PROFILE

# 添加以下内容
$DevDCPRoot = "d:\00_QC-share\01_DevDCP\02_Work\02_Code"
Set-Alias -Name start-dev -Value "$DevDCPRoot\start-all-services.ps1"
Set-Alias -Name stop-dev -Value "$DevDCPRoot\stop-all-services.ps1"
Set-Alias -Name check-dev -Value "$DevDCPRoot\check-services.ps1"
Set-Alias -Name check-docker -Value "$DevDCPRoot\check-docker-services.ps1"

# Docker 快捷命令
function Start-DevDocker {
    Set-Location "$DevDCPRoot\infra"
    docker-compose -f docker-compose.dev.yml up -d postgres
}
Set-Alias -Name start-docker -Value Start-DevDocker
```

**使用：**
```powershell
start-docker    # 启动 Docker 服务
check-docker    # 检查 Docker 状态
start-dev       # 启动所有微服务
check-dev       # 检查微服务状态
stop-dev        # 停止所有微服务
```

### 2. 自动启动（开机启动）

创建 Windows 任务计划程序：

1. Win+R 输入 `taskschd.msc`
2. 创建基本任务
3. 触发器：登录时
4. 操作：启动程序
5. 程序/脚本：`powershell.exe`
6. 参数：`-File "d:\00_QC-share\01_DevDCP\02_Work\02_Code\start-all-services.ps1"`

**注意：** 需要确保 Docker Desktop 也设置为开机自启。

### 3. 查看实时日志

在服务窗口中：
```powershell
# 启动服务时添加详细日志
dotnet run --verbosity detailed

# 查看数据库查询日志
# 修改 appsettings.Development.json:
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

### 4. Docker 数据持久化

**查看 Docker 数据卷：**
```powershell
docker volume ls | findstr devdcp
```

**备份 PostgreSQL 数据：**
```powershell
# 导出数据库
docker exec devdcp-postgres pg_dump -U devdcp devdcp > backup.sql

# 导入数据库
docker exec -i devdcp-postgres psql -U devdcp devdcp < backup.sql
```

### 5. 性能监控

**实时监控 Docker 容器资源：**
```powershell
docker stats devdcp-postgres devdcp-rabbitmq devdcp-influx devdcp-redis
```

**查看服务进程：**
```powershell
# 查看 dotnet 进程
Get-Process dotnet | Select-Object Id, ProcessName, CPU, WorkingSet

# 查看 node 进程
Get-Process node | Select-Object Id, ProcessName, CPU, WorkingSet
```

---

## 📊 架构说明

### 微服务架构
```
┌─────────────────────────────────────────────────────────┐
│                    Frontend (Vue 3)                     │
│                  http://localhost:3200                  │
└──────────────────────┬──────────────────────────────────┘
                       │
         ┌─────────────┴─────────────┐
         │                           │
         ▼                           ▼
┌────────────────┐          ┌────────────────┐
│   Auth.Api     │          │  Device.Api    │
│  port: 60621   │◄─────────│  port: 60623   │
│  (JWT 签发)    │  依赖JWT  │  (设备管理)    │
└────────┬───────┘          └────────┬───────┘
         │                           │
         └─────────────┬─────────────┘
                       │
         ┌─────────────┴─────────────┐
         │                           │
         ▼                           ▼
┌────────────────┐          ┌────────────────┐
│  PostgreSQL    │          │   RabbitMQ     │
│  (共享数据库)  │          │  (消息队列)    │
│  port: 5432    │          │  port: 5672    │
└────────────────┘          └────────────────┘
```

### 数据库架构
- **共享数据库模式**：Auth.Api 和 Device.Api 共享 `devdcp` 数据库
- **统一实体管理**：所有实体类和 DbContext 在 `SharedAuth.Library` 项目中
- **数据库迁移**：在 `SharedAuth.Library` 中统一管理 EF Core Migrations
- **审计字段**：所有表包含 `created_by`, `created_at`, `updated_by`, `updated_at`, `deleted_flag`

### 认证授权流程
1. **登录**：前端 → Auth.Api `/auth/login` → 返回 JWT Token
2. **访问 Device.Api**：
   - 前端携带 JWT Token → Device.Api
   - Device.Api 通过 Auth.Api 的公钥验证 JWT（本地验证，不调用接口）
   - 验证成功 → 返回数据
3. **跨服务按钮权限**：Device.Api 通过 `SharedAuth.Library` 的扩展方法直接查询数据库验证权限

---

## 📦 项目结构

```
02_Code/
├── platform/center/
│   ├── Auth.Api/                 # 认证授权服务（端口 60621）
│   ├── Device.Api/               # 设备管理服务（端口 60623）
│   └── SharedAuth.Library/       # 共享库（实体、DbContext、迁移）
├── web/frontend/                 # 前端项目（端口 3200）
├── infra/
│   └── docker-compose.dev.yml   # Docker 基础设施配置
├── start-all-services.ps1        # 启动所有服务
├── stop-all-services.ps1         # 停止所有服务
├── start-service.ps1             # 启动单个服务
├── check-services.ps1            # 检查微服务状态
└── check-docker-services.ps1     # 检查 Docker 服务状态
```

---

## � 相关文档

- [项目立项说明](../../01_Doc/01_PJDesign/01_项目立项说明.md)
- [产品需求文档 (PRD)](../../01_Doc/01_PJDesign/02_PRD_工业数据采集系统产品需求文档.md)
- [功能规格说明 (FSD)](../../01_Doc/01_PJDesign/03_FSD_工业数据采集系统功能规格说明文档.md)
- [架构设计说明 (SSA)](../../01_Doc/01_PJDesign/04_SSA_工业数据采集系统架构设计说明.md)
- [详细设计 (LLD)](../../01_Doc/01_PJDesign/05_LLD_工业数据采集系统详细设计.md)

---

**享受开发！** 🚀
