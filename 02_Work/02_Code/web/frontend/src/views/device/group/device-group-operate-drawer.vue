<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import { ElDrawer, ElForm, ElFormItem, ElInput, ElInputNumber, ElTreeSelect } from 'element-plus';
import type { FormInstance, FormRules } from 'element-plus';
import { fetchAddDeviceGroup, fetchUpdateDeviceGroup } from '@/service/api';
import { $t } from '@/locales';

defineOptions({
  name: 'DeviceGroupOperateDrawer'
});

interface Props {
  /** the type of operation */
  operateType: 'add' | 'edit';
  /** the edit row data */
  rowData?: Api.Device.DeviceGroup | null;
  /** all groups tree data for parent selection */
  treeData: Api.Device.DeviceGroupTreeNode[];
}

const props = defineProps<Props>();

interface Emits {
  (e: 'submitted'): void;
}

const emit = defineEmits<Emits>();

const visible = defineModel<boolean>('visible', {
  default: false
});

const formRef = ref<FormInstance>();

const title = computed(() => {
  return props.operateType === 'add' ? $t('common.add') : $t('common.edit');
});

interface FormData {
  name: string;
  parentId?: string;
  sortOrder: number;
  description: string;
}

const formData = reactive<FormData>({
  name: '',
  parentId: undefined,
  sortOrder: 0,
  description: ''
});

const formRules: FormRules<FormData> = {
  name: [
    { required: true, message: $t('page.deviceGroup.nameRequired'), trigger: 'blur' },
    { min: 1, max: 50, message: $t('page.deviceGroup.nameLength'), trigger: 'blur' }
  ]
};

// 计算父节点的层级
const parentLevel = computed(() => {
  if (!formData.parentId) return 0;
  const findLevel = (nodes: Api.Device.DeviceGroupTreeNode[], id: string): number => {
    for (const node of nodes) {
      if (node.id === id) return node.level;
      if (node.children) {
        const level = findLevel(node.children, id);
        if (level > 0) return level;
      }
    }
    return 0;
  };
  return findLevel(props.treeData, formData.parentId);
});

// 判断是否达到最大层级
const isMaxLevel = computed(() => parentLevel.value >= 3);

// 构建树形选择器数据(排除自己和子节点)
const treeSelectData = computed(() => {
  const filterTree = (nodes: Api.Device.DeviceGroupTreeNode[]): Api.Device.DeviceGroupTreeNode[] => {
    return nodes
      .filter(node => {
        // 编辑时排除自己
        if (props.operateType === 'edit' && props.rowData && node.id === props.rowData.id) {
          return false;
        }
        // 排除第3级节点(不能作为父节点)
        return node.level < 3;
      })
      .map(node => ({
        ...node,
        children: node.children ? filterTree(node.children) : undefined
      }));
  };
  return filterTree(props.treeData);
});

async function handleSubmit() {
  if (!formRef.value) return;

  await formRef.value.validate();

  // 如果选择了第3级节点作为父节点,给出警告
  if (isMaxLevel.value) {
    window.$message?.warning($t('page.deviceGroup.levelWarning'));
    return;
  }

  const submitData: Api.Device.DeviceGroupEdit = {
    name: formData.name,
    parentId: formData.parentId,
    sortOrder: formData.sortOrder,
    description: formData.description
  };

  let success = false;

  if (props.operateType === 'add') {
    const { error } = await fetchAddDeviceGroup(submitData);
    success = !error;
  } else if (props.operateType === 'edit' && props.rowData) {
    const { error } = await fetchUpdateDeviceGroup(props.rowData.id, submitData);
    success = !error;
  }

  if (success) {
    window.$message?.success($t('common.updateSuccess'));
    closeDrawer();
    emit('submitted');
  }
}

function closeDrawer() {
  visible.value = false;
}

function resetForm() {
  formData.name = '';
  formData.parentId = undefined;
  formData.sortOrder = 0;
  formData.description = '';
  formRef.value?.clearValidate();
}

watch(visible, val => {
  if (val) {
    if (props.operateType === 'edit' && props.rowData) {
      // 编辑模式：从rowData加载数据
      formData.name = props.rowData.name;
      formData.parentId = props.rowData.parentId;
      formData.sortOrder = props.rowData.sortOrder;
      formData.description = props.rowData.description || '';
    } else {
      // 添加模式：重置表单，但保留rowData中的parentId（用于添加子分组）
      resetForm();
      if (props.rowData?.parentId) {
        formData.parentId = props.rowData.parentId;
      }
    }
  }
});
</script>

<template>
  <ElDrawer v-model="visible" :title="title" :size="480" destroy-on-close>
    <ElForm ref="formRef" :model="formData" :rules="formRules" label-position="top">
      <ElFormItem :label="$t('page.deviceGroup.name')" prop="name">
        <ElInput
          v-model="formData.name"
          :placeholder="$t('page.deviceGroup.namePlaceholder')"
          :maxlength="50"
          show-word-limit
        />
      </ElFormItem>

      <ElFormItem :label="$t('page.deviceGroup.parent')" prop="parentId">
        <ElTreeSelect
          v-model="formData.parentId"
          :data="treeSelectData"
          :props="{ label: 'name', value: 'id', children: 'children' }"
          :placeholder="$t('page.deviceGroup.parentPlaceholder')"
          clearable
          filterable
          check-strictly
          default-expand-all
          class="w-full"
        />
        <div v-if="isMaxLevel" class="el-form-item__error position-static">
          {{ $t('page.deviceGroup.levelWarning') }}
        </div>
      </ElFormItem>

      <ElFormItem :label="$t('page.deviceGroup.sortOrder')" prop="sortOrder">
        <ElInputNumber v-model="formData.sortOrder" :min="0" class="w-full" />
      </ElFormItem>

      <ElFormItem :label="$t('page.deviceGroup.description')" prop="description">
        <ElInput
          v-model="formData.description"
          type="textarea"
          :placeholder="$t('page.deviceGroup.descriptionPlaceholder')"
          :maxlength="200"
          show-word-limit
          :rows="4"
        />
      </ElFormItem>
    </ElForm>

    <template #footer>
      <div class="flex justify-end gap-12px">
        <ElButton @click="closeDrawer">{{ $t('common.cancel') }}</ElButton>
        <ElButton type="primary" @click="handleSubmit">{{ $t('common.confirm') }}</ElButton>
      </div>
    </template>
  </ElDrawer>
</template>

<style scoped></style>
