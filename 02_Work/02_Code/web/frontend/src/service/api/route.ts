import { request } from '../request';

/**
 * 获取常量路由列表。
 *
 * 该函数通过发送 GET 请求到 `/route/getConstantRoutes` 接口，
 * 返回包含菜单路由信息的数组。
 *
 * @returns 包含菜单路由信息的 Promise 对象。
 */
export function fetchGetConstantRoutes() {
  return request<Api.Route.MenuRoute[]>({ url: '/route/getConstantRoutes' });
}

/**
 * 获取当前用户的路由信息。
 *
 * 该函数会向后端发送请求，获取当前登录用户的路由权限数据。
 *
 * @returns 包含用户路由信息的 Promise 对象。
 */
export function fetchGetUserRoutes() {
  return request<Api.Route.UserRoute>({ url: '/route/getUserRoutes' });
}

/**
 * 检查指定的路由名称是否存在。
 *
 * @param routeName 路由名称字符串。
 * @returns 一个 Promise，解析为布尔值，表示路由是否存在。
 */
export function fetchIsRouteExist(routeName: string) {
  return request<boolean>({ url: '/route/isRouteExist', params: { routeName } });
}
