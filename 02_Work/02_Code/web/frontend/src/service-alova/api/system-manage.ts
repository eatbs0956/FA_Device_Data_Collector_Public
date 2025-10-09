import { alova } from '../request';

/**
 * 获取角色列表。
 *
 * @param params 可选参数，用于角色搜索过滤。
 * @returns 包含角色列表的 Promise 对象。
 */
export function fetchGetRoleList(params?: Api.SystemManage.RoleSearchParams) {
  return alova.Get<Api.SystemManage.RoleList>('/systemManage/getRoleList', { params });
}

/**
 * 获取所有角色信息。
 *
 * 该方法通过 GET 请求 `/systemManage/getAllRoles` 接口，返回所有角色的列表。
 *
 * @returns 包含所有角色信息的 Promise 对象。
 */
export function fetchGetAllRoles() {
  return alova.Get<Api.SystemManage.AllRole[]>('/systemManage/getAllRoles');
}

/**
 * 获取用户列表。
 *
 * @param params 用户搜索参数，可选。
 * @returns 包含用户列表的 Promise 对象。
 */
export function fetchGetUserList(params?: Api.SystemManage.UserSearchParams) {
  return alova.Get<Api.SystemManage.UserList>('/systemManage/getUserList', { params });
}

/**
 * 用户模型类型，仅包含用户的基本信息字段。
 *
 * 包含以下属性：
 * - userName: 用户名
 * - userGender: 用户性别
 * - nickName: 用户昵称
 * - userPhone: 用户手机号
 * - userEmail: 用户邮箱
 * - userRoles: 用户角色列表
 * - status: 用户状态
 */
export type UserModel = Pick<
  Api.SystemManage.User,
  'userName' | 'userGender' | 'nickName' | 'userPhone' | 'userEmail' | 'userRoles' | 'status'
>;

/**
 * 添加新用户。
 *
 * @param data 用户信息数据，类型为 UserModel。
 * @returns 返回一个 Promise，解析结果为 null。
 */
export function addUser(data: UserModel) {
  return alova.Post<null>('/systemManage/addUser', data);
}

/**
 * 更新用户信息。
 *
 * @param data 用户信息数据，类型为 UserModel。
 * @returns 返回一个 Promise，结果为 null，表示更新操作的响应。
 */
export function updateUser(data: UserModel) {
  return alova.Post<null>('/systemManage/updateUser', data);
}

/**
 * 删除指定用户。
 *
 * @param id 用户的唯一标识符
 * @returns 返回一个 Promise，表示删除操作的结果
 */
export function deleteUser(id: number) {
  return alova.Delete<null>('/systemManage/deleteUser', { id });
}

/**
 * 批量删除用户。
 *
 * @param ids 用户ID数组
 * @returns 返回一个Promise对象，删除操作完成后返回null
 */
export function batchDeleteUser(ids: number[]) {
  return alova.Delete<null>('/systemManage/batchDeleteUser', { ids });
}

/**
 * 获取菜单列表数据。
 *
 * 该方法通过 GET 请求从 `/systemManage/getMenuList/v2` 接口获取菜单列表。
 *
 * @returns 返回一个包含菜单列表的 Promise 对象。
 */
export function fetchGetMenuList() {
  return alova.Get<Api.SystemManage.MenuList>('/systemManage/getMenuList/v2');
}

/**
 * 获取所有页面的名称列表。
 *
 * @returns 返回一个 Promise，解析为字符串数组，包含所有页面的名称。
 */
export function fetchGetAllPages() {
  return alova.Get<string[]>('/systemManage/getAllPages');
}

/**
 * 获取菜单树数据。
 *
 * 该方法通过 GET 请求从 `/systemManage/getMenuTree` 接口获取菜单树列表。
 *
 * @returns 返回一个 Promise，解析为菜单树数组 `Api.SystemManage.MenuTree[]`。
 */
export function fetchGetMenuTree() {
  return alova.Get<Api.SystemManage.MenuTree[]>('/systemManage/getMenuTree');
}
