<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { platformTypeOptions } from '@/constants/business';
import { fetchAddEdgeNode, fetchUpdateEdgeNode } from '@/service/api';
import { useForm, useFormRules } from '@/hooks/common/form';
import { translateOptions } from '@/utils/common';
import { $t } from '@/locales';

defineOptions({ name: 'NodeOperateDrawer' });

interface Props {
  /** the type of operation */
  operateType: UI.TableOperateType;
  /** the edit row data */
  rowData?: Api.EdgeNode.EdgeNode | null;
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
    add: $t('page.edgeNode.addNode'),
    edit: $t('page.edgeNode.editNode')
  };
  return titles[props.operateType];
});

// ============ 表单Model定义 ============

type Model = {
  nodeId: string;
  nodeName: string;
  platform: Api.EdgeNode.PlatformType;
  version: string;
  location: string;
  ipAddress: string;
  port: number | null;
  resourceLimits: string;
  osInfo: string;
  hardwareInfo: string;
  installPath: string;
};

const model = ref(createDefaultModel());

function createDefaultModel(): Model {
  return {
    nodeId: '',
    nodeName: '',
    platform: 'NET8.0',
    version: '',
    location: '',
    ipAddress: '',
    port: null,
    resourceLimits: '',
    osInfo: '',
    hardwareInfo: '',
    installPath: ''
  };
}

// ============ 表单验证规则 ============

type RuleKey = 'nodeId' | 'nodeName' | 'platform';

const rules = computed<Record<RuleKey, App.Global.FormRule>>(() => {
  return {
    nodeId: defaultRequiredRule,
    nodeName: defaultRequiredRule,
    platform: defaultRequiredRule
  };
});

// ============ 模式判断 ============
const isEdit = computed(() => props.operateType === 'edit');

// 是否为手动添加的节点
const isManualNode = computed(() => {
  if (!isEdit.value || !props.rowData) return false;
  return props.rowData.registrationType === 'manual';
});

// 采集程序是否曾经连接过（有心跳记录）
const hasConnected = computed(() => {
  if (!isEdit.value || !props.rowData) return false;
  return props.rowData.lastHeartbeat !== null;
});

// 系统字段是否可编辑：手动添加的节点 且 从未连接过
const canEditSystemFields = computed(() => {
  return isManualNode.value && !hasConnected.value;
});

// 提示信息
const alertMessage = computed(() => {
  if (!isEdit.value) {
    return $t('page.edgeNode.manualNodeNote');
  }
  if (isManualNode.value && !hasConnected.value) {
    return $t('page.edgeNode.manualNodeEditableNote');
  }
  if (isManualNode.value && hasConnected.value) {
    return $t('page.edgeNode.manualNodeConnectedNote');
  }
  return $t('page.edgeNode.autoNodeEditNote');
});

const alertType = computed(() => {
  if (!isEdit.value) return 'info';
  if (canEditSystemFields.value) return 'success';
  return 'warning';
});

// 统一监听：当 drawer 打开且有数据时初始化
watch(
  [visible, () => props.rowData, () => props.operateType],
  ([isVisible, rowData, opType], [prevVisible]) => {
    // 只在 drawer 刚打开时初始化
    if (isVisible && !prevVisible) {
      restoreValidation();
      if (opType === 'add') {
        model.value = createDefaultModel();
      } else if (opType === 'edit' && rowData) {
        model.value = {
          nodeId: rowData.nodeId || '',
          nodeName: rowData.nodeName || '',
          platform: rowData.platform || 'NET8.0',
          version: rowData.version || '',
          location: rowData.location || '',
          ipAddress: rowData.ipAddress || '',
          port: rowData.port ?? null,
          resourceLimits: rowData.resourceLimits || '',
          osInfo: rowData.osInfo || '',
          hardwareInfo: rowData.hardwareInfo || '',
          installPath: rowData.installPath || ''
        };
      }
    }
  },
  { flush: 'post' }
);

// ============ 表单提交 ============

function closeDrawer() {
  visible.value = false;
}

async function handleSubmit() {
  await validate();

  if (props.operateType === 'add') {
    // 新增节点 - 只发送核心字段
    const { error } = await fetchAddEdgeNode({
      nodeId: model.value.nodeId,
      nodeName: model.value.nodeName,
      platform: model.value.platform,
      location: model.value.location || undefined,
      resourceLimits: model.value.resourceLimits || undefined
    });

    if (!error) {
      window.$message?.success($t('common.addSuccess'));
      closeDrawer();
      emit('submitted');
    }
  } else {
    // 编辑节点
    const updateData: Api.EdgeNode.UpdateEdgeNodeRequest = {
      nodeName: model.value.nodeName,
      location: model.value.location || undefined,
      resourceLimits: model.value.resourceLimits || undefined
    };

    // 如果系统字段可编辑（手动添加且未连接），则包含这些字段
    if (canEditSystemFields.value) {
      updateData.platform = model.value.platform;
      updateData.version = model.value.version || undefined;
      updateData.ipAddress = model.value.ipAddress || undefined;
      updateData.port = model.value.port || undefined;
      updateData.osInfo = model.value.osInfo || undefined;
      updateData.hardwareInfo = model.value.hardwareInfo || undefined;
      updateData.installPath = model.value.installPath || undefined;
    }

    const { error } = await fetchUpdateEdgeNode(props.rowData!.id, updateData);

    if (!error) {
      window.$message?.success($t('common.updateSuccess'));
      closeDrawer();
      emit('submitted');
    }
  }
}
</script>

<template>
  <ElDrawer v-model="visible" :title="title" size="560px">
    <ElForm ref="formRef" :model="model" :rules="rules" label-position="top">
      <!-- 提示信息 -->
      <ElAlert :type="alertType" :title="alertMessage" show-icon :closable="false" class="mb-16px" />

      <!-- 基本信息 -->
      <ElDivider content-position="left">{{ $t('page.edgeNode.basicInfo') }}</ElDivider>

      <!-- 节点ID（新增时可编辑，编辑时始终只读） -->
      <ElFormItem :label="$t('page.edgeNode.nodeId')" prop="nodeId">
        <ElInput v-model="model.nodeId" :placeholder="$t('page.edgeNode.form.nodeId')" :disabled="isEdit" clearable />
      </ElFormItem>

      <!-- 节点名称（始终可编辑） -->
      <ElFormItem :label="$t('page.edgeNode.nodeName')" prop="nodeName">
        <ElInput v-model="model.nodeName" :placeholder="$t('page.edgeNode.form.nodeName')" clearable />
      </ElFormItem>

      <!-- 平台类型（新增可编辑，编辑时：手动未连接可编辑，其他只读） -->
      <ElFormItem :label="$t('page.edgeNode.platform')" prop="platform">
        <ElSelect
          v-model="model.platform"
          :placeholder="$t('page.edgeNode.form.platform')"
          :disabled="isEdit && !canEditSystemFields"
        >
          <ElOption
            v-for="{ label, value } in translateOptions(platformTypeOptions)"
            :key="value"
            :label="label"
            :value="value"
          />
        </ElSelect>
      </ElFormItem>

      <!-- 位置（始终可编辑） -->
      <ElFormItem :label="$t('page.edgeNode.location')">
        <ElInput v-model="model.location" :placeholder="$t('page.edgeNode.form.location')" clearable />
      </ElFormItem>

      <!-- 系统字段区域 -->
      <!-- 新增模式：显示可编辑的系统字段 -->
      <!-- 编辑模式：手动未连接可编辑，其他只读展示 -->
      <ElDivider content-position="left">{{ $t('page.edgeNode.systemInfo') }}</ElDivider>

      <!-- 可编辑的系统字段（新增模式 或 手动未连接） -->
      <template v-if="!isEdit || canEditSystemFields">
        <ElRow :gutter="16">
          <ElCol :span="12">
            <ElFormItem :label="$t('page.edgeNode.version')">
              <ElInput v-model="model.version" :placeholder="$t('page.edgeNode.form.version')" clearable />
            </ElFormItem>
          </ElCol>
          <ElCol :span="12">
            <ElFormItem :label="$t('page.edgeNode.ipAddress')">
              <ElInput v-model="model.ipAddress" :placeholder="$t('page.edgeNode.form.ipAddress')" clearable />
            </ElFormItem>
          </ElCol>
        </ElRow>

        <ElRow :gutter="16">
          <ElCol :span="12">
            <ElFormItem :label="$t('page.edgeNode.port')">
              <ElInputNumber v-model="model.port" :min="1" :max="65535" controls-position="right" class="w-full" />
            </ElFormItem>
          </ElCol>
          <ElCol :span="12">
            <ElFormItem :label="$t('page.edgeNode.installPath')">
              <ElInput v-model="model.installPath" :placeholder="$t('page.edgeNode.form.installPath')" clearable />
            </ElFormItem>
          </ElCol>
        </ElRow>

        <ElFormItem :label="$t('page.edgeNode.osInfo')">
          <ElInput v-model="model.osInfo" :placeholder="$t('page.edgeNode.form.osInfo')" clearable />
        </ElFormItem>

        <ElFormItem :label="$t('page.edgeNode.hardwareInfo')">
          <ElInput
            v-model="model.hardwareInfo"
            type="textarea"
            :rows="2"
            :placeholder="$t('page.edgeNode.form.hardwareInfo')"
          />
        </ElFormItem>
      </template>

      <!-- 已连接或自动注册：只读展示系统信息 -->
      <template v-else-if="isEdit && rowData">
        <ElDescriptions :column="2" border>
          <ElDescriptionsItem :label="$t('page.edgeNode.status')">
            <ElTag :type="rowData.status === 'Online' ? 'success' : rowData.status === 'Error' ? 'danger' : 'warning'">
              {{ rowData.status || '-' }}
            </ElTag>
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.edgeNode.registrationType')">
            <ElTag :type="rowData.registrationType === 'manual' ? 'success' : 'primary'">
              {{
                rowData.registrationType === 'manual'
                  ? $t('page.edgeNode.registrationTypeOptions.manual')
                  : $t('page.edgeNode.registrationTypeOptions.auto')
              }}
            </ElTag>
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.edgeNode.version')">
            {{ rowData.version || '-' }}
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.edgeNode.ipAddress')">
            {{ rowData.ipAddress || '-' }}
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.edgeNode.port')">
            {{ rowData.port || '-' }}
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.edgeNode.lastHeartbeat')">
            {{ rowData.lastHeartbeat ? new Date(rowData.lastHeartbeat).toLocaleString('zh-CN') : '-' }}
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.edgeNode.osInfo')" :span="2">
            {{ rowData.osInfo || '-' }}
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.edgeNode.hardwareInfo')" :span="2">
            {{ rowData.hardwareInfo || '-' }}
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.edgeNode.installPath')" :span="2">
            {{ rowData.installPath || '-' }}
          </ElDescriptionsItem>
        </ElDescriptions>
      </template>

      <!-- 高级配置 -->
      <ElDivider content-position="left">{{ $t('page.edgeNode.advancedConfig') }}</ElDivider>

      <!-- 资源限制 (JSON) -->
      <ElFormItem :label="$t('page.edgeNode.resourceLimits')">
        <ElInput
          v-model="model.resourceLimits"
          type="textarea"
          :rows="3"
          :placeholder="$t('page.edgeNode.form.resourceLimits')"
        />
      </ElFormItem>
    </ElForm>

    <template #footer>
      <ElSpace>
        <ElButton @click="closeDrawer">{{ $t('common.cancel') }}</ElButton>
        <ElButton type="primary" @click="handleSubmit">{{ $t('common.confirm') }}</ElButton>
      </ElSpace>
    </template>
  </ElDrawer>
</template>

<style scoped></style>
