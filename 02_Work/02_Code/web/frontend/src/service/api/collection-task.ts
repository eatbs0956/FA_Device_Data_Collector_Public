import { request } from '../request';

// ==================== 采集任务 API ====================

/**
 * 获取采集任务列表（分页）
 */
export function fetchGetCollectionTaskList(params?: Api.CollectionTask.CollectionTaskSearchParams) {
  return request<Api.CollectionTask.CollectionTaskList>({
    url: '/api/collection-tasks',
    method: 'get',
    params
  });
}

/**
 * 获取采集任务详情
 */
export function fetchGetCollectionTask(id: string) {
  return request<Api.CollectionTask.CollectionTask>({
    url: `/api/collection-tasks/${id}`,
    method: 'get'
  });
}

/**
 * 创建采集任务
 */
export function fetchAddCollectionTask(data: Api.CollectionTask.CreateCollectionTaskRequest) {
  return request<{ id: string }>({
    url: '/api/collection-tasks',
    method: 'post',
    data
  });
}

/**
 * 更新采集任务
 */
export function fetchUpdateCollectionTask(id: string, data: Api.CollectionTask.UpdateCollectionTaskRequest) {
  return request<null>({
    url: `/api/collection-tasks/${id}`,
    method: 'put',
    data
  });
}

/**
 * 删除采集任务
 */
export function fetchDeleteCollectionTask(id: string) {
  return request<null>({
    url: `/api/collection-tasks/${id}`,
    method: 'delete'
  });
}

/**
 * 变更任务状态
 */
export function fetchChangeCollectionTaskStatus(id: string, data: Api.CollectionTask.TaskStatusChangeRequest) {
  return request<null>({
    url: `/api/collection-tasks/${id}/status`,
    method: 'put',
    data
  });
}

/**
 * 获取可用设备列表
 */
export function fetchGetAvailableDevices(taskId?: string) {
  return request<Api.CollectionTask.AvailableDevice[]>({
    url: '/api/collection-tasks/available-devices',
    method: 'get',
    params: taskId ? { taskId } : undefined
  });
}
