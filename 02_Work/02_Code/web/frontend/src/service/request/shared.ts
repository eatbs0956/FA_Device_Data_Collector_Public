import { useAuthStore } from '@/store/modules/auth';
import { localStg } from '@/utils/storage';
import { fetchRefreshToken } from '../api';
import type { RequestInstanceState } from './type';

/**
 * 获取本地存储中的 token，并生成用于请求的 Authorization 头部字符串。
 * 如果 token 存在，则返回格式为 `Bearer <token>` 的字符串；否则返回 null。
 *
 * @returns {string | null} 用于请求的 Authorization 头部字符串或 null。
 */
export function getAuthorization() {
  const token = localStg.get('token');
  const Authorization = token ? `Bearer ${token}` : null;

  return Authorization;
}

/**
 * @zh
 * 尝试使用本地存储中的刷新令牌（refreshToken）获取新的访问令牌（token）。
 * 如果刷新成功，则更新本地存储中的令牌信息并返回 true；
 * 如果刷新失败，则重置认证状态并返回 false。
 *
 * @returns {Promise<boolean>} 是否成功刷新令牌。
 */
async function handleRefreshToken() {
  const { resetStore } = useAuthStore();

  const rToken = localStg.get('refreshToken') || '';
  const { error, data } = await fetchRefreshToken(rToken);
  if (!error) {
    localStg.set('token', data.token);
    localStg.set('refreshToken', data.refreshToken);
    return true;
  }

  resetStore();

  return false;
}

/**
 * 处理请求过期的情况,支持请求队列防止并发刷新token
 *
 * 工作流程：
 * 1. 如果没有正在刷新，启动刷新流程
 * 2. 如果正在刷新，将请求加入队列等待
 * 3. 刷新完成后，通知所有等待的请求
 *
 * @param state 请求实例的状态对象，包含刷新令牌的方法和相关状态。
 * @returns 一个 Promise，解析为刷新令牌操作是否成功的布尔值。
 */
export async function handleExpiredRequest(state: RequestInstanceState): Promise<boolean> {
  // 如果正在刷新token，将请求加入队列等待
  if (state.isRefreshing) {
    return new Promise<boolean>(resolve => {
      state.pendingRequests.push((token: string) => {
        resolve(Boolean(token));
      });
    });
  }

  // 开始刷新token
  state.isRefreshing = true;

  try {
    const success = await handleRefreshToken();

    if (success) {
      const newToken = localStg.get('token') || '';
      // 通知所有等待的请求
      state.pendingRequests.forEach(callback => callback(newToken));
    } else {
      // 刷新失败，通知所有等待的请求
      state.pendingRequests.forEach(callback => callback(''));
    }

    // 清空队列
    state.pendingRequests = [];

    return success;
  } finally {
    // 重置刷新状态
    state.isRefreshing = false;
  }
}

/**
 * 显示错误消息并将其添加到错误消息堆栈中，避免重复显示相同的消息。
 * 当消息关闭时，会从堆栈中移除该消息，并在 5 秒后清空整个堆栈。
 *
 * @param state - 请求实例的状态对象，包含错误消息堆栈。
 * @param message - 需要显示的错误消息文本。
 */
export function showErrorMsg(state: RequestInstanceState, message: string) {
  if (!state.errMsgStack?.length) {
    state.errMsgStack = [];
  }

  const isExist = state.errMsgStack.includes(message);

  if (!isExist) {
    state.errMsgStack.push(message);

    window.$message?.error({
      message,
      onClose: () => {
        state.errMsgStack = state.errMsgStack.filter(msg => msg !== message);

        setTimeout(() => {
          state.errMsgStack = [];
        }, 5000);
      }
    });
  }
}
