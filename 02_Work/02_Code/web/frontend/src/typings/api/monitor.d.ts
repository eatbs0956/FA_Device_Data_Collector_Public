declare namespace Api {
  /**
   * namespace Monitor
   *
   * backend api module: "monitor"
   */
  namespace Monitor {
    // ==================== Real-time Monitoring Types ====================

    /** Tag value item */
    type TagValueItem = {
      /** Tag name */
      tagName: string;
      /** Display name */
      displayName?: string;
      /** Current value */
      value?: any;
      /** Unit */
      unit?: string;
      /** Update time */
      updatedAt?: string;
    };

    /** Device monitor data (for real-time monitoring cards) */
    type DeviceMonitorData = {
      /** Device ID (database primary key) */
      id: string;
      /** Device identifier */
      deviceId: string;
      /** Device name */
      deviceName: string;
      /** Connection status */
      connectionStatus: string;
      /** Is enabled */
      enabled: boolean;
      /** Group name */
      groupName?: string;
      /** Node name */
      nodeName?: string;
      /** Last update time */
      lastUpdateTime?: string;
      /** Key tags data */
      keyTags: TagValueItem[];
    };

    /** Device detail data (for dialog display) */
    type DeviceDetailData = {
      /** Device ID */
      id: string;
      /** Device identifier */
      deviceId: string;
      /** Device name */
      deviceName: string;
      /** Connection status */
      connectionStatus: string;
      /** Device type */
      deviceType: string;
      /** Protocol type */
      protocolType: string;
      /** Location */
      location?: string;
      /** Last connect time */
      lastConnectTime?: string;
      /** All tags latest values */
      allTags: TagValueItem[];
    };

    // ==================== Historical Data Types ====================

    /** Device tree node */
    type DeviceTreeNode = {
      /** Node ID */
      id: string;
      /** Node label */
      label: string;
      /** Node type (group/device/tag) */
      type: 'group' | 'device' | 'tag';
      /** Is leaf node */
      isLeaf: boolean;
      /** Children nodes */
      children?: DeviceTreeNode[];
      /** Device ID (only for device type) */
      deviceId?: string;
      /** Device status (only for device type) */
      status?: string;
    };

    /** History query params */
    type HistoryQueryParams = {
      /** Start time */
      start?: string;
      /** End time */
      end?: string;
      /** Tags list (comma separated) */
      tags?: string;
      /** Sample interval */
      interval?: string;
      /** Aggregate function */
      aggregateFn?: string;
      /** Max records limit */
      limit?: number;
    };

    /** Time series data point */
    type TimeSeriesPoint = {
      /** Timestamp */
      timestamp: string;
      /** Tag values */
      values: Record<string, any>;
    };

    /** History data result */
    type HistoryDataResult = {
      /** Device ID */
      deviceId: string;
      /** Time series data */
      series: TimeSeriesPoint[];
    };

    // ==================== Statistics Types ====================

    /** Statistics query params */
    type StatisticsQueryParams = {
      /** Start time */
      start?: string;
      /** End time */
      end?: string;
      /** Aggregation granularity */
      granularity?: string;
      /** Device IDs */
      deviceIds?: string;
      /** Group ID */
      groupId?: string;
      /** Node ID */
      nodeId?: string;
    };

    /** Aggregated value */
    type AggregatedValue = {
      /** Min value */
      min?: number;
      /** Max value */
      max?: number;
      /** Average value */
      avg?: number;
      /** Sum */
      sum?: number;
      /** Count */
      count: number;
    };

    /** Statistics item */
    type StatisticsItem = {
      /** Period start */
      periodStart: string;
      /** Period end */
      periodEnd: string;
      /** Data point count */
      dataPointCount: number;
      /** Online device count */
      onlineDeviceCount?: number;
      /** Alert count */
      alertCount?: number;
      /** Tag aggregations */
      tagAggregations?: Record<string, AggregatedValue>;
    };

    /** Statistics result */
    type StatisticsResult = {
      /** Statistics dimension */
      dimension: string;
      /** Dimension ID */
      dimensionId: string;
      /** Dimension name */
      dimensionName: string;
      /** Statistics items */
      items: StatisticsItem[];
    };

    // ==================== Dashboard Types ====================

    /** Recent alert */
    type RecentAlert = {
      /** Alert ID */
      alertId: string;
      /** Device ID */
      deviceId: string;
      /** Device name */
      deviceName: string;
      /** Alert level */
      level: string;
      /** Alert message */
      message: string;
      /** Alert time */
      alertTime: string;
      /** Is handled */
      isHandled: boolean;
    };

    /** Group device count */
    type GroupDeviceCount = {
      /** Group ID */
      groupId: string;
      /** Group name */
      groupName: string;
      /** Device count */
      deviceCount: number;
      /** Online count */
      onlineCount: number;
    };

    /** Trend point */
    type TrendPoint = {
      /** Time point */
      time: string;
      /** Value */
      value: number;
    };

    /** Dashboard summary data */
    type DashboardSummary = {
      /** Total devices */
      totalDevices: number;
      /** Online devices */
      onlineDevices: number;
      /** Offline devices */
      offlineDevices: number;
      /** Error devices */
      errorDevices: number;
      /** Online rate */
      onlineRate: number;
      /** Today data points */
      todayDataPoints: number;
      /** Yesterday data points */
      yesterdayDataPoints: number;
      /** Today alerts */
      todayAlerts: number;
      /** Unhandled alerts */
      unhandledAlerts: number;
      /** Total nodes */
      totalNodes: number;
      /** Online nodes */
      onlineNodes: number;
      /** Recent alerts list */
      recentAlerts: RecentAlert[];
      /** Group statistics */
      groupStats: GroupDeviceCount[];
      /** Collection trend data */
      collectionTrend: TrendPoint[];
    };
  }
}
