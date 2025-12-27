<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { taskTypeOptions } from '@/constants/business';
import { fetchAddCollectionTask, fetchGetAvailableDevices, fetchUpdateCollectionTask } from '@/service/api';
import { useForm, useFormRules } from '@/hooks/common/form';
import { translateOptions } from '@/utils/common';
import { $t } from '@/locales';

defineOptions({ name: 'TaskOperateDrawer' });

interface Props {
  /** the type of operation */
  operateType: UI.TableOperateType;
  /** the edit row data */
  rowData?: Api.CollectionTask.CollectionTask | null;
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
    add: $t('page.collectionTask.addTask'),
    edit: $t('page.collectionTask.editTask')
  };
  return titles[props.operateType];
});

// ============ 表单Model定义 ============

type Model = {
  name: string;
  code: string;
  description: string;
  taskType: Api.CollectionTask.TaskType;
  defaultInterval: number | null;
  cronExpression: string;
  priority: number;
  isEnabled: boolean;
  effectiveFrom: string;
  effectiveTo: string;
  deviceIds: string[];
};

const model = ref(createDefaultModel());

function createDefaultModel(): Model {
  return {
    name: '',
    code: '',
    description: '',
    taskType: 'Periodic',
    defaultInterval: 1000,
    cronExpression: '',
    priority: 5,
    isEnabled: true,
    effectiveFrom: '',
    effectiveTo: '',
    deviceIds: []
  };
}

// ============ 表单验证规则 ============

type RuleKey = 'name' | 'code' | 'taskType';

const rules = computed<Record<RuleKey, App.Global.FormRule>>(() => {
  return {
    name: defaultRequiredRule,
    code: defaultRequiredRule,
    taskType: defaultRequiredRule
  };
});

// ============ 模式判断 ============
const isEdit = computed(() => props.operateType === 'edit');

// 任务类型需要显示间隔配置
const showIntervalConfig = computed(() => {
  return model.value.taskType === 'Periodic' || model.value.taskType === 'Hybrid';
});

// 任务类型需要显示Cron配置
const showCronConfig = computed(() => {
  return model.value.taskType === 'Scheduled';
});

// ============ 设备选项 ============
const deviceOptions = ref<Api.CollectionTask.AvailableDevice[]>([]);
const loadingDevices = ref(false);

async function loadDeviceOptions() {
  loadingDevices.value = true;
  try {
    const taskId = isEdit.value && props.rowData ? props.rowData.id : undefined;
    const { data, error } = await fetchGetAvailableDevices(taskId);
    if (!error && data) {
      deviceOptions.value = data;
    }
  } finally {
    loadingDevices.value = false;
  }
}

// 优先级选项
const priorityOptions = Array.from({ length: 10 }, (_, i) => ({
  value: i + 1,
  label: String(i + 1)
}));

// 统一监听：当 drawer 打开且有数据时初始化
watch(
  [visible, () => props.rowData, () => props.operateType],
  ([isVisible, rowData, opType], [prevVisible]) => {
    // 只在 drawer 刚打开时初始化
    if (isVisible && !prevVisible) {
      restoreValidation();
      loadDeviceOptions();
      if (opType === 'add') {
        model.value = createDefaultModel();
      } else if (opType === 'edit' && rowData) {
        model.value = {
          name: rowData.name || '',
          code: rowData.code || '',
          description: rowData.description || '',
          taskType: rowData.taskType || 'Periodic',
          defaultInterval: rowData.defaultInterval ?? 1000,
          cronExpression: rowData.cronExpression || '',
          priority: rowData.priority ?? 5,
          isEnabled: rowData.isEnabled ?? true,
          effectiveFrom: rowData.effectiveFrom || '',
          effectiveTo: rowData.effectiveTo || '',
          deviceIds: rowData.deviceIds || []
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

// 构建请求数据
function buildRequestData() {
  const baseData = {
    name: model.value.name,
    description: model.value.description || undefined,
    taskType: model.value.taskType,
    defaultInterval: showIntervalConfig.value ? (model.value.defaultInterval ?? undefined) : undefined,
    cronExpression: showCronConfig.value ? model.value.cronExpression || undefined : undefined,
    priority: model.value.priority,
    isEnabled: model.value.isEnabled,
    effectiveFrom: model.value.effectiveFrom || undefined,
    effectiveTo: model.value.effectiveTo || undefined,
    deviceIds: model.value.deviceIds.length > 0 ? model.value.deviceIds : undefined
  };

  if (props.operateType === 'add') {
    return { ...baseData, code: model.value.code };
  }
  return baseData;
}

async function handleSubmit() {
  await validate();

  const requestData = buildRequestData();

  if (props.operateType === 'add') {
    const { error } = await fetchAddCollectionTask(requestData as Api.CollectionTask.CreateCollectionTaskRequest);

    if (!error) {
      window.$message?.success($t('common.addSuccess'));
      closeDrawer();
      emit('submitted');
    }
  } else {
    const { error } = await fetchUpdateCollectionTask(
      props.rowData!.id,
      requestData as Api.CollectionTask.UpdateCollectionTaskRequest
    );

    if (!error) {
      window.$message?.success($t('common.updateSuccess'));
      closeDrawer();
      emit('submitted');
    }
  }
}

// 获取任务类型描述
function getTaskTypeDescription(type: Api.CollectionTask.TaskType): string {
  const descMap: Record<Api.CollectionTask.TaskType, string> = {
    Periodic: $t('page.collectionTask.taskTypeDescription.periodic'),
    Scheduled: $t('page.collectionTask.taskTypeDescription.scheduled'),
    EventDriven: $t('page.collectionTask.taskTypeDescription.eventDriven'),
    Hybrid: $t('page.collectionTask.taskTypeDescription.hybrid')
  };
  return descMap[type] || '';
}
</script>

<template>
  <ElDrawer v-model="visible" :title="title" size="600px">
    <ElForm ref="formRef" :model="model" :rules="rules" label-position="top">
      <!-- 基本信息 -->
      <ElDivider content-position="left">{{ $t('page.edgeNode.basicInfo') }}</ElDivider>

      <!-- 任务名称 -->
      <ElFormItem :label="$t('page.collectionTask.name')" prop="name">
        <ElInput v-model="model.name" :placeholder="$t('page.collectionTask.form.name')" clearable />
      </ElFormItem>

      <!-- 任务编码（新增时可编辑，编辑时只读） -->
      <ElFormItem :label="$t('page.collectionTask.code')" prop="code">
        <ElInput v-model="model.code" :placeholder="$t('page.collectionTask.form.code')" :disabled="isEdit" clearable />
      </ElFormItem>

      <!-- 任务描述 -->
      <ElFormItem :label="$t('page.collectionTask.description')">
        <ElInput
          v-model="model.description"
          type="textarea"
          :rows="2"
          :placeholder="$t('page.collectionTask.form.description')"
        />
      </ElFormItem>

      <!-- 任务配置 -->
      <ElDivider content-position="left">{{ $t('page.device.protocolConfig') }}</ElDivider>

      <!-- 任务类型 -->
      <ElFormItem :label="$t('page.collectionTask.taskType')" prop="taskType">
        <ElSelect v-model="model.taskType" :placeholder="$t('page.collectionTask.form.taskType')">
          <ElOption
            v-for="{ label, value } in translateOptions(taskTypeOptions)"
            :key="value"
            :label="label"
            :value="value"
          />
        </ElSelect>
        <div class="mt-8px text-12px text-gray-500">
          {{ getTaskTypeDescription(model.taskType) }}
        </div>
      </ElFormItem>

      <!-- 采集间隔（周期/混合模式） -->
      <ElFormItem v-if="showIntervalConfig" :label="$t('page.collectionTask.defaultInterval')">
        <ElInputNumber
          v-model="model.defaultInterval"
          :min="100"
          :max="86400000"
          :step="100"
          controls-position="right"
          class="w-full"
        />
        <span class="ml-8px text-gray-500">{{ $t('page.collectionTask.intervalMs') }}</span>
      </ElFormItem>

      <!-- Cron表达式（定时模式） -->
      <ElFormItem v-if="showCronConfig" :label="$t('page.collectionTask.cronExpression')">
        <ElInput
          v-model="model.cronExpression"
          :placeholder="$t('page.collectionTask.form.cronExpression')"
          clearable
        />
        <div class="mt-8px text-12px text-gray-500">
          {{ $t('page.collectionTask.cronExpressionHelp') }}
        </div>
      </ElFormItem>

      <!-- 优先级 -->
      <ElFormItem :label="$t('page.collectionTask.priority')">
        <ElSelect v-model="model.priority" :placeholder="$t('page.collectionTask.form.priority')">
          <ElOption v-for="opt in priorityOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
        </ElSelect>
      </ElFormItem>

      <!-- 启用状态 -->
      <ElFormItem :label="$t('page.collectionTask.isEnabled')">
        <ElSwitch v-model="model.isEnabled" />
      </ElFormItem>

      <!-- 生效时间 -->
      <ElRow :gutter="16">
        <ElCol :span="12">
          <ElFormItem :label="$t('page.collectionTask.effectiveFrom')">
            <ElDatePicker
              v-model="model.effectiveFrom"
              type="datetime"
              value-format="YYYY-MM-DDTHH:mm:ss"
              class="w-full"
            />
          </ElFormItem>
        </ElCol>
        <ElCol :span="12">
          <ElFormItem :label="$t('page.collectionTask.effectiveTo')">
            <ElDatePicker
              v-model="model.effectiveTo"
              type="datetime"
              value-format="YYYY-MM-DDTHH:mm:ss"
              class="w-full"
            />
          </ElFormItem>
        </ElCol>
      </ElRow>

      <!-- 关联设备 -->
      <ElDivider content-position="left">{{ $t('page.collectionTask.devices') }}</ElDivider>

      <ElFormItem :label="$t('page.collectionTask.devices')">
        <ElSelect
          v-model="model.deviceIds"
          multiple
          filterable
          collapse-tags
          collapse-tags-tooltip
          :loading="loadingDevices"
          :placeholder="$t('page.collectionTask.form.devices')"
          class="w-full"
        >
          <ElOption
            v-for="device in deviceOptions"
            :key="device.id"
            :label="`${device.deviceName} (${device.deviceId})`"
            :value="device.id"
          />
        </ElSelect>
        <div class="mt-8px text-12px text-gray-500">
          {{
            $t('page.collectionTask.selectedDevices', {
              count: model.deviceIds.length
            })
          }}
        </div>
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
