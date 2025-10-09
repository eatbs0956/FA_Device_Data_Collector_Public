/**
 * 创建静态路由（constantRoutes 和 authRoutes）。
 *
 * - constantRoutes: 包含 meta.constant 为 true 的路由，通常为无需权限的常量路由。
 * - authRoutes: 其他需要权限验证的路由。
 *
 * 合并自定义路由（customRoutes）与自动生成路由（generatedRoutes），并根据 meta.constant 字段进行分类。
 *
 * @returns 包含 constantRoutes 和 authRoutes 的对象
 */

import type { CustomRoute, ElegantConstRoute, ElegantRoute } from '@elegant-router/types';
import { generatedRoutes } from '../elegant/routes';
import { layouts, views } from '../elegant/imports';
import { transformElegantRoutesToVueRoutes } from '../elegant/transform';

const customRoutes: CustomRoute[] = [
  {
    name: 'exception',
    path: '/exception',
    component: 'layout.base',
    meta: {
      title: 'exception',
      i18nKey: 'route.exception',
      icon: 'ant-design:exception-outlined',
      order: 7,
      hideInMenu: true // 隐藏异常页菜单 - 不在左侧导航中显示异常页相关菜单项
    },
    children: [
      {
        name: 'exception_403',
        path: '/exception/403',
        component: 'view.403',
        meta: {
          title: 'exception_403',
          i18nKey: 'route.exception_403',
          icon: 'ic:baseline-block'
        }
      },
      {
        name: 'exception_404',
        path: '/exception/404',
        component: 'view.404',
        meta: {
          title: 'exception_404',
          i18nKey: 'route.exception_404',
          icon: 'ic:baseline-web-asset-off'
        }
      },
      {
        name: 'exception_500',
        path: '/exception/500',
        component: 'view.500',
        meta: {
          title: 'exception_500',
          i18nKey: 'route.exception_500',
          icon: 'ic:baseline-wifi-off'
        }
      }
    ]
  }
];

/**
 * 创建静态路由和鉴权路由的集合。
 *
 * 此函数遍历 `customRoutes` 和 `generatedRoutes`，根据路由的 `meta.constant` 属性，
 * 将路由分别归类到 `constantRoutes`（静态路由）和 `authRoutes`（需要鉴权的路由）数组中。
 *
 * @returns 包含 `constantRoutes`（静态路由数组）和 `authRoutes`（鉴权路由数组）的对象。
 */
export function createStaticRoutes() {
  const constantRoutes: ElegantRoute[] = [];

  const authRoutes: ElegantRoute[] = [];

  [...customRoutes, ...generatedRoutes].forEach(item => {
    if (item.meta?.constant) {
      constantRoutes.push(item);
    } else {
      authRoutes.push(item);
    }
  });

  return {
    constantRoutes,
    authRoutes
  };
}

/**
 * 根据传入的 ElegantConstRoute 路由数组，结合预定义的布局和视图，
 * 转换为 Vue 路由对象数组。用于生成带有鉴权的 Vue 路由配置。
 *
 * @param routes ElegantConstRoute 类型的路由配置数组
 * @returns 转换后的 Vue 路由对象数组
 */
export function getAuthVueRoutes(routes: ElegantConstRoute[]) {
  return transformElegantRoutesToVueRoutes(routes, layouts, views);
}
