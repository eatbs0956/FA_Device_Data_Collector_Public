import { alova } from '../request';

/**
 * 发送登录请求，使用用户名和密码进行身份验证。
 *
 * @param userName - 用户名
 * @param password - 密码
 * @returns 返回一个 Promise，解析为登录令牌信息。
 */
export function fetchLogin(userName: string, password: string) {
  return alova.Post<Api.Auth.LoginToken>('/auth/login', { userName, password });
}

/**
 * 获取当前用户信息。
 *
 * 发送 GET 请求到 `/auth/getUserInfo`，返回用户信息数据。
 *
 * @returns 包含用户信息的 Promise 对象。
 */
export function fetchGetUserInfo() {
  return alova.Get<Api.Auth.UserInfo>('/auth/getUserInfo');
}

/**
 * 发送验证码到指定手机号。
 *
 * @param phone - 接收验证码的手机号
 * @returns 返回一个 Promise，解析后无数据（null）
 */
// export function sendCaptcha(phone: string) {
//   return alova.Post<null>('/auth/sendCaptcha', { phone });
// }

/**
 * 验证手机验证码。
 *
 * @param phone - 手机号码，用于接收验证码。
 * @param code - 用户输入的验证码。
 * @returns 返回一个 Promise，表示验证码验证的结果。
 */
// export function verifyCaptcha(phone: string, code: string) {
//   return alova.Post<null>('/auth/verifyCaptcha', { phone, code });
// }

/**
 * 使用刷新令牌请求新的登录令牌。
 *
 * @param refreshToken 用于刷新令牌的字符串。
 * @returns 返回一个包含新登录令牌的 Promise。
 */
export function fetchRefreshToken(refreshToken: string) {
  return alova.Post<Api.Auth.LoginToken>(
    '/auth/refreshToken',
    { refreshToken },
    {
      meta: {
        authRole: 'refreshToken'
      }
    }
  );
}

/**
 * 根据后端返回的错误码和错误信息，向 `/auth/error` 接口发送 GET 请求。
 *
 * @param code 后端错误码
 * @param msg  后端错误信息
 * @returns 返回一个 alova 的 GET 请求对象
 */
export function fetchCustomBackendError(code: string, msg: string) {
  return alova.Get('/auth/error', {
    params: { code, msg },
    shareRequest: false
  });
}
