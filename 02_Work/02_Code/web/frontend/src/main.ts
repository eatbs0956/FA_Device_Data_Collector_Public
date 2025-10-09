import { createApp } from 'vue';
import './plugins/assets';
import {
  setupAppVersionNotification,
  setupDayjs,
  setupIconifyOffline,
  setupLoading,
  setupNProgress,
  setupUI
} from './plugins';
import { setupStore } from './store';
import { setupRouter } from './router';
import { setupI18n } from './locales';
import App from './App.vue';

/**
 * 应用程序主入口函数 - 初始化并配置主应用程序
 *
 * 此函数按照依赖顺序设置了应用范围的各种功能和插件，包括：
 * - 加载指示器（品牌化启动界面）
 * - NProgress 进度条（页面切换视觉反馈）
 * - Iconify 离线图标（支持内网环境）
 * - Day.js 日期处理（统一时间格式化）
 * - UI 组件库（Element Plus 全局注册）
 * - 状态管理（Pinia 全局状态）
 * - 路由系统（异步加载，包含权限守卫）
 * - 国际化（多语言支持）
 * - 应用版本通知（生产环境自动更新检测）
 */
async function setupApp() {
  // 1. 设置应用加载界面 - 显示品牌 Logo 和动画，提升用户体验
  setupLoading();

  // 2. 配置页面顶部进度条 - 为路由跳转等异步操作提供视觉反馈
  setupNProgress();

  // 3. 初始化离线图标系统 - 支持自定义图标服务器，适用于内网环境
  setupIconifyOffline();

  // 4. 配置日期时间处理库 - 统一应用的日期格式化和国际化
  setupDayjs();

  // 5. 创建 Vue 应用实例 - 基于根组件创建应用
  const app = createApp(App);

  // 6. 注册 UI 组件库 - Element Plus 等组件的全局注册
  setupUI(app);

  // 7. 配置状态管理系统 - Pinia 全局状态管理，支持重置功能
  setupStore(app);

  // 8. 设置路由系统 - 等待路由准备就绪，包含权限守卫和动态路由
  await setupRouter(app);

  // 9. 配置国际化系统 - Vue I18n 多语言支持
  setupI18n(app);

  // 10. 启动版本更新检测 - 生产环境自动检查应用更新并通知用户
  setupAppVersionNotification();

  // 11. 挂载应用到 DOM - 最后一步，确保所有配置完成后再渲染
  app.mount('#app');
}

// 启动应用程序 - 调用主初始化函数
setupApp();
