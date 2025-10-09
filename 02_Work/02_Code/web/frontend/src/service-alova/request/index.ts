import { createAlovaRequest } from '@sa/alova';
import { createAlovaMockAdapter } from '@sa/alova/mock';
import adapterFetch from '@sa/alova/fetch';
import { useAuthStore } from '@/store/modules/auth';
import { getServiceBaseURL } from '@/utils/service';
import { $t } from '@/locales';
import featureUsers20241014 from '../mocks/feature-users-20241014';
import { getAuthorization, handleRefreshToken, showErrorMsg } from './shared';
import type { RequestInstanceState } from './type';

const isHttpProxy = import.meta.env.DEV && import.meta.env.VITE_HTTP_PROXY === 'Y';
const { baseURL } = getServiceBaseURL(import.meta.env, isHttpProxy);

const state: RequestInstanceState = {
  errMsgStack: []
};
/**
 * 创建一个用于模拟请求的 Alova 适配器。
 *
 * @remarks
 * 该适配器通过 `createAlovaMockAdapter` 创建，支持全局启用 mock 功能，并可设置响应延迟时间。
 * 当请求未命中 mock 数据时，将使用 `adapterFetch` 作为后备适配器进行真实请求。
 * `matchMode` 设置为 `'methodurl'`，表示通过请求方法和 URL 进行 mock 匹配。
 *
 * @example
 * // 使用 mockAdapter 进行请求拦截和模拟
 * alovaInstance.useAdapter(mockAdapter);
 *
 * @see {@link https://alova.js.org/}
 */
const mockAdapter = createAlovaMockAdapter([featureUsers20241014], {
  // using requestAdapter if not match mock request
  httpAdapter: adapterFetch(),

  // response delay time
  delay: 1000,

  // global mock toggle
  enable: true,
  matchMode: 'methodurl'
});

/**
 * 创建并配置 Alova 请求实例。
 *
 * @remarks
 * 该实例用于统一管理前端与后端的 HTTP 请求，支持自动处理鉴权、Token 刷新、错误处理等功能。
 *
 * @example
 * ```typescript
 * import { alova } from 'service-alova/request';
 * alova.get('/api/data');
 * ```
 *
 * @property {string} baseURL - 请求的基础 URL。
 * @property {Function} requestAdapter - 请求适配器，根据开发环境选择 mock 或真实请求。
 * @property {Function} onRequest - 请求发送前的钩子，用于设置请求头（如 Authorization、apifoxToken）。
 * @property {Object} tokenRefresher - Token 刷新相关配置。
 * @property {Function} tokenRefresher.isExpired - 判断 Token 是否过期。
 * @property {Function} tokenRefresher.handler - Token 过期时的处理逻辑。
 * @property {Function} isBackendSuccess - 判断后端响应是否成功。
 * @property {Function} transformBackendResponse - 转换后端响应数据，仅返回 data 字段。
 * @property {Function} onError - 请求错误处理，包括登出、弹窗提示等。
 *
 * @see {@link https://alova.js.org/}
 */
export const alova = createAlovaRequest(
  {
    baseURL,
    requestAdapter: import.meta.env.DEV ? mockAdapter : adapterFetch()
  },
  {
    onRequest({ config }) {
      const Authorization = getAuthorization();
      config.headers.Authorization = Authorization;
      config.headers.apifoxToken = 'XL299LiMEDZ0H5h3A29PxwQXdMJqWyY2';
    },
    tokenRefresher: {
      async isExpired(response) {
        const expiredTokenCodes = import.meta.env.VITE_SERVICE_EXPIRED_TOKEN_CODES?.split(',') || [];
        const { code } = await response.clone().json();
        return expiredTokenCodes.includes(String(code));
      },
      async handler() {
        await handleRefreshToken();
      }
    },
    async isBackendSuccess(response) {
      // when the backend response code is "0000"(default), it means the request is success
      // to change this logic by yourself, you can modify the `VITE_SERVICE_SUCCESS_CODE` in `.env` file
      const resp = response.clone();
      const data = await resp.json();
      return String(data.code) === import.meta.env.VITE_SERVICE_SUCCESS_CODE;
    },
    async transformBackendResponse(response) {
      return (await response.clone().json()).data;
    },
    async onError(error, response) {
      const authStore = useAuthStore();

      let message = error.message;
      let responseCode = '';
      if (response) {
        const data = await response?.clone().json();
        message = data.msg;
        responseCode = String(data.code);
      }

      function handleLogout() {
        showErrorMsg(state, message);
        authStore.resetStore();
      }

      function logoutAndCleanup() {
        handleLogout();
        window.removeEventListener('beforeunload', handleLogout);
        state.errMsgStack = state.errMsgStack.filter(msg => msg !== message);
      }

      // when the backend response code is in `logoutCodes`, it means the user will be logged out and redirected to login page
      const logoutCodes = import.meta.env.VITE_SERVICE_LOGOUT_CODES?.split(',') || [];
      if (logoutCodes.includes(responseCode)) {
        handleLogout();
        throw error;
      }

      // when the backend response code is in `modalLogoutCodes`, it means the user will be logged out by displaying a modal
      const modalLogoutCodes = import.meta.env.VITE_SERVICE_MODAL_LOGOUT_CODES?.split(',') || [];
      if (modalLogoutCodes.includes(responseCode) && !state.errMsgStack?.includes(message)) {
        state.errMsgStack = [...(state.errMsgStack || []), message];

        // prevent the user from refreshing the page
        window.addEventListener('beforeunload', handleLogout);

        if (window.$messageBox) {
          window.$messageBox({
            type: 'error',
            title: $t('common.error'),
            message,
            confirmButtonText: $t('common.confirm'),
            closeOnClickModal: false,
            closeOnPressEscape: false,
            callback() {
              logoutAndCleanup();
            }
          });
        }
        throw error;
      }
      showErrorMsg(state, message);
      throw error;
    }
  }
);
