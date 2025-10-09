import type { Router } from 'vue-router';
import { createRouteGuard } from './route';
import { createProgressGuard } from './progress';
import { createDocumentTitleGuard } from './title';

/**
 * 创建并注册路由守卫。
 *
 * 此函数会为传入的 `router` 实例依次注册进度守卫、路由守卫和文档标题守卫，
 * 用于增强路由导航过程中的用户体验和安全性。
 *
 * @param router - 需要注册守卫的 Vue Router 实例
 */
export function createRouterGuard(router: Router) {
  createProgressGuard(router);
  createRouteGuard(router);
  createDocumentTitleGuard(router);
}
