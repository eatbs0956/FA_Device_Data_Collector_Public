import { request } from '../request';

/**
 * 发送登录请求，使用用户名和密码进行身份验证。
 *
 * @param userName 用户名
 * @param password 密码
 * @returns 返回一个包含登录令牌的 Promise 对象
 */
export function fetchLogin(userName: string, password: string) {
  return request<Api.Auth.LoginToken>({
    url: '/auth/login',
    method: 'post',
    data: {
      userName,
      password
    }
  });
}

/**
 * 获取当前用户信息。
 *
 * 该函数会向 `/auth/getUserInfo` 接口发送请求，返回用户信息数据。
 *
 * @returns 包含用户信息的 Promise 对象。
 */
export function fetchGetUserInfo() {
  return request<Api.Auth.UserInfo>({ url: '/auth/getUserInfo' });
}

/**
 * 使用刷新令牌（refreshToken）向服务器请求新的访问令牌。
 *
 * @param refreshToken 用于刷新访问令牌的刷新令牌字符串
 * @returns 包含新的登录令牌的请求结果
 */
export function fetchRefreshToken(refreshToken: string) {
  return request<Api.Auth.LoginToken>({
    url: '/auth/refreshToken',
    method: 'post',
    data: {
      refreshToken
    }
  });
}

/**
 * 根据提供的错误码和错误信息，向后端发送自定义错误请求。
 *
 * @param code 错误码，用于标识具体的错误类型。
 * @param msg 错误信息，描述错误的详细内容。
 * @returns 返回一个包含后端响应的 Promise 对象。
 */
export function fetchCustomBackendError(code: string, msg: string) {
  return request({ url: '/auth/error', params: { code, msg } });
}
