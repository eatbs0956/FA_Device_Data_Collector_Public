import type { AxiosResponse } from 'axios';
import { BACKEND_ERROR_CODE, createFlatRequest, createRequest } from '@sa/axios';
import { useAuthStore } from '@/store/modules/auth';
import { localStg } from '@/utils/storage';
import { getServiceBaseURL } from '@/utils/service';
import { $t } from '@/locales';
import { getAuthorization, handleExpiredRequest, showErrorMsg } from './shared';
import type { RequestInstanceState } from './type';

const isHttpProxy = import.meta.env.DEV && import.meta.env.VITE_HTTP_PROXY === 'Y';
const { baseURL, otherBaseURL } = getServiceBaseURL(import.meta.env, isHttpProxy);

/**
 * 检查错误码是否应该跳过显示错误消息
 */
function shouldSkipErrorMessage(backendErrorCode: string): boolean {
  const modalLogoutCodes = import.meta.env.VITE_SERVICE_MODAL_LOGOUT_CODES?.split(',') || [];
  if (modalLogoutCodes.includes(backendErrorCode)) {
    return true;
  }

  const expiredTokenCodes = import.meta.env.VITE_SERVICE_EXPIRED_TOKEN_CODES?.split(',') || [];
  if (expiredTokenCodes.includes(backendErrorCode)) {
    return true;
  }

  return false;
}

/**
 * 创建并配置一个扁平化的请求实例，用于统一处理前端与后端的接口交互。
 *
 * - 自动注入 `Authorization` 头部信息。
 * - 支持自定义基础 URL 和请求头。
 * - 响应数据自动提取 `data` 字段。
 * - 根据后端返回的 `code` 字段判断请求是否成功，支持通过 `.env` 文件配置成功码。
 * - 统一处理后端失败逻辑，包括：
 *   - 登出相关错误码自动重置用户状态并跳转登录页。
 *   - 弹窗登出错误码弹出提示框，阻止页面刷新，确认后登出并清理错误信息。
 *   - Token 过期错误码自动刷新 Token 并重试请求。
 * - 请求失败时统一展示错误信息，特殊错误码（如登出、Token 过期）不弹窗提示。
 *
 * @remarks
 * 需配合 `.env` 文件中的相关配置项使用：
 * - `VITE_SERVICE_SUCCESS_CODE`：后端成功码
 * - `VITE_SERVICE_LOGOUT_CODES`：登出错误码列表（逗号分隔）
 * - `VITE_SERVICE_MODAL_LOGOUT_CODES`：弹窗登出错误码列表
 * - `VITE_SERVICE_EXPIRED_TOKEN_CODES`：Token 过期错误码列表
 *
 * @example
 * ```typescript
 * const res = await request.get('/api/user/info');
 * ```
 */
export const request = createFlatRequest(
  {
    baseURL,
    headers: {
      apifoxToken: 'XL299LiMEDZ0H5h3A29PxwQXdMJqWyY2'
    }
  },
  {
    defaultState: {
      errMsgStack: [],
      refreshTokenPromise: null,
      isRefreshing: false,
      pendingRequests: []
    } as RequestInstanceState,
    transform(response: AxiosResponse<App.Service.Response<any>>) {
      return response.data.data;
    },
    async onRequest(config) {
      const Authorization = getAuthorization();
      Object.assign(config.headers, { Authorization });

      return config;
    },
    isBackendSuccess(response) {
      // when the backend response code is "0000"(default), it means the request is success
      // to change this logic by yourself, you can modify the `VITE_SERVICE_SUCCESS_CODE` in `.env` file
      return String(response.data.code) === import.meta.env.VITE_SERVICE_SUCCESS_CODE;
    },
    async onBackendFail(response, instance) {
      const authStore = useAuthStore();
      const responseCode = String(response.data.code);

      function handleLogout() {
        authStore.resetStore();
      }

      function logoutAndCleanup() {
        handleLogout();
        window.removeEventListener('beforeunload', handleLogout);

        request.state.errMsgStack = request.state.errMsgStack.filter(msg => msg !== response.data.msg);
      }

      // when the backend response code is in `logoutCodes`, it means the user will be logged out and redirected to login page
      const logoutCodes = import.meta.env.VITE_SERVICE_LOGOUT_CODES?.split(',') || [];
      if (logoutCodes.includes(responseCode)) {
        handleLogout();
        return null;
      }

      // when the backend response code is in `modalLogoutCodes`, it means the user will be logged out by displaying a modal
      const modalLogoutCodes = import.meta.env.VITE_SERVICE_MODAL_LOGOUT_CODES?.split(',') || [];
      if (modalLogoutCodes.includes(responseCode) && !request.state.errMsgStack?.includes(response.data.msg)) {
        request.state.errMsgStack = [...(request.state.errMsgStack || []), response.data.msg];

        // prevent the user from refreshing the page
        window.addEventListener('beforeunload', handleLogout);

        window.$messageBox
          ?.confirm(response.data.msg, $t('common.error'), {
            confirmButtonText: $t('common.confirm'),
            cancelButtonText: $t('common.cancel'),
            type: 'error',
            closeOnClickModal: false,
            closeOnPressEscape: false
          })
          .then(() => {
            logoutAndCleanup();
          });

        return null;
      }

      // when the backend response code is in `expiredTokenCodes`, it means the token is expired, and refresh token
      // the api `refreshToken` can not return error code in `expiredTokenCodes`, otherwise it will be a dead loop, should return `logoutCodes` or `modalLogoutCodes`
      const expiredTokenCodes = import.meta.env.VITE_SERVICE_EXPIRED_TOKEN_CODES?.split(',') || [];
      if (expiredTokenCodes.includes(responseCode)) {
        const success = await handleExpiredRequest(request.state);
        if (success) {
          const Authorization = getAuthorization();
          Object.assign(response.config.headers, { Authorization });

          return instance.request(response.config) as Promise<AxiosResponse>;
        }
      }

      return null;
    },
    onError(error) {
      // Check if this is a silent request (no error message should be shown)
      const isSilent = error.config?.headers?.['X-Silent-Error'] === 'true';
      if (isSilent) {
        return;
      }

      // when the request is fail, you can show error message
      let message = error.message;
      let backendErrorCode = '';

      // Handle 403 Forbidden error with custom message
      if (error.response?.status === 403) {
        message = $t('request.forbidden');
      } else if (error.code === BACKEND_ERROR_CODE) {
        // Backend logic error (2xx response but business logic failed)
        message = error.response?.data?.msg || message;
        backendErrorCode = String(error.response?.data?.code || '');
      } else if (error.response?.data?.msg) {
        // HTTP error (4xx/5xx) but backend returned friendly message
        message = error.response.data.msg;
        backendErrorCode = String(error.response.data.code || '');
      }

      // Skip error message for special error codes (logout, token expired)
      if (shouldSkipErrorMessage(backendErrorCode)) {
        return;
      }

      showErrorMsg(request.state, message);
    }
  }
);

/**
 * 创建一个用于演示的请求实例，包含请求拦截、响应转换、错误处理等逻辑。
 *
 * - `baseURL`: 设置请求的基础地址为 `otherBaseURL.demo`。
 * - `transform`: 响应转换函数，返回后端数据的 `result` 字段。
 * - `onRequest`: 请求拦截器，自动为请求头添加 `Authorization` token。
 * - `isBackendSuccess`: 判断后端请求是否成功，成功条件为 `status === '200'`。
 * - `onBackendFail`: 后端请求失败的处理逻辑（例如 token 过期刷新等）。
 * - `onError`: 请求失败时的错误处理，支持显示后端错误消息。
 *
 * @example
 * ```typescript
 * const res = await demoRequest.get('/api/demo');
 * ```
 */
export const demoRequest = createRequest(
  {
    baseURL: otherBaseURL.demo
  },
  {
    transform(response: AxiosResponse<App.Service.DemoResponse>) {
      return response.data.result;
    },
    async onRequest(config) {
      const { headers } = config;

      // set token
      const token = localStg.get('token');
      const Authorization = token ? `Bearer ${token}` : null;
      Object.assign(headers, { Authorization });

      return config;
    },
    isBackendSuccess(response) {
      // when the backend response code is "200", it means the request is success
      // you can change this logic by yourself
      return response.data.status === '200';
    },
    async onBackendFail(_response) {
      // when the backend response code is not "200", it means the request is fail
      // for example: the token is expired, refresh token and retry request
    },
    onError(error) {
      // when the request is fail, you can show error message

      let message = error.message;

      // show backend error message
      if (error.code === BACKEND_ERROR_CODE) {
        message = error.response?.data?.message || message;
      }

      window.$message?.error(message);
    }
  }
);
