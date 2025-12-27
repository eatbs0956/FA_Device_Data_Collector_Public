import { request } from '../request';

// ==================== 边缘节点 API ====================

/**
 * 获取边缘节点列表（分页）
 */
export function fetchGetEdgeNodeList(params?: Api.EdgeNode.EdgeNodeSearchParams) {
  return request<Api.EdgeNode.EdgeNodeList>({
    url: '/api/edge-nodes',
    method: 'get',
    params
  });
}

/**
 * 获取边缘节点下拉列表
 */
export function fetchGetEdgeNodeDropdown() {
  return request<Api.EdgeNode.EdgeNodeDropdownItem[]>({
    url: '/api/edge-nodes/dropdown',
    method: 'get'
  });
}

/**
 * 获取边缘节点详情
 */
export function fetchGetEdgeNode(id: string) {
  return request<Api.EdgeNode.EdgeNode>({
    url: `/api/edge-nodes/${id}`,
    method: 'get'
  });
}

/**
 * 创建边缘节点
 */
export function fetchAddEdgeNode(data: Api.EdgeNode.CreateEdgeNodeRequest) {
  return request<{ id: string }>({
    url: '/api/edge-nodes',
    method: 'post',
    data
  });
}

/**
 * 更新边缘节点
 */
export function fetchUpdateEdgeNode(id: string, data: Api.EdgeNode.UpdateEdgeNodeRequest) {
  return request<null>({
    url: `/api/edge-nodes/${id}`,
    method: 'put',
    data
  });
}

/**
 * 删除边缘节点
 */
export function fetchDeleteEdgeNode(id: string) {
  return request<{ deviceCount: number }>({
    url: `/api/edge-nodes/${id}`,
    method: 'delete'
  });
}

/**
 * 获取边缘节点关联的设备数量
 */
export function fetchGetEdgeNodeDeviceCount(id: string) {
  return request<{ count: number }>({
    url: `/api/edge-nodes/${id}/device-count`,
    method: 'get'
  });
}
