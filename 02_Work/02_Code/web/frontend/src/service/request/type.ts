export interface RequestInstanceState {
  /** 刷新token的promise */
  refreshTokenPromise: Promise<boolean> | null;
  /** 请求错误信息堆栈 */
  errMsgStack: string[];
  /** 正在刷新token的标志 */
  isRefreshing: boolean;
  /** 等待token刷新完成的请求队列 */
  pendingRequests: Array<(token: string) => void>;
  [key: string]: unknown;
}
