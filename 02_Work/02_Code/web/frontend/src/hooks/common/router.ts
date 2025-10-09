import { useRouter } from 'vue-router';
import type { RouteLocationRaw } from 'vue-router';
import type { RouteKey } from '@elegant-router/types';
import { router as globalRouter } from '@/router';

/**
 * 路由跳转Hook - 提供路由导航功能，可以替代router.push函数
 *
 * 跳转到指定路由，提供更便捷的路由操作方法
 *
 * @param inSetup 是否在Vue script setup中使用 - 决定使用哪个router实例
 */
export function useRouterPush(inSetup = true) {
  // 路由器实例 - 根据setup环境选择合适的router实例
  const router = inSetup ? useRouter() : globalRouter;
  // 当前路由信息 - 获取当前路由状态
  const route = globalRouter.currentRoute;

  // 路由跳转方法 - 基础的路由导航函数
  const routerPush = router.push;

  // 路由返回方法 - 浏览器后退功能
  const routerBack = router.back;

  // 路由跳转选项接口 - 定义路由跳转时可传递的参数
  interface RouterPushOptions {
    query?: Record<string, string>; // 查询参数 - URL查询字符串参数
    params?: Record<string, string>; // 路由参数 - 动态路由参数
  }

  /**
   * 根据路由键跳转 - 使用路由名称进行页面跳转
   * @param key 路由键 - 目标路由的名称标识
   * @param options 跳转选项 - 可选的查询参数和路由参数
   */
  async function routerPushByKey(key: RouteKey, options?: RouterPushOptions) {
    // 解构跳转选项参数
    const { query, params } = options || {};

    // 构建路由位置对象 - 基于路由名称创建路由配置
    const routeLocation: RouteLocationRaw = {
      name: key
    };

    // 添加查询参数 - 如果存在查询参数则添加到路由配置
    if (Object.keys(query || {}).length) {
      routeLocation.query = query;
    }

    // 添加路由参数 - 如果存在路由参数则添加到路由配置
    if (Object.keys(params || {}).length) {
      routeLocation.params = params;
    }

    // 执行路由跳转
    return routerPush(routeLocation);
  }

  function routerPushByKeyWithMetaQuery(key: RouteKey) {
    const allRoutes = router.getRoutes();
    const meta = allRoutes.find(item => item.name === key)?.meta || null;

    const query: Record<string, string> = {};

    meta?.query?.forEach(item => {
      query[item.key] = item.value;
    });

    return routerPushByKey(key, { query });
  }

  async function toHome() {
    return routerPushByKey('root');
  }

  /**
   * Navigate to login page
   *
   * @param loginModule The login module
   * @param redirectUrl The redirect url, if not specified, it will be the current route fullPath
   */
  async function toLogin(loginModule?: UnionKey.LoginModule, redirectUrl?: string) {
    const module = loginModule || 'pwd-login';

    const options: RouterPushOptions = {
      params: {
        module
      }
    };

    const redirect = redirectUrl || route.value.fullPath;

    options.query = {
      redirect
    };

    return routerPushByKey('login', options);
  }

  /**
   * Toggle login module
   *
   * @param module
   */
  async function toggleLoginModule(module: UnionKey.LoginModule) {
    const query = route.value.query as Record<string, string>;

    return routerPushByKey('login', { query, params: { module } });
  }

  /**
   * Redirect from login
   *
   * @param [needRedirect=true] Whether to redirect after login. Default is `true`
   */
  async function redirectFromLogin(needRedirect = true) {
    const redirect = route.value.query?.redirect as string;

    if (needRedirect && redirect) {
      await routerPush(redirect);
    } else {
      await toHome();
    }
  }

  return {
    routerPush,
    routerBack,
    routerPushByKey,
    routerPushByKeyWithMetaQuery,
    toLogin,
    toggleLoginModule,
    redirectFromLogin
  };
}
