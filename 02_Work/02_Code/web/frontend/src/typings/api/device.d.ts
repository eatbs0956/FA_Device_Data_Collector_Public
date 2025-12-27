declare namespace Api {
  /**
   * namespace Device
   *
   * backend api module: "device"
   */
  namespace Device {
    type CommonSearchParams = Pick<Common.PaginatingCommonParams, 'current' | 'size'>;

    /**
     * protocol type
     *
     * - "1": "Modbus TCP"
     * - "2": "Modbus RTU"
     * - "3": "OPC UA"
     * - "4": "OPC DA"
     * - "5": "S7"
     */
    type ProtocolType = '1' | '2' | '3' | '4' | '5';

    /**
     * connection status
     *
     * - "1": "Connected"
     * - "2": "Disconnected"
     * - "3": "Error"
     * - "99": "Unknown"
     */
    type ConnectionStatus = '1' | '2' | '3' | '99';

    /** edge node */
    type EdgeNode = {
      /** edge node id */
      id: string;
      /** edge node name */
      nodeName: string;
      /** edge node code */
      nodeCode: string;
      /** edge node description */
      description?: string;
      /** ip address */
      ipAddress?: string;
      /** operating system */
      os?: string;
      /** status */
      status: Common.EnableStatus | undefined;
      /** created by user id - audit field */
      createdBy?: string;
      /** updated by user id - audit field */
      updatedBy?: string;
      /** created at timestamp - audit field */
      createdAt?: string;
      /** updated at timestamp - audit field */
      updatedAt?: string;
      /** deleted flag - audit field */
      deletedFlag?: boolean;
      /** tenant id - audit field */
      tenantId?: string;
    };

    /** device */
    type Device = {
      /** device id */
      id: string;
      /** device name */
      deviceName: string;
      /** device unique identifier */
      deviceId: string;
      /** device description */
      description?: string;
      /** protocol type */
      protocolType: ProtocolType;
      /** connection configuration (JSONB) */
      connectionConfig: Record<string, any>;
      /** protocol-specific configuration (JSONB) */
      protocolConfig: Record<string, any>;
      /** tags configuration (JSONB) */
      tagsConfig: Record<string, any>;
      /** number of configured tags */
      tagCount: number;
      /** edge node id (optional) */
      edgeNodeId?: string;
      /** edge node info */
      edgeNode?: EdgeNode;
      /** device location */
      location?: string;
      /** device group id (optional) */
      groupId?: string;
      /** enabled status */
      enabled: boolean;
      /** connection status */
      connectionStatus: ConnectionStatus;
      /** last connection time */
      lastConnectedAt?: string;
      /** created by user id - audit field */
      createdBy?: string;
      /** updated by user id - audit field */
      updatedBy?: string;
      /** created at timestamp - audit field */
      createdAt?: string;
      /** updated at timestamp - audit field */
      updatedAt?: string;
      /** deleted flag - audit field */
      deletedFlag?: boolean;
      /** tenant id - audit field */
      tenantId?: string;
    };

    /** device edit */
    type DeviceEdit = {
      /** device name */
      deviceName: string;
      /** device unique identifier */
      deviceId: string;
      /** device description */
      description?: string;
      /** protocol type - number for API */
      protocolType: number;
      /** connection configuration (JSONB) */
      connectionConfig: Record<string, any>;
      /** protocol-specific configuration (JSONB) */
      protocolConfig: Record<string, any>;
      /** tags configuration (JSONB) */
      tagsConfig: Record<string, any>;
      /** edge node id (optional) */
      edgeNodeId?: string;
      /** device location */
      location?: string;
      /** device group id (optional) */
      groupId?: string;
      /** enabled status */
      enabled?: boolean;
    };

    /** device search params */
    type DeviceSearchParams = CommonType.RecordNullable<
      {
        /** device name */
        deviceName?: string;
        /** device unique identifier */
        deviceId?: string;
        /** protocol type - backend expects string like 'MODBUS_TCP' */
        protocolType?: string;
        /** connection status - backend expects string like 'CONNECTED' */
        connectionStatus?: string;
        /** edge node id */
        edgeNodeId?: string;
        /** enabled status */
        enabled?: boolean;
      } & CommonSearchParams
    >;

    /** device list */
    type DeviceList = Common.PaginatingQueryRecord<Device>;

    /** connection test result */
    type ConnectionTestResult = {
      /** test success status */
      success: boolean;
      /** test message */
      message: string;
      /** connection latency in milliseconds */
      latencyMs?: number;
      /** tested at timestamp */
      testedAt: string;
    };

    /** device group tree node (for dropdown selection) */
    type DeviceGroupTreeNode = {
      /** group id */
      id: string;
      /** group name */
      name: string;
      /** parent group id */
      parentId?: string;
      /** tree level (0 = root) */
      level: number;
      /** sort order */
      sortOrder: number;
      /** children groups */
      children?: DeviceGroupTreeNode[];
    };

    /** device group (full info) */
    type DeviceGroup = {
      /** group id */
      id: string;
      /** group name */
      name: string;
      /** parent group id */
      parentId?: string;
      /** tree level (0 = root) */
      level: number;
      /** sort order */
      sortOrder: number;
      /** group description */
      description?: string;
      /** device count in this group */
      deviceCount: number;
      /** child group count */
      childCount?: number;
      /** children groups */
      children?: DeviceGroup[];
      /** created by user id */
      createdBy?: string;
      /** updated by user id */
      updatedBy?: string;
      /** created at timestamp */
      createdAt?: string;
      /** updated at timestamp */
      updatedAt?: string;
    };

    /** device group search params */
    type DeviceGroupSearchParams = CommonType.RecordNullable<{
      /** page number */
      current: number;
      /** page size */
      size: number;
      /** parent group id */
      parentId?: string;
      /** group name (for search) */
      name?: string;
      /** whether to include all groups */
      includeAll?: boolean;
    }>;

    /** device group list response */
    type DeviceGroupList = Api.Common.PaginatingQueryRecord<DeviceGroup>;

    /** device group edit */
    type DeviceGroupEdit = {
      /** group name */
      name: string;
      /** parent group id */
      parentId?: string;
      /** sort order */
      sortOrder?: number;
      /** group description */
      description?: string;
    };

    // ==================== Tag Types ====================

    /**
     * data type
     *
     * - "1": "Int16"
     * - "2": "Int32"
     * - "3": "Int64"
     * - "4": "UInt16"
     * - "5": "UInt32"
     * - "6": "UInt64"
     * - "7": "Float"
     * - "8": "Double"
     * - "9": "Boolean"
     * - "10": "String"
     */
    type DataType = '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9' | '10';

    /**
     * access mode
     *
     * - "1": "ReadOnly"
     * - "2": "WriteOnly"
     * - "3": "ReadWrite"
     */
    type AccessMode = '1' | '2' | '3';

    /** Modbus address configuration */
    type ModbusAddressConfig = {
      /** function code: 01=coils, 02=discrete inputs, 03=holding registers, 04=input registers */
      functionCode: string;
      /** register address */
      address: number;
      /** slave/unit id */
      slaveId: number;
      /** number of registers to read */
      quantity: number;
    };

    /** OPC UA address configuration */
    type OpcUaAddressConfig = {
      /** node id (e.g., ns=2;s=Channel1.Device1.Tag1) */
      nodeId: string;
      /** namespace index */
      namespaceIndex: number;
    };

    /** OPC DA address configuration */
    type OpcDaAddressConfig = {
      /** item id (e.g., Channel1.Device1.Tag1) */
      itemId: string;
    };

    /** S7 address configuration */
    type S7AddressConfig = {
      /** area: DB, M, I, Q, T, C */
      area: string;
      /** DB number (only for DB area) */
      dbNumber?: number;
      /** byte offset */
      offset: number;
      /** bit offset (for boolean types) */
      bitOffset?: number;
    };

    /** tag address configuration (union type based on protocol) */
    type TagAddressConfig = ModbusAddressConfig | OpcUaAddressConfig | OpcDaAddressConfig | S7AddressConfig;

    /** tag definition */
    type Tag = {
      /** tag id (primary key) */
      id: string;
      /** tag unique identifier */
      tagId: string;
      /** device id (foreign key) */
      deviceId: string;
      /** device name */
      deviceName: string;
      /** protocol type of the device */
      protocolType: string;
      /** tag name */
      tagName: string;
      /** tag address (JSON string) */
      tagAddress: string;
      /** data type */
      dataType: string;
      /** unit */
      unit?: string;
      /** description */
      description?: string;
      /** enabled status */
      enabled: boolean;
      /** minimum value */
      minValue?: number;
      /** maximum value */
      maxValue?: number;
      /** scaling factor */
      scalingFactor: number;
      /** offset */
      offset: number;
      /** access mode */
      accessMode: string;
      /** deadband */
      deadband: number;
      /** tenant id */
      tenantId?: string;
      /** created by user id */
      createdBy?: string;
      /** created at timestamp */
      createdAt?: string;
      /** updated by user id */
      updatedBy?: string;
      /** updated at timestamp */
      updatedAt?: string;
    };

    /** tag edit model for create/update */
    type TagEdit = {
      /** tag unique identifier */
      tagId: string;
      /** device id */
      deviceId: string;
      /** tag name */
      tagName: string;
      /** tag address (JSON string) */
      tagAddress: string;
      /** data type */
      dataType: string;
      /** unit */
      unit?: string;
      /** description */
      description?: string;
      /** enabled status */
      enabled: boolean;
      /** minimum value */
      minValue?: number;
      /** maximum value */
      maxValue?: number;
      /** scaling factor */
      scalingFactor: number;
      /** offset */
      offset: number;
      /** access mode */
      accessMode: string;
      /** deadband */
      deadband: number;
    };

    /** tag search params */
    type TagSearchParams = CommonType.RecordNullable<
      {
        /** device id */
        deviceId?: string;
        /** tag name (fuzzy search) */
        tagName?: string;
        /** tag id (exact search) */
        tagId?: string;
        /** enabled status */
        enabled?: boolean;
        /** data type */
        dataType?: string;
      } & CommonSearchParams
    >;

    /** tag list */
    type TagList = Common.PaginatingQueryRecord<Tag>;

    /** device list item for tree (simplified) */
    type DeviceTreeItem = {
      /** device id */
      id: string;
      /** device name */
      deviceName: string;
      /** device unique identifier */
      deviceId: string;
      /** protocol type */
      protocolType: string;
      /** tag count */
      tagCount: number;
      /** enabled status */
      enabled: boolean;
      /** connection status */
      connectionStatus: string;
    };
  }
}
