/**
 * Collection Task (采集任务) 相关类型定义
 */
declare namespace Api {
  namespace CollectionTask {
    /** 任务类型 */
    type TaskType = "Periodic" | "Scheduled" | "EventDriven" | "Hybrid";

    /** 任务状态 */
    type TaskStatus = "Draft" | "Active" | "Paused" | "Stopped";

    /** 采集任务 */
    interface CollectionTask {
      /** 主键ID */
      id: string;
      /** 任务名称 */
      name: string;
      /** 任务编码 */
      code: string;
      /** 任务描述 */
      description: string | null;
      /** 任务类型 */
      taskType: TaskType;
      /** 默认采集间隔(毫秒) */
      defaultInterval: number | null;
      /** Cron表达式 */
      cronExpression: string | null;
      /** 优先级 (1-10) */
      priority: number;
      /** 任务状态 */
      status: TaskStatus;
      /** 是否启用 */
      isEnabled: boolean;
      /** 生效开始时间 */
      effectiveFrom: string | null;
      /** 生效结束时间 */
      effectiveTo: string | null;
      /** 关联设备数量 */
      deviceCount: number;
      /** 关联设备ID列表 */
      deviceIds: string[];
      /** 创建时间 */
      createdAt: string;
      /** 更新时间 */
      updatedAt: string | null;
    }

    /** 采集任务列表响应 */
    interface CollectionTaskList {
      /** 任务列表 */
      records: CollectionTask[];
      /** 总数 */
      total: number;
      /** 当前页码 */
      current: number;
      /** 每页大小 */
      size: number;
    }

    /** 采集任务查询参数 */
    interface CollectionTaskSearchParams {
      /** 当前页码 */
      current?: number;
      /** 每页大小 */
      size?: number;
      /** 任务名称 */
      name?: string;
      /** 任务类型 */
      taskType?: TaskType;
      /** 任务状态 */
      status?: TaskStatus;
    }

    /** 创建采集任务请求 */
    interface CreateCollectionTaskRequest {
      /** 任务名称 */
      name: string;
      /** 任务编码 */
      code: string;
      /** 任务描述 */
      description?: string;
      /** 任务类型 */
      taskType: TaskType;
      /** 默认采集间隔(毫秒) */
      defaultInterval?: number;
      /** Cron表达式 */
      cronExpression?: string;
      /** 优先级 (1-10) */
      priority?: number;
      /** 是否启用 */
      isEnabled?: boolean;
      /** 生效开始时间 */
      effectiveFrom?: string;
      /** 生效结束时间 */
      effectiveTo?: string;
      /** 关联设备ID列表 */
      deviceIds?: string[];
    }

    /** 更新采集任务请求 */
    interface UpdateCollectionTaskRequest {
      /** 任务名称 */
      name: string;
      /** 任务描述 */
      description?: string;
      /** 任务类型 */
      taskType: TaskType;
      /** 默认采集间隔(毫秒) */
      defaultInterval?: number;
      /** Cron表达式 */
      cronExpression?: string;
      /** 优先级 (1-10) */
      priority?: number;
      /** 是否启用 */
      isEnabled?: boolean;
      /** 生效开始时间 */
      effectiveFrom?: string;
      /** 生效结束时间 */
      effectiveTo?: string;
      /** 关联设备ID列表 */
      deviceIds?: string[];
    }

    /** 状态变更请求 */
    interface TaskStatusChangeRequest {
      /** 新状态 */
      newStatus: TaskStatus;
    }

    /** 可用设备列表项 */
    interface AvailableDevice {
      /** 设备ID */
      id: string;
      /** 设备名称 */
      deviceName: string;
      /** 设备ID标识 */
      deviceId: string;
      /** 是否已关联 */
      isAssigned: boolean;
    }
  }
}
