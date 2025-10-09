import { computed, reactive, ref } from 'vue';
import { useRoute } from 'vue-router';
import { defineStore } from 'pinia';
import { useLoading } from '@sa/hooks';
import { fetchGetUserInfo, fetchLogin } from '@/service/api';
import { useRouterPush } from '@/hooks/common/router';
import { localStg } from '@/utils/storage';
import { SetupStoreId } from '@/enum';
import { $t } from '@/locales';
import { useRouteStore } from '../route';
import { useTabStore } from '../tab';
import { clearAuthStorage, getToken } from './shared';

/**
 * 用户认证状态管理的 Pinia Store。
 *
 * 该 Store 提供了用户登录、登出、用户信息获取、权限判断等功能。
 *
 * - `token`: 当前用户的认证令牌。
 * - `userInfo`: 当前用户信息，包括用户ID、用户名、角色、按钮权限等。
 * - `isStaticSuper`: 是否为静态路由模式下的超级角色。
 * - `isLogin`: 当前是否已登录。
 * - `loginLoading`: 登录加载状态。
 * - `resetStore()`: 重置认证状态，清除本地存储并跳转到登录页。
 * - `login(userName, password, redirect?)`: 用户登录方法，支持登录后重定向。
 * - `initUserInfo()`: 初始化用户信息，校验令牌有效性。
 *
 * @remarks
 * 适用于需要用户认证和权限控制的前端应用场景。
 */
export const useAuthStore = defineStore(SetupStoreId.Auth, () => {
  // 当前路由信息 - 获取路由元信息和路径
  const route = useRoute();
  // 认证存储实例 - 用于调用认证相关方法
  const authStore = useAuthStore();
  // 路由存储实例 - 管理动态路由和权限路由
  const routeStore = useRouteStore();
  // 标签页存储实例 - 管理多标签页状态
  const tabStore = useTabStore();
  // 路由跳转工具 - 提供登录跳转和重定向功能
  const { toLogin, redirectFromLogin } = useRouterPush(false);
  // 加载状态管理 - 控制登录过程中的加载状态
  const { loading: loginLoading, startLoading, endLoading } = useLoading();

  // 认证令牌 - 用户登录后的访问令牌
  const token = ref(getToken());

  // 用户信息 - 当前登录用户的基础信息和权限
  const userInfo: Api.Auth.UserInfo = reactive({
    userId: '', // 用户ID - 唯一标识符
    userName: '', // 用户名 - 登录用户名
    roles: [], // 角色列表 - 用户拥有的角色权限
    buttons: [] // 按钮权限 - 用户可操作的按钮权限列表
  });

  /** 静态路由超级角色判断 - 检查当前用户是否为静态路由模式下的超级管理员 */
  const isStaticSuper = computed(() => {
    // 环境变量配置 - 获取认证路由模式和超级角色标识
    const { VITE_AUTH_ROUTE_MODE, VITE_STATIC_SUPER_ROLE } = import.meta.env;

    // 返回是否为静态路由模式下的超级角色
    return VITE_AUTH_ROUTE_MODE === 'static' && userInfo.roles.includes(VITE_STATIC_SUPER_ROLE);
  });

  /** 登录状态判断 - 检查用户是否已登录（基于token存在性） */
  const isLogin = computed(() => Boolean(token.value));

  /** 重置认证存储 - 清除用户认证状态并跳转到登录页 */
  async function resetStore() {
    // 记录当前用户ID - 用于下次登录时的用户切换检测
    recordUserId();

    // 清除认证相关本地存储 - 删除token和用户信息
    clearAuthStorage();

    // 重置Pinia存储状态 - 恢复到初始状态
    authStore.$reset();

    // 非常量路由时跳转登录页 - 避免在静态页面时重定向
    if (!route.meta.constant) {
      await toLogin();
    }

    // 缓存标签页状态 - 保存当前打开的标签页
    tabStore.cacheTabs();
    // 重置路由存储 - 清除动态路由配置
    routeStore.resetStore();
  }

  /** 记录用户ID - 保存当前登录用户ID，用于下次登录时的用户切换检测 */
  function recordUserId() {
    // 用户ID不存在时直接返回
    if (!userInfo.userId) {
      return;
    }

    // 本地存储当前用户ID - 用于下次登录时比较是否为同一用户
    localStg.set('lastLoginUserId', userInfo.userId);
  }

  /**
   * 检查标签页清除需求 - 判断当前登录用户是否与上次登录用户不同，如不同则清除所有标签页
   *
   * @returns {boolean} 是否需要清除所有标签页
   */
  function checkTabClear(): boolean {
    // 用户ID不存在时返回false
    if (!userInfo.userId) {
      return false;
    }

    // 获取上次登录用户ID - 从本地存储中读取
    const lastLoginUserId = localStg.get('lastLoginUserId');

    // 用户切换检测 - 如果当前用户与上次登录用户不同则清除标签页
    if (lastLoginUserId !== userInfo.userId) {
      // 清除全局标签页存储
      localStg.remove('globalTabs');
      // 清除标签页状态
      tabStore.clearTabs();

      return true;
    }

    return false;
  }

  /**
   * 用户登录 - 执行用户登录流程，包括认证、获取用户信息和重定向
   *
   * @param userName 用户名 - 登录用户名
   * @param password 密码 - 登录密码
   * @param [redirect=true] 是否重定向 - 登录成功后是否进行页面重定向，默认为true
   */
  async function login(userName: string, password: string, redirect = true) {
    // 开始加载状态 - 显示登录加载动画
    startLoading();

    // 调用登录API - 发送登录请求获取认证令牌
    const { data: loginToken, error } = await fetchLogin(userName, password);

    if (!error) {
      // 通过令牌登录 - 使用返回的令牌进行后续认证流程
      const pass = await loginByToken(loginToken);

      if (pass) {
        // 检查标签页清除需求 - 判断是否需要清除用户切换产生的旧标签页
        const isClear = checkTabClear();
        // 重定向控制变量 - 基于标签页清除状态调整重定向行为
        let needRedirect = redirect;

        if (isClear) {
          // 标签页已清除时不需要重定向 - 避免重复跳转
          needRedirect = false;
        }
        // 登录后重定向 - 根据需要跳转到目标页面
        await redirectFromLogin(needRedirect);

        // 登录成功通知 - 显示欢迎消息
        window.$notification?.success({
          title: $t('page.login.common.loginSuccess'),
          message: $t('page.login.common.welcomeBack', { userName: userInfo.userName }),
          duration: 4500
        });
      }
    } else {
      // 登录失败处理 - 重置认证状态
      resetStore();
    }

    // 结束加载状态 - 隐藏登录加载动画
    endLoading();
  }

  /**
   * 通过令牌登录 - 使用登录令牌完成认证流程
   * @param loginToken 登录令牌 - 包含访问令牌和刷新令牌的对象
   * @returns Promise<boolean> 登录是否成功
   */
  async function loginByToken(loginToken: Api.Auth.LoginToken) {
    // 1. 存储令牌到本地 - 后续请求需要在请求头中携带
    localStg.set('token', loginToken.token);
    localStg.set('refreshToken', loginToken.refreshToken);

    // 2. 获取用户信息 - 使用令牌获取用户详细信息
    const pass = await getUserInfo();

    if (pass) {
      // 更新响应式令牌状态 - 触发登录状态变更
      token.value = loginToken.token;

      return true;
    }

    return false;
  }

  /**
   * 获取用户信息 - 从服务器获取当前用户的详细信息
   * @returns Promise<boolean> 获取是否成功
   */
  async function getUserInfo() {
    // 调用用户信息API - 获取用户基本信息和权限
    const { data: info, error } = await fetchGetUserInfo();

    if (!error) {
      // 更新用户信息存储 - 将服务器返回的信息同步到本地状态
      Object.assign(userInfo, info);

      return true;
    }

    return false;
  }

  /**
   * 初始化用户信息 - 应用启动时根据本地令牌状态初始化用户信息
   */
  async function initUserInfo() {
    // 检查本地令牌 - 判断是否存在有效令牌
    const hasToken = getToken();

    if (hasToken) {
      // 尝试获取用户信息 - 验证令牌有效性
      const pass = await getUserInfo();

      if (!pass) {
        // 令牌无效时重置状态 - 清除无效认证信息
        resetStore();
      }
    }
  }

  return {
    token,
    userInfo,
    isStaticSuper,
    isLogin,
    loginLoading,
    resetStore,
    login,
    initUserInfo
  };
});
