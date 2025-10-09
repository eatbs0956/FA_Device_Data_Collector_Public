/**
 * 设置并初始化 Vue Router。
 *
 * 此函数将 Vue Router 实例注册到传入的 Vue 应用，并应用路由守卫。
 * 在路由器准备好后返回，确保路由器已完全初始化。
 *
 * @param app - Vue 应用实例
 * @returns Promise<void> 路由器准备完成后的 Promise
 */
import type { App } from 'vue';
import {
  type RouterHistory,
  createMemoryHistory,
  createRouter,
  createWebHashHistory,
  createWebHistory
} from 'vue-router';
import { createBuiltinVueRoutes } from './routes/builtin';
import { createRouterGuard } from './guard';

const { VITE_ROUTER_HISTORY_MODE = 'history', VITE_BASE_URL } = import.meta.env;

/**
 * 一个根据不同路由历史模式（hash、history、memory）返回对应路由历史创建函数的映射表。
 *
 * - `hash`: 使用 `createWebHashHistory` 创建基于 hash 的路由历史。
 * - `history`: 使用 `createWebHistory` 创建基于 HTML5 history 的路由历史。
 * - `memory`: 使用 `createMemoryHistory` 创建内存中的路由历史，常用于测试或非浏览器环境。
 *
 * @example
 * const history = historyCreatorMap['hash']('/base-path');
 *
 * @param base 可选的基础路径。
 * @returns 对应的 RouterHistory 实例。
 */
const historyCreatorMap: Record<Env.RouterHistoryMode, (base?: string) => RouterHistory> = {
  hash: createWebHashHistory,
  history: createWebHistory,
  memory: createMemoryHistory
};

/**
 * 创建并导出 Vue 路由实例。
 *
 * @remarks
 * 此路由实例使用指定的历史模式和基础路径，并通过 `createBuiltinVueRoutes` 生成路由配置。
 *
 * @example
 * ```typescript
 * import { router } from './router';
 * app.use(router);
 * ```
 *
 * @see {@link https://router.vuejs.org/}
 */
export const router = createRouter({
  history: historyCreatorMap[VITE_ROUTER_HISTORY_MODE](VITE_BASE_URL),
  routes: createBuiltinVueRoutes()
});

/**
 * 初始化并配置应用的路由。
 *
 * 此函数将路由实例注册到应用，并设置路由守卫，最后等待路由准备完成。
 *
 * @param app - Vue 应用实例。
 * @returns 一个 Promise，当路由准备完成时解析。
 */
export async function setupRouter(app: App) {
  app.use(router);
  createRouterGuard(router);
  await router.isReady();
}
