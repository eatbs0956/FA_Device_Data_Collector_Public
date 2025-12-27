/**
 * Edge Node (采集节点) 相关类型定义
 */
declare namespace Api {
  namespace EdgeNode {
    /** 平台类型 */
    type PlatformType = 'NET8.0' | 'NET45';

    /** 节点状态 */
    type NodeStatus = 'Online' | 'Offline' | 'Error';

    /** 注册类型 */
    type RegistrationType = 'auto' | 'manual';

    /** 边缘节点 */
    interface EdgeNode {
      /** 主键ID */
      id: string;
      /** 节点标识符 */
      nodeId: string;
      /** 节点名称 */
      nodeName: string;
      /** 平台类型 */
      platform: PlatformType;
      /** 版本号 */
      version: string;
      /** 部署位置 */
      location: string | null;
      /** IP地址 */
      ipAddress: string | null;
      /** 端口 */
      port: number | null;
      /** 状态 */
      status: NodeStatus;
      /** 平台配置（JSON字符串） */
      platformConfig: string | null;
      /** 资源限制（JSON字符串） */
      resourceLimits: string | null;
      /** 操作系统信息 */
      osInfo: string | null;
      /** 硬件信息（JSON字符串） */
      hardwareInfo: string | null;
      /** 安装路径 */
      installPath: string | null;
      /** 最后心跳时间 */
      lastHeartbeat: string | null;
      /** 注册类型 */
      registrationType: RegistrationType;
      /** 关联设备数量 */
      deviceCount: number;
      /** 创建时间 */
      createdAt: string;
      /** 更新时间 */
      updatedAt: string | null;
    }

    /** 边缘节点列表响应 */
    interface EdgeNodeList {
      /** 节点列表 */
      records: EdgeNode[];
      /** 总数 */
      total: number;
      /** 当前页码 */
      current: number;
      /** 每页大小 */
      size: number;
    }

    /** 边缘节点查询参数 */
    interface EdgeNodeSearchParams {
      /** 当前页码 */
      current?: number;
      /** 每页大小 */
      size?: number;
      /** 节点ID */
      nodeId?: string;
      /** 节点名称 */
      nodeName?: string;
      /** 平台类型 */
      platform?: string;
      /** 状态 */
      status?: string;
    }

    /** 创建边缘节点请求 */
    interface CreateEdgeNodeRequest {
      /** 节点标识符 */
      nodeId: string;
      /** 节点名称 */
      nodeName: string;
      /** 平台类型 */
      platform: PlatformType;
      /** 版本号 */
      version?: string;
      /** 部署位置 */
      location?: string;
      /** IP地址 */
      ipAddress?: string;
      /** 端口 */
      port?: number;
      /** 平台配置 */
      platformConfig?: string;
      /** 资源限制 */
      resourceLimits?: string;
      /** 操作系统信息 */
      osInfo?: string;
      /** 硬件信息 */
      hardwareInfo?: string;
      /** 安装路径 */
      installPath?: string;
    }

    /** 更新边缘节点请求 */
    interface UpdateEdgeNodeRequest {
      /** 节点名称（始终可编辑） */
      nodeName: string;
      /** 部署位置（始终可编辑） */
      location?: string;
      /** 资源限制（始终可编辑） */
      resourceLimits?: string;
      /** 以下字段仅手动添加且未连接时可编辑 */
      /** 平台类型 */
      platform?: PlatformType;
      /** 版本号 */
      version?: string;
      /** IP地址 */
      ipAddress?: string;
      /** 端口 */
      port?: number;
      /** 操作系统信息 */
      osInfo?: string;
      /** 硬件信息 */
      hardwareInfo?: string;
      /** 安装路径 */
      installPath?: string;
    }

    /** 下拉选项 */
    interface EdgeNodeDropdownItem {
      id: string;
      nodeName: string;
      nodeId: string;
      status: string;
      platform: string;
    }
  }
}
