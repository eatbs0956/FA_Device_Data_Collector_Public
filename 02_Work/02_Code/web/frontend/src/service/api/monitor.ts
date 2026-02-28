import { request } from '../request';

// ==================== 瀹炴椂鐩戞帶 API ====================

/**
 * 鑾峰彇鎵€鏈夎澶囩殑瀹炴椂鐩戞帶鏁版嵁
 */
export function fetchGetDevicesLatest(params?: { groupId?: string; nodeId?: string; connectionStatus?: string }) {
  return request<Api.Monitor.DeviceMonitorData[]>({
    url: '/api/monitor/devices/latest',
    method: 'get',
    params
  });
}

/**
 * 鑾峰彇鍗曚釜璁惧鐨勬渶鏂拌缁嗘暟鎹?
 */
export function fetchGetDeviceLatest(id: string) {
  return request<Api.Monitor.DeviceDetailData>({
    url: `/api/monitor/devices/${id}/latest`,
    method: 'get'
  });
}

// ==================== 鍘嗗彶鏁版嵁 API ====================

/**
 * 鑾峰彇璁惧鏍?
 */
export function fetchGetDeviceTree() {
  return request<Api.Monitor.DeviceTreeNode[]>({
    url: '/api/monitor/device-tree',
    method: 'get'
  });
}

/**
 * 鑾峰彇璁惧鍘嗗彶鏁版嵁
 */
export function fetchGetDeviceHistory(id: string, params: Api.Monitor.HistoryQueryParams) {
  return request<Api.Monitor.HistoryDataResult>({
    url: `/api/monitor/devices/${id}/history`,
    method: 'get',
    params
  });
}

// ==================== 缁熻鎶ヨ〃 API ====================

/**
 * 鎸夎澶囩粺璁?
 */
export function fetchGetStatisticsByDevices(params?: Api.Monitor.StatisticsQueryParams) {
  return request<Api.Monitor.StatisticsResult[]>({
    url: '/api/monitor/statistics/devices',
    method: 'get',
    params
  });
}

/**
 * 鎸夊垎缁勭粺璁?
 */
export function fetchGetStatisticsByGroups(params?: Api.Monitor.StatisticsQueryParams) {
  return request<Api.Monitor.StatisticsResult[]>({
    url: '/api/monitor/statistics/groups',
    method: 'get',
    params
  });
}

/**
 * 鎸夎妭鐐圭粺璁?
 */
export function fetchGetStatisticsByNodes(params?: Api.Monitor.StatisticsQueryParams) {
  return request<Api.Monitor.StatisticsResult[]>({
    url: '/api/monitor/statistics/nodes',
    method: 'get',
    params
  });
}

// ==================== 浠〃鐩?API ====================

/**
 * 鑾峰彇浠〃鐩樻憳瑕佹暟鎹?
 */
export function fetchGetDashboardSummary() {
  return request<Api.Monitor.DashboardSummary>({
    url: '/api/monitor/dashboard/summary',
    method: 'get'
  });
}
