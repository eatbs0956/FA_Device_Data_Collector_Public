import { request } from '../request';

/**
 * 获取角色列表。
 *
 * @param params 可选参数，用于角色搜索的过滤条件。
 * @returns 包含角色列表的请求结果。
 */
export function fetchGetRoleList(params?: Api.SystemManage.RoleSearchParams) {
  return request<Api.SystemManage.RoleList>({
    url: '/systemManage/getRoleList',
    method: 'get',
    params
  });
}

/**
 * 获取所有角色列表。
 *
 * 发送 GET 请求到 `/systemManage/getAllRoles`，返回所有角色的数组。
 *
 * @returns 包含所有角色信息的 Promise。
 */
export function fetchGetAllRoles() {
  return request<Api.SystemManage.AllRole[]>({
    url: '/systemManage/getAllRoles',
    method: 'get'
  });
}

/**
 * 新增角色 - 创建新的角色
 *
 * @param data 角色数据
 * @returns 创建结果
 */
export function fetchAddRole(data: Api.SystemManage.RoleEdit) {
  return request({
    url: '/admin/roles',
    method: 'post',
    data
  });
}

/**
 * 更新角色 - 修改已存在的角色
 *
 * @param id 角色ID
 * @param data 角色数据
 * @returns 更新结果
 */
export function fetchUpdateRole(id: string, data: Api.SystemManage.RoleEdit) {
  return request({
    url: `/admin/roles/${id}`,
    method: 'put',
    data
  });
}

/**
 * 删除角色 - 删除指定ID的角色
 *
 * @param id 角色ID
 * @returns 删除结果
 */
export function fetchDeleteRole(id: string) {
  return request({
    url: `/admin/roles/${id}`,
    method: 'delete'
  });
}

/**
 * 获取用户列表。
 *
 * 发送 GET 请求到 `/systemManage/getUserList`，根据提供的查询参数返回用户列表数据。
 *
 * @param params 用户查询参数，可选。
 * @returns 包含用户列表的请求结果。
 */
export function fetchGetUserList(params?: Api.SystemManage.UserSearchParams) {
  return request<Api.SystemManage.UserList>({
    url: '/systemManage/getUserList',
    method: 'get',
    params
  });
}

/**
 * 新增用户 - 创建新的用户
 *
 * @param data 用户数据
 * @returns 创建结果
 */
export function fetchAddUser(data: Api.SystemManage.UserEdit) {
  return request({
    url: '/admin/users',
    method: 'post',
    data
  });
}

/**
 * 更新用户 - 修改已存在的用户
 *
 * @param id 用户ID
 * @param data 用户数据
 * @returns 更新结果
 */
export function fetchUpdateUser(id: string, data: Api.SystemManage.UserEdit) {
  return request({
    url: `/admin/users/${id}`,
    method: 'put',
    data
  });
}

/**
 * 删除用户 - 删除指定ID的用户
 *
 * @param id 用户ID
 * @returns 删除结果
 */
export function fetchDeleteUser(id: string) {
  return request({
    url: `/admin/users/${id}`,
    method: 'delete'
  });
}

/**
 * 获取菜单列表数据 - 支持分页查询，返回树形结构
 *
 * @param current 当前页码 - 默认第1页
 * @param size 每页数量 - 默认10条
 * @returns 返回一个包含菜单树形列表的 Promise 对象
 */
export function fetchGetMenuList(current = 1, size = 10) {
  return request<Api.SystemManage.MenuList>({
    url: '/admin/getMenuList',
    method: 'get',
    params: { current, size }
  });
}

/**
 * 获取所有页面的列表 - 用于菜单配置时选择页面组件
 *
 * @returns 包含所有页面路由路径的字符串数组
 */
export function fetchGetAllPages() {
  return request<string[]>({
    url: '/systemManage/getAllPages',
    method: 'get'
  });
}

/**
 * 获取菜单详情 - 根据ID查询单个菜单
 *
 * @param id 菜单ID
 * @returns 菜单详细信息
 */
export function fetchGetMenu(id: string) {
  return request<Api.SystemManage.Menu>({
    url: `/admin/menus/${id}`,
    method: 'get'
  });
}

/**
 * 新增菜单 - 创建新的菜单项
 *
 * @param data 菜单数据
 * @returns 创建结果
 */
export function fetchAddMenu(data: any) {
  return request({
    url: '/admin/menus',
    method: 'post',
    data
  });
}

/**
 * 更新菜单 - 修改已存在的菜单
 *
 * @param id 菜单ID
 * @param data 菜单数据
 * @returns 更新结果
 */
export function fetchUpdateMenu(id: string, data: any) {
  return request({
    url: `/admin/menus/${id}`,
    method: 'put',
    data
  });
}

/**
 * 删除菜单 - 删除指定ID的菜单
 *
 * @param id 菜单ID
 * @returns 删除结果
 */
export function fetchDeleteMenu(id: string) {
  return request({
    url: `/admin/menus/${id}`,
    method: 'delete'
  });
}

/**
 * 批量删除菜单 - 删除多个菜单项
 *
 * @param ids 菜单ID数组
 * @returns 删除结果
 */
export function fetchBatchDeleteMenus(ids: string[]) {
  return request({
    url: '/admin/menus/batch',
    method: 'delete',
    data: ids
  });
}

/**
 * 获取菜单树 - 用于角色授权时展示菜单树
 *
 * @returns 菜单树数据
 */
export function fetchGetMenuTree() {
  return request<Api.SystemManage.MenuTree[]>({
    url: '/systemManage/getMenuTree',
    method: 'get'
  });
}

/**
 * 获取角色的菜单权限 - 获取指定角色已授权的菜单ID列表
 *
 * @param roleId 角色ID
 * @returns 菜单ID数组
 */
export function fetchGetRoleMenus(roleId: string) {
  return request<number[]>({
    url: `/systemManage/getRoleMenus/${roleId}`,
    method: 'get'
  });
}

/**
 * 保存角色的菜单权限 - 更新角色的菜单授权
 *
 * @param roleId 角色ID
 * @param menuIds 菜单ID数组
 * @returns 保存结果
 */
export function fetchSaveRoleMenus(roleId: string, menuIds: number[]) {
  return request({
    url: `/systemManage/saveRoleMenus/${roleId}`,
    method: 'post',
    data: { menuIds }
  });
}
