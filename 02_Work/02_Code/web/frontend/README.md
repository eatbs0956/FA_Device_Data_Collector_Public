<div align="center">
	<img src="./public/favicon.svg" width="160" />
	<h1>SoybeanAdmin ElementPlus</h1>
  <span>中文 | <a href="./README.en_US.md">English</a></span>
</div>

---

[![license](https://img.shields.io/badge/license-MIT-green.svg)](./LICENSE)

## 简介

[`SoybeanAdmin`](https://github.com/soybeanjs/soybean-admin) 是一个清新优雅、高颜值且功能强大的后台管理模板，基于最新的前端技术栈，包括 Vue3, Vite7, TypeScript, Pinia 和 UnoCSS。它内置了丰富的主题配置和组件，代码规范严谨，实现了自动化的文件路由系统。此外，它还采用了基于 ApiFox 的在线Mock数据方案。`SoybeanAdmin` 为您提供了一站式的后台管理解决方案，无需额外配置，开箱即用。同样是一个快速学习前沿技术的最佳实践。

## 特性

- **前沿技术应用**：采用 Vue3, Vite7, TypeScript, Pinia 和 UnoCSS 等最新流行的技术栈。
- **清晰的项目架构**：采用 pnpm monorepo 架构，结构清晰，优雅易懂。
- **严格的代码规范**：遵循 [SoybeanJS 规范](https://docs.soybeanjs.cn/zh/standard)，集成了eslint, prettier 和 simple-git-hooks，保证代码的规范性。
- **TypeScript**： 支持严格的类型检查，提高代码的可维护性。
- **丰富的主题配置**：内置多样的主题配置，与 UnoCSS 完美结合。
- **内置国际化方案**：轻松实现多语言支持。
- **自动化文件路由系统**：自动生成路由导入、声明和类型。更多细节请查看 [Elegant Router](https://github.com/soybeanjs/elegant-router)。
- **灵活的权限路由**：同时支持前端静态路由和后端动态路由。
- **丰富的页面组件**：内置多样页面和组件，包括403、404、500页面，以及布局组件、标签组件、主题配置组件等。
- **命令行工具**：内置高效的命令行工具，git提交、删除文件、发布等。
- **移动端适配**：完美支持移动端，实现自适应布局。


## 版本

- **NaiveUI 版本:**
  - [预览地址](https://naive.soybeanjs.cn/)
  - [Github 仓库](https://github.com/soybeanjs/soybean-admin)
  - [Gitee 仓库](https://gitee.com/honghuangdc/soybean-admin)
  - [Gitcode 仓库](https://gitcode.com/soybeanjs/soybean-admin)
- **AntDesignVue 版本:**
  - [预览地址](https://antd.soybeanjs.cn/)
  - [Github 仓库](https://github.com/soybeanjs/soybean-admin-antd)
  - [Gitee 仓库](https://gitee.com/honghuangdc/soybean-admin-antd)
  - [Gitcode 仓库](https://gitcode.com/soybeanjs/soybean-admin-antd)
- **ElementPlus 版本:**
  - [预览地址](https://elp.soybeanjs.cn/)
  - [Github 仓库](https://github.com/soybeanjs/soybean-admin-element-plus)
  - [Gitee 仓库](https://gitee.com/honghuangdc/soybean-admin-element-plus)
  - [Gitcode 仓库](https://gitcode.com/soybeanjs/soybean-admin-element-plus)
- **旧版:**
  - [预览地址](https://legacy.soybeanjs.cn/)
  - [Github 仓库](https://github.com/soybeanjs/soybean-admin/tree/legacy)
  - [Gitee 仓库](https://gitee.com/honghuangdc/soybean-admin/tree/legacy)
  - [Gitcode 仓库](https://gitcode.com/soybeanjs/soybean-admin/tree/legacy)


## 文档

- [地址](https://docs.soybeanjs.cn)
- [旧版文档](https://legacy-docs.soybeanjs.cn)


## 使用

**环境准备**

确保你的环境满足以下要求：

- **git**: 你需要git来克隆和管理项目版本。
- **NodeJS**: >=20.19.0，推荐 20.19.0 或更高。
- **pnpm**: >= 10.5.0，推荐 10.5.0 或更高。

**克隆项目**

```bash
# github
git clone https://github.com/soybeanjs/soybean-admin.git
# gitee
git clone https://gitee.com/honghuangdc/soybean-admin.git
# gitcode
git clone https://gitcode.com/soybeanjs/soybean-admin.git
```

**安装依赖**

```bash
pnpm i
```
> 由于本项目采用了 pnpm monorepo 的管理方式，因此请不要使用 npm 或 yarn 来安装依赖。

**启动项目**

```bash
pnpm dev
```

**构建项目**

```bash
pnpm build
```

## 周边生态

- [react-soybean-admin](https://github.com/mufeng889/react-soybean-admin): 基于SoybeanAdmin的React版本.
- [electron-mock-admin](https://github.com/lixin59/electron-mock-api): 一个 Mock Api 管理系统，帮助前端开发伙伴快速实现接口的 mock。
- [T-Shell](https://github.com/TheBlindM/T-Shell): 是一个可配置命令提示的终端模拟器和 SSH 客户端。
- [pea](https://github.com/haitang1894/pea) : 采用SpringBoot3.2 + JDK21、MyBatis-Plus、SpringSecurity安全框架等，适配 [soybean-admin](https://gitee.com/honghuangdc/soybean-admin) 开发的简单权限系统。
- [MalusAdmin](https://github.com/pridejoy/MalusAdmin): 基于 Vue3/TypeScript/NaiveUI 和 NET7 & Sqlsugar 开发的后台管理框架。采用最原生最简洁的方式来实现, 前端清新优雅高颜值，后端 结构清晰，优雅易懂，功能强大。
- [PanisAdmin](https://github.com/paynezhuang/panis-admin): 采用SpringBoot3、SaToken、MySQL等框架开发，二次修改 [soybean-admin](https://github.com/soybeanjs/soybean-admin)，适配动态菜单/按钮级别的鉴权，保留原汁原味、清新优雅、高颜值的后台管理系统脚手架。
- [snail-job](https://github.com/aizuda/snail-job): 一款兼具 “高性能、高颜值、高活跃” 的分布式任务重试和分布式任务调度平台。
- [SuperApi](https://github.com/TmmTop/SuperApi): 快速将你的 idea 变成线上稳定运行的产品！ 无实体建库建表，对无实体库表进行增删改查，支持 15 种条件查询，以及分页，列表，无限级树形列表 等功能的 API 部署！ 拥有接口文档，Auth 授权，接口限流，获取客户端真实 IP，先进的服务器缓存组件，动态 API 等功能，期待您的体验！
- [FastSoyAdmin](https://github.com/sleep1223/fast-soy-admin): 基于 FastAPI+Vue3+Naive UI 的现代化轻量管理平台.
- [ba](https://github.com/xiatianYa/Ba-Server): 基于goFrame框架开发的后端服务对接soybean-admin,适配动态路由,接口鉴权限。
- [soybean-admin-go](https://github.com/WgoW/soybean-admin-go):基于gin+gorm框架开发的go语言后端服务对接soybean-admin的example分支,适配动态路由,接口鉴权限。


## 开源协议

项目基于 [MIT © 2021 Soybean](./LICENSE) 协议，仅供学习参考，商业使用请保留作者版权信息，作者不保证也不承担任何软件的使用风险。
