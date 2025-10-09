import { alova } from '../request';

/**
 * 从后端 API 获取常量菜单路由列表。
 *
 * @returns 返回一个包含 `MenuRoute` 对象数组的 Promise。
 *
 * @remarks
 * 此函数会向 `/route/getConstantRoutes` 端点发送 GET 请求。
 * 返回的路由通常用于静态或常量导航菜单。
 */
export function fetchGetConstantRoutes() {
  return alova.Get<Api.Route.MenuRoute[]>('/route/getConstantRoutes');
}

/**
 * 获取当前用户的路由信息。
 *
 * 该方法通过 GET 请求 `/route/getUserRoutes` 接口，返回用户的路由数据。
 *
 * @returns 返回一个 Promise，解析为 `Api.Route.UserRoute` 类型的用户路由信息。
 */
export function fetchGetUserRoutes() {
  return alova.Get<Api.Route.UserRoute>('/route/getUserRoutes');
}

/**
 * 检查指定的路由名称是否存在。
 *
 * @param routeName 路由名称字符串
 * @returns 返回一个 Promise，解析为布尔值，表示路由是否存在
 */
export function fetchIsRouteExist(routeName: string) {
  return alova.Get<boolean>('/route/isRouteExist', { params: { routeName } });
}
