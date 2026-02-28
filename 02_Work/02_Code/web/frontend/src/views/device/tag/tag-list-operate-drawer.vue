<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import type { FormInstance, FormRules } from 'element-plus';
import { ElMessage } from 'element-plus';
import {
  accessModeFrontendToBackendMap,
  accessModeOptions,
  dataTypeFrontendToBackendMap,
  dataTypeOptions,
  protocolTypeBackendToFrontendMap
} from '@/constants/business';
import { fetchAddTag, fetchUpdateTag } from '@/service/api';
import { $t } from '@/locales';
import ModbusAddressInput from './components/modbus-address-input.vue';
import OpcUaAddressInput from './components/opc-ua-address-input.vue';
import OpcDaAddressInput from './components/opc-da-address-input.vue';
import S7AddressInput from './components/s7-address-input.vue';

defineOptions({
  name: 'TagListOperateDrawer'
});

export type OperateType = 'add' | 'edit';

interface Props {
  operateType?: OperateType;
  rowData?: Api.Device.Tag | null;
  device?: Api.Device.Device | null;
}

const props = withDefaults(defineProps<Props>(), {
  operateType: 'add',
  rowData: null,
  device: null
});

const visible = defineModel<boolean>('visible', { default: false });

const emit = defineEmits<{
  submitted: [];
}>();

const formRef = ref<FormInstance>();
const submitting = ref(false);

// 表单数据
const formModel = reactive<Api.Device.TagEdit>({
  tagId: '',
  deviceId: '',
  tagName: '',
  tagAddress: '{}',
  dataType: 'Float',
  unit: '',
  description: '',
  enabled: true,
  enableRealtime: false,
  minValue: undefined,
  maxValue: undefined,
  scalingFactor: 1,
  offset: 0,
  accessMode: 'ReadOnly',
  deadband: 0
});

// 地址配置（用于各协议组件）
const addressConfig = ref<Record<string, any>>({});

// 标题
const title = computed(() => {
  return props.operateType === 'add' ? $t('page.tag.addTag') : $t('page.tag.editTag');
});

// 当前协议类型（将后端字符串转换为前端代码）
const protocolType = computed(() => {
  const backendType = props.device?.protocolType || '';
  return protocolTypeBackendToFrontendMap[backendType] || '';
});

// 表单验证规则
const rules = reactive<FormRules>({
  tagId: [{ required: true, message: $t('page.tag.tagIdRequired'), trigger: 'blur' }],
  tagName: [{ required: true, message: $t('page.tag.tagNameRequired'), trigger: 'blur' }],
  dataType: [{ required: true, message: $t('page.tag.dataTypeRequired'), trigger: 'change' }]
});

// 初始化表单
function initForm() {
  if (props.operateType === 'edit' && props.rowData) {
    // 编辑模式
    const row = props.rowData;
    Object.assign(formModel, {
      tagId: row.tagId,
      deviceId: row.deviceId,
      tagName: row.tagName,
      tagAddress: row.tagAddress,
      dataType: row.dataType,
      unit: row.unit || '',
      description: row.description || '',
      enabled: row.enabled,
      enableRealtime: row.enableRealtime,
      minValue: row.minValue,
      maxValue: row.maxValue,
      scalingFactor: row.scalingFactor,
      offset: row.offset,
      accessMode: row.accessMode,
      deadband: row.deadband
    });

    // 解析地址配置
    try {
      addressConfig.value = JSON.parse(row.tagAddress);
    } catch {
      addressConfig.value = {};
    }
  } else {
    // 新增模式
    Object.assign(formModel, {
      tagId: '',
      deviceId: props.device?.id || '',
      tagName: '',
      tagAddress: '{}',
      dataType: 'Float',
      unit: '',
      description: '',
      enabled: true,
      enableRealtime: false,
      minValue: undefined,
      maxValue: undefined,
      scalingFactor: 1,
      offset: 0,
      accessMode: 'ReadOnly',
      deadband: 0
    });

    // 初始化地址配置
    initAddressConfig();
  }
}

// 根据协议类型初始化地址配置
function initAddressConfig() {
  const protocol = protocolType.value;
  // 协议类型: '1'=MODBUS_TCP, '2'=MODBUS_RTU, '3'=OPC_UA, '4'=OPC_DA, '5'=S7
  if (protocol === '1' || protocol === '2') {
    // MODBUS_TCP or MODBUS_RTU - 从站ID已移至设备级别配置
    addressConfig.value = {
      functionCode: '03',
      address: 0,
      quantity: 1
    };
  } else if (protocol === '3') {
    // OPC_UA
    addressConfig.value = {
      nodeId: '',
      namespaceIndex: 2
    };
  } else if (protocol === '4') {
    // OPC_DA
    addressConfig.value = {
      itemId: ''
    };
  } else if (protocol === '5') {
    // S7
    addressConfig.value = {
      area: 'DB',
      dbNumber: 1,
      offset: 0,
      bitOffset: 0
    };
  } else {
    addressConfig.value = {};
  }
}

// 地址配置变更
function handleAddressChange(config: Record<string, any>) {
  addressConfig.value = config;
  formModel.tagAddress = JSON.stringify(config);
}

// 提交表单
async function handleSubmit() {
  const valid = await formRef.value?.validate().catch(() => false);
  if (!valid) return;

  // 同步地址配置
  formModel.tagAddress = JSON.stringify(addressConfig.value);
  formModel.deviceId = props.device?.id || '';

  submitting.value = true;
  try {
    if (props.operateType === 'add') {
      const { error } = await fetchAddTag(formModel);
      if (!error) {
        ElMessage.success($t('common.addSuccess'));
        visible.value = false;
        emit('submitted');
      }
    } else {
      const { error } = await fetchUpdateTag(props.rowData!.id, formModel);
      if (!error) {
        ElMessage.success($t('common.updateSuccess'));
        visible.value = false;
        emit('submitted');
      }
    }
  } finally {
    submitting.value = false;
  }
}

// 关闭抽屉
function handleClose() {
  visible.value = false;
}

// 监听抽屉打开
watch(visible, val => {
  if (val) {
    initForm();
  } else {
    formRef.value?.resetFields();
  }
});
</script>

<template>
  <ElDrawer v-model="visible" :title="title" size="500px" @close="handleClose">
    <ElForm ref="formRef" :model="formModel" :rules="rules" label-width="100px" label-position="left">
      <!-- 基本信息 -->
      <ElDivider content-position="left">{{ $t('page.tag.basicInfo') }}</ElDivider>

      <ElFormItem :label="$t('page.tag.tagId')" prop="tagId">
        <ElInput v-model="formModel.tagId" :placeholder="$t('page.tag.tagIdPlaceholder')" />
      </ElFormItem>

      <ElFormItem :label="$t('page.tag.tagName')" prop="tagName">
        <ElInput v-model="formModel.tagName" :placeholder="$t('page.tag.tagNamePlaceholder')" />
      </ElFormItem>

      <ElFormItem :label="$t('page.tag.dataTypeLabel')" prop="dataType">
        <ElSelect v-model="formModel.dataType" :placeholder="$t('page.tag.dataTypePlaceholder')" class="w-full">
          <ElOption
            v-for="option in dataTypeOptions"
            :key="option.value"
            :label="$t(option.label)"
            :value="dataTypeFrontendToBackendMap[option.value as keyof typeof dataTypeFrontendToBackendMap]"
          />
        </ElSelect>
      </ElFormItem>

      <ElFormItem :label="$t('page.tag.unit')">
        <ElInput v-model="formModel.unit" :placeholder="$t('page.tag.unitPlaceholder')" />
      </ElFormItem>

      <ElFormItem :label="$t('page.tag.description')">
        <ElInput
          v-model="formModel.description"
          type="textarea"
          :rows="2"
          :placeholder="$t('page.tag.descriptionPlaceholder')"
        />
      </ElFormItem>

      <ElFormItem :label="$t('common.status')">
        <ElSwitch v-model="formModel.enabled" />
      </ElFormItem>

      <ElFormItem :label="$t('page.tag.enableRealtime')">
        <ElSwitch v-model="formModel.enableRealtime" />
        <span class="ml-2 text-xs text-gray-400">{{ $t('page.tag.enableRealtimeTip') }}</span>
      </ElFormItem>

      <!-- 地址配置 -->
      <ElDivider content-position="left">{{ $t('page.tag.addressConfig') }}</ElDivider>

      <!-- 协议类型: '1'=MODBUS_TCP, '2'=MODBUS_RTU, '3'=OPC_UA, '4'=OPC_DA, '5'=S7 -->
      <template v-if="protocolType === '1' || protocolType === '2'">
        <ModbusAddressInput :model-value="addressConfig" @update:model-value="handleAddressChange" />
      </template>

      <template v-else-if="protocolType === '3'">
        <OpcUaAddressInput :model-value="addressConfig" @update:model-value="handleAddressChange" />
      </template>

      <template v-else-if="protocolType === '4'">
        <OpcDaAddressInput :model-value="addressConfig" @update:model-value="handleAddressChange" />
      </template>

      <template v-else-if="protocolType === '5'">
        <S7AddressInput :model-value="addressConfig" @update:model-value="handleAddressChange" />
      </template>

      <template v-else>
        <ElFormItem :label="$t('page.tag.tagAddress')">
          <ElInput
            v-model="formModel.tagAddress"
            type="textarea"
            :rows="3"
            :placeholder="$t('page.tag.tagAddressPlaceholder')"
          />
        </ElFormItem>
      </template>

      <!-- 高级配置 -->
      <ElDivider content-position="left">{{ $t('page.tag.advancedConfig') }}</ElDivider>

      <ElFormItem :label="$t('page.tag.accessModeLabel')">
        <ElSelect v-model="formModel.accessMode" class="w-full">
          <ElOption
            v-for="option in accessModeOptions"
            :key="option.value"
            :label="$t(option.label)"
            :value="accessModeFrontendToBackendMap[option.value as keyof typeof accessModeFrontendToBackendMap]"
          />
        </ElSelect>
      </ElFormItem>

      <ElRow :gutter="16">
        <ElCol :span="12">
          <ElFormItem :label="$t('page.tag.minValue')">
            <ElInputNumber v-model="formModel.minValue" :controls="false" class="w-full" />
          </ElFormItem>
        </ElCol>
        <ElCol :span="12">
          <ElFormItem :label="$t('page.tag.maxValue')">
            <ElInputNumber v-model="formModel.maxValue" :controls="false" class="w-full" />
          </ElFormItem>
        </ElCol>
      </ElRow>

      <ElRow :gutter="16">
        <ElCol :span="12">
          <ElFormItem :label="$t('page.tag.scalingFactor')">
            <ElInputNumber v-model="formModel.scalingFactor" :step="0.1" :controls="false" class="w-full" />
          </ElFormItem>
        </ElCol>
        <ElCol :span="12">
          <ElFormItem :label="$t('page.tag.offset')">
            <ElInputNumber v-model="formModel.offset" :step="0.1" :controls="false" class="w-full" />
          </ElFormItem>
        </ElCol>
      </ElRow>

      <ElFormItem :label="$t('page.tag.deadband')">
        <ElInputNumber v-model="formModel.deadband" :min="0" :step="0.1" :controls="false" class="w-full" />
      </ElFormItem>
    </ElForm>

    <template #footer>
      <ElButton @click="handleClose">{{ $t('common.cancel') }}</ElButton>
      <ElButton type="primary" :loading="submitting" @click="handleSubmit">{{ $t('common.confirm') }}</ElButton>
    </template>
  </ElDrawer>
</template>
