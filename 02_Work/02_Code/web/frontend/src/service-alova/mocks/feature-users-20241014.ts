import { defineMock } from '@sa/alova/mock';

/**
 * @module feature-users-20241014
 *
 * 该模块定义了一组用于模拟用户管理和验证码相关接口的 mock 数据。
 *
 * 包含的接口有：
 * - [POST]/systemManage/addUser: 添加用户
 * - [POST]/systemManage/updateUser: 更新用户信息
 * - [DELETE]/systemManage/deleteUser: 删除单个用户
 * - [DELETE]/systemManage/batchDeleteUser: 批量删除用户
 * - [POST]/auth/sendCaptcha: 发送验证码
 * - [POST]/auth/verifyCaptcha: 验证验证码
 * - /mock/getLastTime: 获取当前时间（模拟）
 *
 * 所有接口均返回统一格式的响应对象，包含 code、msg 和 data 字段。
 */
export default defineMock({
  '[POST]/systemManage/addUser': () => {
    return {
      code: '0000',
      msg: 'success',
      data: null
    };
  },
  '[POST]/systemManage/updateUser': () => {
    return {
      code: '0000',
      msg: 'success',
      data: null
    };
  },
  '[DELETE]/systemManage/deleteUser': () => {
    return {
      code: '0000',
      msg: 'success',
      data: null
    };
  },
  '[DELETE]/systemManage/batchDeleteUser': () => {
    return {
      code: '0000',
      msg: 'success',
      data: null
    };
  },
  // '[POST]/auth/sendCaptcha': () => {
  //   return {
  //     code: '0000',
  //     msg: 'success',
  //     data: null
  //   };
  // },
  // '[POST]/auth/verifyCaptcha': () => {
  //   return {
  //     code: '0000',
  //     msg: 'success',
  //     data: null
  //   };
  // },
  '/mock/getLastTime': () => {
    return {
      code: '0000',
      msg: 'success',
      data: {
        time: new Date().toLocaleTimeString()
      }
    };
  }
});
