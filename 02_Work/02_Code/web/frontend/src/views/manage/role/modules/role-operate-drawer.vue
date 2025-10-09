<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue';
import type { FormInstance, FormRules } from 'element-plus';
import { useBoolean } from '@sa/hooks';
import { enableStatusOptions } from '@/constants/business';
import { fetchAddRole, fetchUpdateRole } from '@/service/api';
import { $t } from '@/locales';
import MenuAuthModal from './menu-auth-modal.vue';
import ButtonAuthModal from './button-auth-modal.vue';

defineOptions({
  name: 'RoleOperateDrawer'
});

interface Props {
  /** 操作类型 */
  operateType: UI.TableOperateType;
  /** 行数据 */
  rowData?: Api.SystemManage.Role | null;
}

const props = defineProps<Props>();

interface Emits {
  (e: 'submitted'): void;
}

const emit = defineEmits<Emits>();

const visible = defineModel<boolean>('visible', {
  default: false
});

const { bool: menuAuthVisible, setTrue: openMenuAuthModal } = useBoolean();
const { bool: buttonAuthVisible, setTrue: openButtonAuthModal } = useBoolean();

const title = computed(() => {
  const titles: Record<UI.TableOperateType, string> = {
    add: $t('page.manage.role.addRole'),
    edit: $t('page.manage.role.editRole')
  };
  return titles[props.operateType];
});

const roleId = computed(() => props.rowData?.id || '');

const isEdit = computed(() => props.operateType === 'edit');

type Model = Pick<Api.SystemManage.Role, 'roleName' | 'roleCode' | 'roleDesc' | 'status'>;

const model = reactive<Model>(createDefaultModel());

function createDefaultModel(): Model {
  return {
    roleName: '',
    roleCode: '',
    roleDesc: '',
    status: '1' as Api.Common.EnableStatus
  };
}

const rules: FormRules = {
  roleName: [
    {
      required: true,
      message: $t('page.manage.role.form.roleName'),
      trigger: 'blur'
    }
  ],
  roleCode: [
    {
      required: true,
      message: $t('page.manage.role.form.roleCode'),
      trigger: 'blur'
    }
  ],
  status: [
    {
      required: true,
      message: $t('page.manage.role.form.roleStatus'),
      trigger: 'change'
    }
  ]
};

const formRef = ref<FormInstance>();

function handleUpdateModelWhenEdit() {
  if (props.operateType === 'add') {
    Object.assign(model, createDefaultModel());
  }

  if (props.operateType === 'edit' && props.rowData) {
    // 将后端返回的数据赋值给 model，并确保 status 是字符串类型
    Object.assign(model, {
      ...props.rowData,
      status:
        props.rowData.status !== undefined
          ? (String(props.rowData.status) as Api.Common.EnableStatus)
          : ('1' as Api.Common.EnableStatus)
    });
  }
}

async function handleSubmit() {
  await formRef.value?.validate();

  // 准备提交数据，将 status 从字符串转换为数字
  const submitData: Api.SystemManage.RoleEdit = {
    roleName: model.roleName,
    roleCode: model.roleCode,
    roleDesc: model.roleDesc,
    status: model.status ? Number(model.status) : undefined
  };

  if (props.operateType === 'add') {
    const { error } = await fetchAddRole(submitData);
    if (!error) {
      window.$message?.success($t('common.addSuccess'));
      closeDrawer();
      emit('submitted');
    }
  } else {
    // 编辑模式,确保 id 存在
    if (!props.rowData?.id) {
      window.$message?.error('缺少角色ID');
      return;
    }

    const { error } = await fetchUpdateRole(props.rowData.id, submitData);

    if (!error) {
      window.$message?.success($t('common.updateSuccess'));
      closeDrawer();
      emit('submitted');
    }
  }
}

function closeDrawer() {
  visible.value = false;
}

watch(visible, () => {
  if (visible.value) {
    handleUpdateModelWhenEdit();
    formRef.value?.clearValidate();
  }
});
</script>

<template>
  <ElDrawer v-model="visible" :title="title" :size="360">
    <ElForm ref="formRef" :model="model" :rules="rules" label-position="top">
      <ElFormItem :label="$t('page.manage.role.roleName')" prop="roleName">
        <ElInput v-model="model.roleName" :placeholder="$t('page.manage.role.form.roleName')" />
      </ElFormItem>
      <ElFormItem :label="$t('page.manage.role.roleCode')" prop="roleCode">
        <ElInput v-model="model.roleCode" :placeholder="$t('page.manage.role.form.roleCode')" />
      </ElFormItem>
      <ElFormItem :label="$t('page.manage.role.roleStatus')" prop="status">
        <ElRadioGroup v-model="model.status">
          <ElRadio v-for="item in enableStatusOptions" :key="item.value" :value="item.value" :label="$t(item.label)" />
        </ElRadioGroup>
      </ElFormItem>
      <ElFormItem :label="$t('page.manage.role.roleDesc')" prop="roleDesc">
        <ElInput
          v-model="model.roleDesc"
          type="textarea"
          :rows="3"
          :placeholder="$t('page.manage.role.form.roleDesc')"
        />
      </ElFormItem>
    </ElForm>
    <ElSpace v-if="isEdit">
      <ElButton @click="openMenuAuthModal">{{ $t('page.manage.role.menuAuth') }}</ElButton>
      <MenuAuthModal v-model:visible="menuAuthVisible" :role-id="roleId" />
      <ElButton @click="openButtonAuthModal">{{ $t('page.manage.role.buttonAuth') }}</ElButton>
      <ButtonAuthModal v-model:visible="buttonAuthVisible" :role-id="roleId" />
    </ElSpace>
    <template #footer>
      <ElSpace :size="16">
        <ElButton @click="closeDrawer">{{ $t('common.cancel') }}</ElButton>
        <ElButton type="primary" @click="handleSubmit">{{ $t('common.confirm') }}</ElButton>
      </ElSpace>
    </template>
  </ElDrawer>
</template>

<style lang="scss" scoped></style>
