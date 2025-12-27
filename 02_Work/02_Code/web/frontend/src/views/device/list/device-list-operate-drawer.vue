<script setup lang="tsx">
import { computed, ref, watch } from 'vue';
import { enableStatusOptions, protocolTypeOptions } from '@/constants/business';
import {
  fetchAddDevice,
  fetchGetDeviceGroupTree,
  fetchGetEdgeNodeDropdownForDevice,
  fetchUpdateDevice
} from '@/service/api';
import { useForm, useFormRules } from '@/hooks/common/form';
import { $t } from '@/locales';

defineOptions({ name: 'DeviceOperateDrawer' });

interface Props {
  /** the type of operation */
  operateType: UI.TableOperateType;
  /** the edit row data */
  rowData?: Api.Device.Device | null;
}

const props = defineProps<Props>();

interface Emits {
  (e: 'submitted'): void;
}

const emit = defineEmits<Emits>();

const visible = defineModel<boolean>('visible', {
  default: false
});

const { formRef, validate, restoreValidation } = useForm();
const { defaultRequiredRule } = useFormRules();

const title = computed(() => {
  const titles: Record<UI.TableOperateType, string> = {
    add: $t('page.device.addDevice'),
    edit: $t('page.device.editDevice')
  };
  return titles[props.operateType];
});

// ============ 协议配置类型定义 ============

/** Modbus TCP 协议配置 */
interface ModbusTcpConfig {
  ip: string;
  port: number;
  unitId: number; // 从站地址 1-247
  pollingInterval: number; // 轮询间隔(ms)
}

/** Modbus RTU 协议配置 */
interface ModbusRtuConfig {
  serialPort: string; // 串口名称 如 COM1
  baudRate: number; // 波特率
  dataBits: number; // 数据位
  stopBits: number; // 停止位
  parity: string; // 校验位 None/Odd/Even
  slaveId: number; // 从站地址
  frameInterval: number; // 帧间隔(ms)
}

/** OPC UA 协议配置 */
interface OpcUaConfig {
  serverUrl: string; // 服务器URL
  securityMode: string; // None/Sign/SignAndEncrypt
  securityPolicy: string; // None/Basic256/Basic256Sha256
  authenticationMode: string; // Anonymous/UserName/Certificate
  samplingInterval: number; // 采样间隔(ms)
}

/** OPC DA 协议配置 */
interface OpcDaConfig {
  serverName: string; // 服务器名称
  clsid: string; // CLSID (可选)
  updateRate: number; // 更新频率(ms)
}

/** S7 协议配置 */
interface S7Config {
  ip: string;
  rack: number; // 机架号 0-7
  slot: number; // 槽号 0-31
  cpuType: string; // S7-200/S7-300/S7-400/S7-1200/S7-1500
}

// ============ 表单Model定义 ============

type Model = Pick<
  Api.Device.Device,
  'deviceName' | 'deviceId' | 'description' | 'protocolType' | 'edgeNodeId' | 'location' | 'enabled' | 'groupId'
> & {
  // 通用连接配置
  connectionTimeout: number;
  connectionRetryCount: number;

  // Modbus TCP 配置
  modbusTcp: ModbusTcpConfig;

  // Modbus RTU 配置
  modbusRtu: ModbusRtuConfig;

  // OPC UA 配置
  opcUa: OpcUaConfig;

  // OPC DA 配置
  opcDa: OpcDaConfig;

  // S7 配置
  s7: S7Config;
};

const model = ref(createDefaultModel());

function createDefaultModel(): Model {
  return {
    deviceName: '',
    deviceId: '',
    description: '',
    protocolType: '1' as Api.Device.ProtocolType,
    edgeNodeId: '',
    groupId: '',
    location: '',
    enabled: true,

    // 通用连接配置默认值
    connectionTimeout: 5000,
    connectionRetryCount: 3,

    // Modbus TCP 默认值
    modbusTcp: {
      ip: '',
      port: 502,
      unitId: 1,
      pollingInterval: 1000
    },

    // Modbus RTU 默认值
    modbusRtu: {
      serialPort: 'COM1',
      baudRate: 9600,
      dataBits: 8,
      stopBits: 1,
      parity: 'None',
      slaveId: 1,
      frameInterval: 50
    },

    // OPC UA 默认值
    opcUa: {
      serverUrl: 'opc.tcp://localhost:4840',
      securityMode: 'None',
      securityPolicy: 'None',
      authenticationMode: 'Anonymous',
      samplingInterval: 1000
    },

    // OPC DA 默认值
    opcDa: {
      serverName: '',
      clsid: '',
      updateRate: 1000
    },

    // S7 默认值
    s7: {
      ip: '',
      rack: 0,
      slot: 1,
      cpuType: 'S7-1200'
    }
  };
}

// ============ 协议类型判断 ============

const isModbusTcp = computed(() => model.value.protocolType === '1');
const isModbusRtu = computed(() => model.value.protocolType === '2');
const isOpcUa = computed(() => model.value.protocolType === '3');
const isOpcDa = computed(() => model.value.protocolType === '4');
const isS7 = computed(() => model.value.protocolType === '5');

// ============ 表单验证规则 ============

type RuleKey = 'deviceName' | 'deviceId' | 'protocolType';

const rules = computed<Record<RuleKey, App.Global.FormRule>>(() => {
  return {
    deviceName: defaultRequiredRule,
    deviceId: defaultRequiredRule,
    protocolType: defaultRequiredRule
  };
});

// ============ 下拉选项 ============

/** the enabled edge node options */
const edgeNodeOptions = ref<CommonType.Option<string>[]>([]);

/** the device group tree options */
const deviceGroupTreeOptions = ref<Api.Device.DeviceGroupTreeNode[]>([]);

/** 波特率选项 */
const baudRateOptions = [
  { label: '1200', value: 1200 },
  { label: '2400', value: 2400 },
  { label: '4800', value: 4800 },
  { label: '9600', value: 9600 },
  { label: '19200', value: 19200 },
  { label: '38400', value: 38400 },
  { label: '57600', value: 57600 },
  { label: '115200', value: 115200 }
];

/** 数据位选项 */
const dataBitsOptions = [
  { label: '7', value: 7 },
  { label: '8', value: 8 }
];

/** 停止位选项 */
const stopBitsOptions = [
  { label: '1', value: 1 },
  { label: '2', value: 2 }
];

/** 校验位选项 */
const parityOptions = [
  { label: 'None', value: 'None' },
  { label: 'Odd', value: 'Odd' },
  { label: 'Even', value: 'Even' }
];

/** S7 CPU类型选项 */
const s7CpuTypeOptions = [
  { label: 'S7-200', value: 'S7-200' },
  { label: 'S7-300', value: 'S7-300' },
  { label: 'S7-400', value: 'S7-400' },
  { label: 'S7-1200', value: 'S7-1200' },
  { label: 'S7-1500', value: 'S7-1500' }
];

/** OPC UA 安全模式选项 */
const securityModeOptions = [
  { label: 'None', value: 'None' },
  { label: 'Sign', value: 'Sign' },
  { label: 'SignAndEncrypt', value: 'SignAndEncrypt' }
];

/** OPC UA 安全策略选项 */
const securityPolicyOptions = [
  { label: 'None', value: 'None' },
  { label: 'Basic256', value: 'Basic256' },
  { label: 'Basic256Sha256', value: 'Basic256Sha256' }
];

/** OPC UA 认证方式选项 */
const authModeOptions = [
  { label: 'Anonymous', value: 'Anonymous' },
  { label: 'UserName', value: 'UserName' },
  { label: 'Certificate', value: 'Certificate' }
];

// ============ 数据加载 ============

async function getEdgeNodeOptions() {
  try {
    const { error, data } = await fetchGetEdgeNodeDropdownForDevice(true);

    if (!error) {
      const options = data.map(item => ({
        label: item.nodeName,
        value: item.id
      }));
      edgeNodeOptions.value = options;
    } else {
      // eslint-disable-next-line no-console
      console.warn($t('page.device.noEdgeNodeFound'));
      edgeNodeOptions.value = [];
    }
  } catch (err) {
    // eslint-disable-next-line no-console
    console.warn($t('page.device.noEdgeNodeFound'), err);
    edgeNodeOptions.value = [];
  }
}

async function getDeviceGroupTreeOptions() {
  try {
    const { error, data } = await fetchGetDeviceGroupTree(true);

    if (!error) {
      deviceGroupTreeOptions.value = data || [];
    } else {
      // eslint-disable-next-line no-console
      console.warn($t('page.device.noDeviceGroupFound'));
      deviceGroupTreeOptions.value = [];
    }
  } catch (err) {
    // eslint-disable-next-line no-console
    console.warn($t('page.device.noDeviceGroupFound'), err);
    deviceGroupTreeOptions.value = [];
  }
}

// ============ 协议配置解析/组装 ============

/** 解析 Modbus TCP 配置 */
function parseModbusTcpConfig(config: Record<string, unknown>): void {
  model.value.modbusTcp = {
    ip: (config.ip as string) || '',
    port: (config.port as number) || 502,
    unitId: (config.unitId as number) || 1,
    pollingInterval: (config.pollingInterval as number) || 1000
  };
}

/** 解析 Modbus RTU 配置 */
function parseModbusRtuConfig(config: Record<string, unknown>): void {
  model.value.modbusRtu = {
    serialPort: (config.serialPort as string) || 'COM1',
    baudRate: (config.baudRate as number) || 9600,
    dataBits: (config.dataBits as number) || 8,
    stopBits: (config.stopBits as number) || 1,
    parity: (config.parity as string) || 'None',
    slaveId: (config.slaveId as number) || 1,
    frameInterval: (config.frameInterval as number) || 50
  };
}

/** 解析 OPC UA 配置 */
function parseOpcUaConfig(config: Record<string, unknown>): void {
  model.value.opcUa = {
    serverUrl: (config.serverUrl as string) || 'opc.tcp://localhost:4840',
    securityMode: (config.securityMode as string) || 'None',
    securityPolicy: (config.securityPolicy as string) || 'None',
    authenticationMode: (config.authenticationMode as string) || 'Anonymous',
    samplingInterval: (config.samplingInterval as number) || 1000
  };
}

/** 解析 OPC DA 配置 */
function parseOpcDaConfig(config: Record<string, unknown>): void {
  model.value.opcDa = {
    serverName: (config.serverName as string) || '',
    clsid: (config.clsid as string) || '',
    updateRate: (config.updateRate as number) || 1000
  };
}

/** 解析 S7 配置 */
function parseS7Config(config: Record<string, unknown>): void {
  model.value.s7 = {
    ip: (config.ip as string) || '',
    rack: (config.rack as number) || 0,
    slot: (config.slot as number) || 1,
    cpuType: (config.cpuType as string) || 'S7-1200'
  };
}

/** 协议解析函数映射表 */
const protocolParsers: Record<string, (config: Record<string, unknown>) => void> = {
  '1': parseModbusTcpConfig,
  '2': parseModbusRtuConfig,
  '3': parseOpcUaConfig,
  '4': parseOpcDaConfig,
  '5': parseS7Config
};

/** 从 protocolConfig JSON 解析到对应的协议配置对象 */
function parseProtocolConfig(protocolType: string, config: Record<string, unknown>): void {
  const parser = protocolParsers[protocolType];
  if (parser) {
    parser(config);
  }
}

/** 根据协议类型组装 protocolConfig JSON */
function buildProtocolConfig(): Record<string, unknown> {
  const configMap: Record<string, Record<string, unknown>> = {
    '1': { ...model.value.modbusTcp },
    '2': { ...model.value.modbusRtu },
    '3': { ...model.value.opcUa },
    '4': { ...model.value.opcDa },
    '5': { ...model.value.s7 }
  };
  return configMap[model.value.protocolType] || {};
}

// ============ 表单初始化 ============

function handleInitModel() {
  model.value = createDefaultModel();

  if (props.operateType === 'edit' && props.rowData) {
    // 调试：输出原始数据
    // eslint-disable-next-line no-console
    console.log('Edit rowData:', props.rowData);
    // eslint-disable-next-line no-console
    console.log('connectionConfig type:', typeof props.rowData.connectionConfig, props.rowData.connectionConfig);
    // eslint-disable-next-line no-console
    console.log('protocolConfig type:', typeof props.rowData.protocolConfig, props.rowData.protocolConfig);

    // 解析配置（如果是字符串则需要 JSON.parse）
    let connectionConfig = props.rowData.connectionConfig || {};
    let protocolConfig = props.rowData.protocolConfig || {};

    if (typeof connectionConfig === 'string') {
      try {
        connectionConfig = JSON.parse(connectionConfig);
      } catch (e) {
        // eslint-disable-next-line no-console
        console.error('Failed to parse connectionConfig:', e);
        connectionConfig = {};
      }
    }

    if (typeof protocolConfig === 'string') {
      try {
        protocolConfig = JSON.parse(protocolConfig);
      } catch (e) {
        // eslint-disable-next-line no-console
        console.error('Failed to parse protocolConfig:', e);
        protocolConfig = {};
      }
    }

    // 后端字符串 -> 前端数字映射
    const protocolTypeToCode: Record<string, Api.Device.ProtocolType> = {
      MODBUS_TCP: '1',
      MODBUS_RTU: '2',
      OPC_UA: '3',
      OPC_DA: '4',
      S7: '5'
    };

    // 基础字段
    model.value.deviceName = props.rowData.deviceName;
    model.value.deviceId = props.rowData.deviceId;
    model.value.description = props.rowData.description || '';
    model.value.protocolType = protocolTypeToCode[props.rowData.protocolType] || '1'; // 转换协议类型
    model.value.edgeNodeId = props.rowData.edgeNodeId || '';
    model.value.groupId = props.rowData.groupId || '';
    model.value.location = props.rowData.location || '';
    model.value.enabled = props.rowData.enabled;

    // 通用连接配置
    model.value.connectionTimeout = (connectionConfig.timeout as number) || 5000;
    model.value.connectionRetryCount = (connectionConfig.retryCount as number) || 3;

    // 调试：输出解析后的配置
    // eslint-disable-next-line no-console
    console.log('Parsed connectionConfig:', connectionConfig);
    // eslint-disable-next-line no-console
    console.log('Parsed protocolConfig:', protocolConfig);
    // eslint-disable-next-line no-console
    console.log('Protocol Type Code:', model.value.protocolType);

    // 解析协议配置
    parseProtocolConfig(model.value.protocolType, protocolConfig);
  }
}

// ============ 表单提交 ============

function closeDrawer() {
  visible.value = false;
}

async function handleSubmit() {
  await validate();

  // 组装通用连接配置
  const connectionConfig = {
    timeout: model.value.connectionTimeout,
    retryCount: model.value.connectionRetryCount
  };

  // 组装协议配置
  const protocolConfig = buildProtocolConfig();

  // 协议类型映射：前端数字 -> 后端字符串
  const protocolTypeMap: Record<string, string> = {
    '1': 'MODBUS_TCP',
    '2': 'MODBUS_RTU',
    '3': 'OPC_UA',
    '4': 'OPC_DA',
    '5': 'S7'
  };

  const submitData: any = {
    deviceName: model.value.deviceName,
    deviceId: model.value.deviceId,
    deviceType: 'PLC', // 默认设备类型
    description: model.value.description || undefined,
    protocolType: protocolTypeMap[model.value.protocolType] || 'OPC_UA',
    connectionConfig: JSON.stringify(connectionConfig), // 序列化为JSON字符串
    protocolConfig: JSON.stringify(protocolConfig), // 序列化为JSON字符串
    tagsConfig: '[]', // 空数组的JSON字符串
    edgeNodeId: model.value.edgeNodeId || undefined,
    groupId: model.value.groupId || undefined,
    location: model.value.location || undefined,
    enabled: model.value.enabled
  };

  if (props.operateType === 'add') {
    const { error } = await fetchAddDevice(submitData);
    if (!error) {
      window.$message?.success($t('common.addSuccess'));
      closeDrawer();
      emit('submitted');
    }
  } else {
    if (!props.rowData?.id) {
      window.$message?.error($t('page.device.missingDeviceId'));
      return;
    }

    const { error } = await fetchUpdateDevice(props.rowData.id, submitData);

    if (!error) {
      window.$message?.success($t('common.updateSuccess'));
      closeDrawer();
      emit('submitted');
    }
  }
}

// ============ 监听器 ============

watch(visible, () => {
  if (visible.value) {
    handleInitModel();
    restoreValidation();
    getEdgeNodeOptions();
    getDeviceGroupTreeOptions();
  }
});
</script>

<template>
  <ElDrawer v-model="visible" :title="title" :size="520">
    <ElForm ref="formRef" :model="model" :rules="rules" label-position="top">
      <!-- 基础信息 -->
      <ElFormItem :label="$t('page.device.deviceName')" prop="deviceName">
        <ElInput v-model="model.deviceName" :placeholder="$t('page.device.form.deviceName')" />
      </ElFormItem>
      <ElFormItem :label="$t('page.device.deviceId')" prop="deviceId">
        <ElInput
          v-model="model.deviceId"
          :placeholder="$t('page.device.form.deviceId')"
          :disabled="operateType === 'edit'"
        />
      </ElFormItem>
      <ElFormItem :label="$t('page.device.description')" prop="description">
        <ElInput
          v-model="model.description"
          type="textarea"
          :placeholder="$t('page.device.form.description')"
          :rows="3"
        />
      </ElFormItem>
      <ElFormItem :label="$t('page.device.protocol')" prop="protocolType">
        <ElSelect v-model="model.protocolType" :placeholder="$t('page.device.form.protocol')">
          <ElOption v-for="item in protocolTypeOptions" :key="item.value" :label="$t(item.label)" :value="item.value" />
        </ElSelect>
      </ElFormItem>
      <ElFormItem :label="$t('page.device.deviceGroup')" prop="groupId">
        <ElTreeSelect
          v-model="model.groupId"
          :data="deviceGroupTreeOptions"
          :props="{ label: 'name', value: 'id', children: 'children' }"
          clearable
          check-strictly
          :placeholder="$t('page.device.form.deviceGroup')"
          :render-after-expand="false"
        />
      </ElFormItem>
      <ElFormItem :label="$t('page.device.edgeNode')" prop="edgeNodeId">
        <ElSelect v-model="model.edgeNodeId" clearable :placeholder="$t('page.device.form.edgeNode')">
          <ElOption v-for="{ label, value } in edgeNodeOptions" :key="value" :label="label" :value="value" />
        </ElSelect>
      </ElFormItem>
      <ElFormItem :label="$t('page.device.location')" prop="location">
        <ElInput v-model="model.location" :placeholder="$t('page.device.form.location')" />
      </ElFormItem>
      <ElFormItem :label="$t('page.device.enabled')" prop="enabled">
        <ElRadioGroup v-model="model.enabled">
          <ElRadio
            v-for="item in enableStatusOptions"
            :key="item.value"
            :value="item.value === '1'"
            :label="$t(item.label)"
          />
        </ElRadioGroup>
      </ElFormItem>

      <!-- 通用连接配置 -->
      <ElDivider content-position="left">{{ $t('page.device.connectionConfig') }}</ElDivider>

      <ElRow :gutter="16">
        <ElCol :span="12">
          <ElFormItem :label="$t('page.device.connectionForm.timeout')" prop="connectionTimeout">
            <ElInputNumber v-model="model.connectionTimeout" :min="100" :max="60000" class="w-full" />
          </ElFormItem>
        </ElCol>
        <ElCol :span="12">
          <ElFormItem :label="$t('page.device.connectionForm.retryCount')" prop="connectionRetryCount">
            <ElInputNumber v-model="model.connectionRetryCount" :min="0" :max="10" class="w-full" />
          </ElFormItem>
        </ElCol>
      </ElRow>

      <!-- 协议配置 - 根据协议类型动态显示 -->
      <ElDivider content-position="left">{{ $t('page.device.protocolConfig') }}</ElDivider>

      <!-- Modbus TCP 配置 -->
      <template v-if="isModbusTcp">
        <ElRow :gutter="16">
          <ElCol :span="16">
            <ElFormItem :label="$t('page.device.protocolForm.ip')" prop="modbusTcp.ip">
              <ElInput v-model="model.modbusTcp.ip" :placeholder="$t('page.device.protocolForm.ipPlaceholder')" />
            </ElFormItem>
          </ElCol>
          <ElCol :span="8">
            <ElFormItem :label="$t('page.device.protocolForm.port')" prop="modbusTcp.port">
              <ElInputNumber v-model="model.modbusTcp.port" :min="1" :max="65535" class="w-full" />
            </ElFormItem>
          </ElCol>
        </ElRow>
        <ElRow :gutter="16">
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.unitId')" prop="modbusTcp.unitId">
              <ElInputNumber v-model="model.modbusTcp.unitId" :min="1" :max="247" class="w-full" />
            </ElFormItem>
          </ElCol>
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.pollingInterval')" prop="modbusTcp.pollingInterval">
              <ElInputNumber v-model="model.modbusTcp.pollingInterval" :min="100" :max="60000" class="w-full" />
            </ElFormItem>
          </ElCol>
        </ElRow>
      </template>

      <!-- Modbus RTU 配置 -->
      <template v-if="isModbusRtu">
        <ElRow :gutter="16">
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.serialPort')" prop="modbusRtu.serialPort">
              <ElInput
                v-model="model.modbusRtu.serialPort"
                :placeholder="$t('page.device.protocolForm.serialPortPlaceholder')"
              />
            </ElFormItem>
          </ElCol>
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.baudRate')" prop="modbusRtu.baudRate">
              <ElSelect v-model="model.modbusRtu.baudRate" class="w-full">
                <ElOption v-for="opt in baudRateOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </ElSelect>
            </ElFormItem>
          </ElCol>
        </ElRow>
        <ElRow :gutter="16">
          <ElCol :span="8">
            <ElFormItem :label="$t('page.device.protocolForm.dataBits')" prop="modbusRtu.dataBits">
              <ElSelect v-model="model.modbusRtu.dataBits" class="w-full">
                <ElOption v-for="opt in dataBitsOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </ElSelect>
            </ElFormItem>
          </ElCol>
          <ElCol :span="8">
            <ElFormItem :label="$t('page.device.protocolForm.stopBits')" prop="modbusRtu.stopBits">
              <ElSelect v-model="model.modbusRtu.stopBits" class="w-full">
                <ElOption v-for="opt in stopBitsOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </ElSelect>
            </ElFormItem>
          </ElCol>
          <ElCol :span="8">
            <ElFormItem :label="$t('page.device.protocolForm.parity')" prop="modbusRtu.parity">
              <ElSelect v-model="model.modbusRtu.parity" class="w-full">
                <ElOption v-for="opt in parityOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </ElSelect>
            </ElFormItem>
          </ElCol>
        </ElRow>
        <ElRow :gutter="16">
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.slaveId')" prop="modbusRtu.slaveId">
              <ElInputNumber v-model="model.modbusRtu.slaveId" :min="1" :max="247" class="w-full" />
            </ElFormItem>
          </ElCol>
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.frameInterval')" prop="modbusRtu.frameInterval">
              <ElInputNumber v-model="model.modbusRtu.frameInterval" :min="0" :max="1000" class="w-full" />
            </ElFormItem>
          </ElCol>
        </ElRow>
      </template>

      <!-- OPC UA 配置 -->
      <template v-if="isOpcUa">
        <ElFormItem :label="$t('page.device.protocolForm.serverUrl')" prop="opcUa.serverUrl">
          <ElInput v-model="model.opcUa.serverUrl" :placeholder="$t('page.device.protocolForm.serverUrlPlaceholder')" />
        </ElFormItem>
        <ElRow :gutter="16">
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.securityMode')" prop="opcUa.securityMode">
              <ElSelect v-model="model.opcUa.securityMode" class="w-full">
                <ElOption v-for="opt in securityModeOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </ElSelect>
            </ElFormItem>
          </ElCol>
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.securityPolicy')" prop="opcUa.securityPolicy">
              <ElSelect v-model="model.opcUa.securityPolicy" class="w-full">
                <ElOption v-for="opt in securityPolicyOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </ElSelect>
            </ElFormItem>
          </ElCol>
        </ElRow>
        <ElRow :gutter="16">
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.authenticationMode')" prop="opcUa.authenticationMode">
              <ElSelect v-model="model.opcUa.authenticationMode" class="w-full">
                <ElOption v-for="opt in authModeOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </ElSelect>
            </ElFormItem>
          </ElCol>
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.samplingInterval')" prop="opcUa.samplingInterval">
              <ElInputNumber v-model="model.opcUa.samplingInterval" :min="100" :max="60000" class="w-full" />
            </ElFormItem>
          </ElCol>
        </ElRow>
      </template>

      <!-- OPC DA 配置 -->
      <template v-if="isOpcDa">
        <ElFormItem :label="$t('page.device.protocolForm.serverName')" prop="opcDa.serverName">
          <ElInput
            v-model="model.opcDa.serverName"
            :placeholder="$t('page.device.protocolForm.serverNamePlaceholder')"
          />
        </ElFormItem>
        <ElRow :gutter="16">
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.clsid')" prop="opcDa.clsid">
              <ElInput v-model="model.opcDa.clsid" :placeholder="$t('page.device.protocolForm.clsidPlaceholder')" />
            </ElFormItem>
          </ElCol>
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.updateRate')" prop="opcDa.updateRate">
              <ElInputNumber v-model="model.opcDa.updateRate" :min="100" :max="60000" class="w-full" />
            </ElFormItem>
          </ElCol>
        </ElRow>
      </template>

      <!-- S7 配置 -->
      <template v-if="isS7">
        <ElRow :gutter="16">
          <ElCol :span="16">
            <ElFormItem :label="$t('page.device.protocolForm.ip')" prop="s7.ip">
              <ElInput v-model="model.s7.ip" :placeholder="$t('page.device.protocolForm.ipPlaceholder')" />
            </ElFormItem>
          </ElCol>
          <ElCol :span="8">
            <ElFormItem :label="$t('page.device.protocolForm.cpuType')" prop="s7.cpuType">
              <ElSelect v-model="model.s7.cpuType" class="w-full">
                <ElOption v-for="opt in s7CpuTypeOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
              </ElSelect>
            </ElFormItem>
          </ElCol>
        </ElRow>
        <ElRow :gutter="16">
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.rack')" prop="s7.rack">
              <ElInputNumber v-model="model.s7.rack" :min="0" :max="7" class="w-full" />
            </ElFormItem>
          </ElCol>
          <ElCol :span="12">
            <ElFormItem :label="$t('page.device.protocolForm.slot')" prop="s7.slot">
              <ElInputNumber v-model="model.s7.slot" :min="0" :max="31" class="w-full" />
            </ElFormItem>
          </ElCol>
        </ElRow>
      </template>
    </ElForm>
    <template #footer>
      <ElSpace :size="16">
        <ElButton @click="closeDrawer">{{ $t('common.cancel') }}</ElButton>
        <ElButton type="primary" @click="handleSubmit">{{ $t('common.confirm') }}</ElButton>
      </ElSpace>
    </template>
  </ElDrawer>
</template>

<style scoped></style>
