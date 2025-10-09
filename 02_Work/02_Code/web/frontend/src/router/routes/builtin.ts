import type { CustomRoute } from '@elegant-router/types';
import { layouts, views } from '../elegant/imports';
import { getRoutePath, transformElegantRoutesToVueRoutes } from '../elegant/transform';

/**
 * @zh
 * 定义应用的根路由配置项。
 *
 * - `name`: 路由名称，设为 'root'。
 * - `path`: 路由路径，设为 '/'。
 * - `redirect`: 重定向到由环境变量 `VITE_ROUTE_HOME` 指定的路径，若未指定则重定向到 '/home'。
 * - `meta`: 路由元数据，包括标题和常量标识。
 */
export const ROOT_ROUTE: CustomRoute = {
  name: 'root',
  path: '/',
  redirect: getRoutePath(import.meta.env.VITE_ROUTE_HOME) || '/home',
  meta: {
    title: 'root',
    constant: true
  }
};

/**
 * @zh
 * 表示用于处理 404 未找到错误的路由配置。
 * 此路由会匹配所有未被其他路由匹配的路径，并渲染 404 视图组件。
 *
 * @说明
 * - `path` 使用全匹配模式，匹配所有不存在的路由。
 * - `component` 设置为显示空白布局和 404 错误视图。
 * - `meta.constant` 属性表示该路由始终存在于路由器中。
 *
 * @参见 {@link CustomRoute}
 */
const NOT_FOUND_ROUTE: CustomRoute = {
  name: 'not-found',
  path: '/:pathMatch(.*)*',
  component: 'layout.blank$view.404',
  meta: {
    title: 'not-found',
    constant: true
  }
};

/**
 * 包含应用内置路由的数组。
 * 该数组通常包括根路由和未找到路由（404），用于基础路由配置。
 * @type {CustomRoute[]}
 */
const builtinRoutes: CustomRoute[] = [ROOT_ROUTE, NOT_FOUND_ROUTE];

/**
 * 创建内置的 Vue 路由配置。
 *
 * 此函数将内置路由（`builtinRoutes`）、布局（`layouts`）和视图（`views`）转换为 Vue 路由对象，
 * 并返回转换后的路由配置数组。
 *
 * @returns {Array<RouteRecordRaw>} 转换后的 Vue 路由配置数组。
 */
export function createBuiltinVueRoutes() {
  return transformElegantRoutesToVueRoutes(builtinRoutes, layouts, views);
}
