import { transformRecordToOption } from '@/utils/common';

export const enableStatusRecord: Record<Api.Common.EnableStatus, App.I18n.I18nKey> = {
  '1': 'page.manage.common.status.enable',
  '2': 'page.manage.common.status.disable'
};

export const enableStatusOptions = transformRecordToOption(enableStatusRecord);

export const userGenderRecord: Record<Api.SystemManage.UserGender, App.I18n.I18nKey> = {
  '1': 'page.manage.user.gender.male',
  '2': 'page.manage.user.gender.female'
};

export const userGenderOptions = transformRecordToOption(userGenderRecord);

export const userTypeRecord: Record<Api.SystemManage.UserType, App.I18n.I18nKey> = {
  user: 'page.manage.user.userType.user',
  service: 'page.manage.user.userType.service'
};

export const userTypeOptions = transformRecordToOption(userTypeRecord);

export const menuTypeRecord: Record<Api.SystemManage.MenuType, App.I18n.I18nKey> = {
  '1': 'page.manage.menu.type.directory',
  '2': 'page.manage.menu.type.menu'
};

export const menuTypeOptions = transformRecordToOption(menuTypeRecord);

export const menuIconTypeRecord: Record<Api.SystemManage.IconType, App.I18n.I18nKey> = {
  '1': 'page.manage.menu.iconType.iconify',
  '2': 'page.manage.menu.iconType.local'
};

export const menuIconTypeOptions = transformRecordToOption(menuIconTypeRecord);

export const protocolTypeRecord: Record<Api.Device.ProtocolType, App.I18n.I18nKey> = {
  '1': 'page.device.protocolType.modbusTcp',
  '2': 'page.device.protocolType.modbusRtu',
  '3': 'page.device.protocolType.opcUa',
  '4': 'page.device.protocolType.opcDa',
  '5': 'page.device.protocolType.s7'
};

export const protocolTypeOptions = transformRecordToOption(protocolTypeRecord);

export const connectionStatusRecord: Record<Api.Device.ConnectionStatus, App.I18n.I18nKey> = {
  '1': 'page.device.connectionStatus.connected',
  '2': 'page.device.connectionStatus.disconnected',
  '3': 'page.device.connectionStatus.error',
  '99': 'page.device.connectionStatus.unknown'
};

export const connectionStatusOptions = transformRecordToOption(connectionStatusRecord);

/** 后端协议类型字符串到前端代码的映射 */
export const protocolTypeBackendToFrontendMap: Record<string, Api.Device.ProtocolType> = {
  MODBUS_TCP: '1',
  MODBUS_RTU: '2',
  OPC_UA: '3',
  OPC_DA: '4',
  S7: '5'
};

/** 前端协议类型代码到后端字符串的映射 */
export const protocolTypeFrontendToBackendMap: Record<Api.Device.ProtocolType, string> = {
  '1': 'MODBUS_TCP',
  '2': 'MODBUS_RTU',
  '3': 'OPC_UA',
  '4': 'OPC_DA',
  '5': 'S7'
};

// ==================== Tag Constants ====================

export const dataTypeRecord: Record<Api.Device.DataType, App.I18n.I18nKey> = {
  '1': 'page.tag.dataType.int16',
  '2': 'page.tag.dataType.int32',
  '3': 'page.tag.dataType.int64',
  '4': 'page.tag.dataType.uint16',
  '5': 'page.tag.dataType.uint32',
  '6': 'page.tag.dataType.uint64',
  '7': 'page.tag.dataType.float',
  '8': 'page.tag.dataType.double',
  '9': 'page.tag.dataType.boolean',
  '10': 'page.tag.dataType.string'
};

export const dataTypeOptions = transformRecordToOption(dataTypeRecord);

export const accessModeRecord: Record<Api.Device.AccessMode, App.I18n.I18nKey> = {
  '1': 'page.tag.accessMode.readOnly',
  '2': 'page.tag.accessMode.writeOnly',
  '3': 'page.tag.accessMode.readWrite'
};

export const accessModeOptions = transformRecordToOption(accessModeRecord);

/** 后端数据类型字符串到前端代码的映射 */
export const dataTypeBackendToFrontendMap: Record<string, Api.Device.DataType> = {
  Int16: '1',
  Int32: '2',
  Int64: '3',
  UInt16: '4',
  UInt32: '5',
  UInt64: '6',
  Float: '7',
  Double: '8',
  Boolean: '9',
  String: '10'
};

/** 前端数据类型代码到后端字符串的映射 */
export const dataTypeFrontendToBackendMap: Record<Api.Device.DataType, string> = {
  '1': 'Int16',
  '2': 'Int32',
  '3': 'Int64',
  '4': 'UInt16',
  '5': 'UInt32',
  '6': 'UInt64',
  '7': 'Float',
  '8': 'Double',
  '9': 'Boolean',
  '10': 'String'
};

/** 后端访问模式字符串到前端代码的映射 */
export const accessModeBackendToFrontendMap: Record<string, Api.Device.AccessMode> = {
  ReadOnly: '1',
  WriteOnly: '2',
  ReadWrite: '3'
};

/** 前端访问模式代码到后端字符串的映射 */
export const accessModeFrontendToBackendMap: Record<Api.Device.AccessMode, string> = {
  '1': 'ReadOnly',
  '2': 'WriteOnly',
  '3': 'ReadWrite'
};

/** Modbus 功能码选项 */
export const modbusFunctionCodeOptions = [
  { value: '01', label: '01 - 线圈状态 (Coils)' },
  { value: '02', label: '02 - 离散输入 (Discrete Inputs)' },
  { value: '03', label: '03 - 保持寄存器 (Holding Registers)' },
  { value: '04', label: '04 - 输入寄存器 (Input Registers)' }
];

/** S7 区域选项 */
export const s7AreaOptions = [
  { value: 'DB', label: 'DB - 数据块' },
  { value: 'M', label: 'M - 标志位' },
  { value: 'I', label: 'I - 输入' },
  { value: 'Q', label: 'Q - 输出' },
  { value: 'T', label: 'T - 定时器' },
  { value: 'C', label: 'C - 计数器' }
];

// ==================== Edge Node Constants ====================

export const nodeStatusRecord: Record<Api.EdgeNode.NodeStatus, App.I18n.I18nKey> = {
  Online: 'page.edgeNode.nodeStatus.online',
  Offline: 'page.edgeNode.nodeStatus.offline',
  Error: 'page.edgeNode.nodeStatus.error'
};

export const nodeStatusOptions = transformRecordToOption(nodeStatusRecord);

export const platformTypeRecord: Record<Api.EdgeNode.PlatformType, App.I18n.I18nKey> = {
  'NET8.0': 'page.edgeNode.platformType.net80',
  NET45: 'page.edgeNode.platformType.net45'
};

export const platformTypeOptions = transformRecordToOption(platformTypeRecord);

export const registrationTypeRecord: Record<Api.EdgeNode.RegistrationType, App.I18n.I18nKey> = {
  auto: 'page.edgeNode.registrationTypeOptions.auto',
  manual: 'page.edgeNode.registrationTypeOptions.manual'
};

export const registrationTypeOptions = transformRecordToOption(registrationTypeRecord);

// ==================== Collection Task Constants ====================

export const taskTypeRecord: Record<Api.CollectionTask.TaskType, App.I18n.I18nKey> = {
  Periodic: 'page.collectionTask.taskTypeOptions.periodic',
  Scheduled: 'page.collectionTask.taskTypeOptions.scheduled',
  EventDriven: 'page.collectionTask.taskTypeOptions.eventDriven',
  Hybrid: 'page.collectionTask.taskTypeOptions.hybrid'
};

export const taskTypeOptions = transformRecordToOption(taskTypeRecord);

export const taskStatusRecord: Record<Api.CollectionTask.TaskStatus, App.I18n.I18nKey> = {
  Draft: 'page.collectionTask.taskStatusOptions.draft',
  Active: 'page.collectionTask.taskStatusOptions.active',
  Paused: 'page.collectionTask.taskStatusOptions.paused',
  Stopped: 'page.collectionTask.taskStatusOptions.stopped'
};

export const taskStatusOptions = transformRecordToOption(taskStatusRecord);
