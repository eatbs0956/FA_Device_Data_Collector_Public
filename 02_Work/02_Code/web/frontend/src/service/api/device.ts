import { request } from '../request';

/**
 * 获取设备列表
 */
export function fetchGetDeviceList(params?: Api.Device.DeviceSearchParams) {
  return request<Api.Device.DeviceList>({
    url: '/api/devices',
    method: 'get',
    params
  });
}

/**
 * 获取设备详情
 */
export function fetchGetDevice(id: string) {
  return request<Api.Device.Device>({
    url: `/api/devices/${id}`,
    method: 'get'
  });
}

/**
 * 新增设备
 */
export function fetchAddDevice(data: Api.Device.DeviceEdit) {
  return request<{ id: string }>({
    url: '/api/devices',
    method: 'post',
    data
  });
}

/**
 * 更新设备
 */
export function fetchUpdateDevice(id: string, data: Api.Device.DeviceEdit) {
  return request({
    url: `/api/devices/${id}`,
    method: 'put',
    data
  });
}

/**
 * 删除设备
 */
export function fetchDeleteDevice(id: string) {
  return request({
    url: `/api/devices/${id}`,
    method: 'delete'
  });
}

/**
 * 批量删除设备
 */
export function fetchBatchDeleteDevices(ids: string[]) {
  return request({
    url: '/api/devices/batch-delete',
    method: 'post',
    data: ids
  });
}

/**
 * 测试设备连接
 */
export function fetchTestDeviceConnection(id: string) {
  return request<Api.Device.ConnectionTestResult>({
    url: `/api/devices/${id}/test-connection`,
    method: 'post'
  });
}

/**
 * 启用/禁用设备
 */
export function fetchToggleDeviceEnabled(id: string, enabled: boolean) {
  return request({
    url: `/api/devices/${id}/enabled`,
    method: 'patch',
    data: enabled,
    headers: {
      'Content-Type': 'application/json'
    },
    transformRequest: [data => JSON.stringify(data)]
  });
}

/**
 * 获取采集节点下拉选项（设备管理页面用）
 * @param silent - 是否静默处理错误（不显示错误弹窗）
 */
export function fetchGetEdgeNodeDropdownForDevice(silent = false) {
  return request<Api.Device.EdgeNode[]>({
    url: '/api/edge-nodes/dropdown',
    method: 'get',
    headers: silent ? { 'X-Silent-Error': 'true' } : undefined
  });
}

/**
 * 获取设备分组树（简化版，用于下拉选择）
 * @param silent - 是否静默处理错误（不显示错误弹窗）
 */
export function fetchGetDeviceGroupTree(silent = false) {
  return request<Api.Device.DeviceGroupTreeNode[]>({
    url: '/api/device-groups/tree/simple',
    method: 'get',
    headers: silent ? { 'X-Silent-Error': 'true' } : undefined
  });
}

/**
 * 获取设备分组树（完整版，包含设备数量等信息）
 */
export function fetchGetDeviceGroupTreeFull() {
  return request<Api.Device.DeviceGroup[]>({
    url: '/api/device-groups/tree',
    method: 'get'
  });
}

/**
 * 获取设备分组列表（分页）
 */
export function fetchGetDeviceGroupList(params?: Api.Device.DeviceGroupSearchParams) {
  return request<Api.Device.DeviceGroupList>({
    url: '/api/device-groups',
    method: 'get',
    params
  });
}

/**
 * 获取设备分组详情
 */
export function fetchGetDeviceGroup(id: string) {
  return request<Api.Device.DeviceGroup>({
    url: `/api/device-groups/${id}`,
    method: 'get'
  });
}

/**
 * 新增设备分组
 */
export function fetchAddDeviceGroup(data: Api.Device.DeviceGroupEdit) {
  return request<{ id: string }>({
    url: '/api/device-groups',
    method: 'post',
    data
  });
}

/**
 * 更新设备分组
 */
export function fetchUpdateDeviceGroup(id: string, data: Api.Device.DeviceGroupEdit) {
  return request({
    url: `/api/device-groups/${id}`,
    method: 'put',
    data
  });
}

/**
 * 删除设备分组
 */
export function fetchDeleteDeviceGroup(id: string) {
  return request({
    url: `/api/device-groups/${id}`,
    method: 'delete'
  });
}

/**
 * 移动设备分组
 */
export function fetchMoveDeviceGroup(id: string, data: { parentId?: string; sortOrder?: number }) {
  return request({
    url: `/api/device-groups/${id}/move`,
    method: 'patch',
    data
  });
}

// ==================== Tag API ====================

/**
 * 获取标签列表（分页）
 */
export function fetchGetTagList(params?: Api.Device.TagSearchParams) {
  return request<Api.Device.TagList>({
    url: '/api/tags',
    method: 'get',
    params
  });
}

/**
 * 获取标签详情
 */
export function fetchGetTag(id: string) {
  return request<Api.Device.Tag>({
    url: `/api/tags/${id}`,
    method: 'get'
  });
}

/**
 * 新增标签
 */
export function fetchAddTag(data: Api.Device.TagEdit) {
  return request<{ id: string }>({
    url: '/api/tags',
    method: 'post',
    data
  });
}

/**
 * 更新标签
 */
export function fetchUpdateTag(id: string, data: Api.Device.TagEdit) {
  return request({
    url: `/api/tags/${id}`,
    method: 'put',
    data
  });
}

/**
 * 删除标签
 */
export function fetchDeleteTag(id: string) {
  return request({
    url: `/api/tags/${id}`,
    method: 'delete'
  });
}

/**
 * 批量删除标签
 */
export function fetchBatchDeleteTags(ids: string[]) {
  return request({
    url: '/api/tags/batch-delete',
    method: 'post',
    data: ids
  });
}

/**
 * 启用/禁用标签
 */
export function fetchToggleTagEnabled(id: string, enabled: boolean) {
  return request({
    url: `/api/tags/${id}/toggle-enabled`,
    method: 'put',
    data: { enabled },
    headers: {
      'Content-Type': 'application/json'
    }
  });
}

/**
 * 批量启用标签
 */
export function fetchBatchEnableTags(ids: string[]) {
  return request({
    url: '/api/tags/batch-enable',
    method: 'post',
    data: ids
  });
}

/**
 * 批量禁用标签
 */
export function fetchBatchDisableTags(ids: string[]) {
  return request({
    url: '/api/tags/batch-disable',
    method: 'post',
    data: ids
  });
}

/**
 * 导出设备的所有标签
 */
export function fetchExportTags(deviceId: string) {
  return request<Api.Device.Tag[]>({
    url: `/api/tags/export/${deviceId}`,
    method: 'get'
  });
}

/**
 * 批量导入标签
 */
export function fetchImportTags(deviceId: string, tags: Api.Device.TagEdit[]) {
  return request<{ importCount: number }>({
    url: `/api/tags/import/${deviceId}`,
    method: 'post',
    data: tags
  });
}

/**
 * 获取设备列表（用于标签管理左侧设备树）- 轻量级接口
 */
export function fetchGetDeviceListForTags(params?: { deviceName?: string; enabled?: boolean }) {
  return request<Api.Device.DeviceList>({
    url: '/api/devices/for-tags',
    method: 'get',
    params
  });
}
