<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { ElMessageBox } from 'element-plus';
import { enableStatusOptions, userGenderOptions, userTypeOptions } from '@/constants/business';
import { fetchAddUser, fetchGetAllRoles, fetchUpdateUser } from '@/service/api';
import { useForm, useFormRules } from '@/hooks/common/form';
import { $t } from '@/locales';

defineOptions({ name: 'UserOperateDrawer' });

interface Props {
  /** the type of operation */
  operateType: UI.TableOperateType;
  /** the edit row data */
  rowData?: Api.SystemManage.User | null;
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
const { defaultRequiredRule, patternRules } = useFormRules();

const title = computed(() => {
  const titles: Record<UI.TableOperateType, string> = {
    add: $t('page.manage.user.addUser'),
    edit: $t('page.manage.user.editUser')
  };
  return titles[props.operateType];
});

type Model = Pick<
  Api.SystemManage.User,
  'userName' | 'userGender' | 'nickName' | 'userPhone' | 'userEmail' | 'userRoles' | 'status' | 'userType'
> & {
  password?: string;
};

const model = ref(createDefaultModel());

// 记录编辑时的原始用户类型，用于确认提示
const originalUserType = ref<Api.SystemManage.UserType>('user');

function createDefaultModel(): Model {
  return {
    userName: '',
    userGender: undefined,
    nickName: '',
    userPhone: '',
    userEmail: '',
    userRoles: [],
    status: '1' as Api.Common.EnableStatus,
    userType: 'user' as Api.SystemManage.UserType,
    password: ''
  };
}

type RuleKey = Extract<keyof Model, 'userName' | 'status' | 'userPhone' | 'userEmail'>;

const rules = computed<Record<RuleKey, App.Global.FormRule>>(() => {
  return {
    userName: defaultRequiredRule,
    status: defaultRequiredRule,
    userPhone: patternRules.phone,
    userEmail: patternRules.email
  };
});

const passwordRule = computed(() => {
  return {
    required: props.operateType === 'add',
    message: '请输入密码',
    trigger: 'blur'
  } as App.Global.FormRule;
});

/** the enabled role options */
const roleOptions = ref<CommonType.Option<string>[]>([]);

async function getRoleOptions() {
  const { error, data } = await fetchGetAllRoles();

  if (!error) {
    const options = data.map(item => ({
      label: item.roleName,
      value: item.roleCode
    }));

    roleOptions.value = options;
  }
}

function handleInitModel() {
  model.value = createDefaultModel();

  if (props.operateType === 'edit' && props.rowData) {
    // 记录原始用户类型
    originalUserType.value = props.rowData.userType || 'user';
    // 将后端返回的数据赋值给 model，并确保类型正确
    Object.assign(model.value, {
      ...props.rowData,
      userGender: props.rowData.userGender,
      status:
        props.rowData.status !== undefined
          ? (String(props.rowData.status) as Api.Common.EnableStatus)
          : ('1' as Api.Common.EnableStatus),
      userType: props.rowData.userType || 'user'
    });
  }
}

function closeDrawer() {
  visible.value = false;
}

/** 检查用户类型变更是否需要确认，返回 true 表示可以继续提交 */
async function confirmUserTypeChange(): Promise<boolean> {
  if (props.operateType !== 'edit' || model.value.userType === originalUserType.value) {
    return true;
  }

  const confirmMsg =
    model.value.userType === 'service'
      ? $t('page.manage.user.userTypeChangeConfirm.toService')
      : $t('page.manage.user.userTypeChangeConfirm.toUser');

  try {
    await ElMessageBox.confirm(confirmMsg, $t('common.tip'), {
      confirmButtonText: $t('common.confirm'),
      cancelButtonText: $t('common.cancel'),
      type: 'warning'
    });
    return true;
  } catch {
    return false;
  }
}

async function handleSubmit() {
  await validate();

  // 编辑模式下，如果用户类型发生变化，需要确认
  if (!(await confirmUserTypeChange())) {
    return;
  }

  // 准备提交数据，将 status 和 userGender 从字符串转换为数字
  const submitData: Api.SystemManage.UserEdit = {
    userName: model.value.userName,
    nickName: model.value.nickName || undefined,
    userGender: model.value.userGender ? Number(model.value.userGender) : undefined,
    userPhone: model.value.userPhone || undefined,
    userEmail: model.value.userEmail || undefined,
    status: model.value.status ? Number(model.value.status) : undefined,
    userType: model.value.userType,
    userRoles: model.value.userRoles && model.value.userRoles.length > 0 ? model.value.userRoles : undefined,
    password: model.value.password || undefined
  };

  if (props.operateType === 'add') {
    const { error } = await fetchAddUser(submitData);
    if (!error) {
      window.$message?.success($t('common.addSuccess'));
      closeDrawer();
      emit('submitted');
    }
  } else {
    // 编辑模式,确保 id 存在
    if (!props.rowData?.id) {
      window.$message?.error('缺少用户ID');
      return;
    }

    const { error } = await fetchUpdateUser(props.rowData.id, submitData);

    if (!error) {
      window.$message?.success($t('common.updateSuccess'));
      closeDrawer();
      emit('submitted');
    }
  }
}

watch(visible, () => {
  if (visible.value) {
    handleInitModel();
    restoreValidation();
    getRoleOptions();
  }
});
</script>

<template>
  <ElDrawer v-model="visible" :title="title" :size="360">
    <ElForm ref="formRef" :model="model" :rules="rules" label-position="top">
      <ElFormItem :label="$t('page.manage.user.userName')" prop="userName">
        <ElInput v-model="model.userName" :placeholder="$t('page.manage.user.form.userName')" />
      </ElFormItem>
      <ElFormItem :label="$t('page.manage.user.userTypeLabel')" prop="userType">
        <ElRadioGroup v-model="model.userType">
          <ElRadio v-for="item in userTypeOptions" :key="item.value" :value="item.value" :label="$t(item.label)" />
        </ElRadioGroup>
      </ElFormItem>
      <ElFormItem
        v-if="operateType === 'add' || operateType === 'edit'"
        label="密码"
        prop="password"
        :rules="passwordRule"
      >
        <ElInput
          v-model="model.password"
          type="password"
          :placeholder="operateType === 'add' ? '请输入密码' : '留空则不修改密码'"
          show-password
        />
      </ElFormItem>
      <ElFormItem :label="$t('page.manage.user.userGender')" prop="userGender">
        <ElRadioGroup v-model="model.userGender">
          <ElRadio v-for="item in userGenderOptions" :key="item.value" :value="item.value" :label="$t(item.label)" />
        </ElRadioGroup>
      </ElFormItem>
      <ElFormItem :label="$t('page.manage.user.nickName')" prop="nickName">
        <ElInput v-model="model.nickName" :placeholder="$t('page.manage.user.form.nickName')" />
      </ElFormItem>
      <ElFormItem :label="$t('page.manage.user.userPhone')" prop="userPhone">
        <ElInput v-model="model.userPhone" :placeholder="$t('page.manage.user.form.userPhone')" />
      </ElFormItem>
      <ElFormItem :label="$t('page.manage.user.userEmail')" prop="userEmail">
        <ElInput v-model="model.userEmail" :placeholder="$t('page.manage.user.form.userEmail')" />
      </ElFormItem>
      <ElFormItem :label="$t('page.manage.user.userStatus')" prop="status">
        <ElRadioGroup v-model="model.status">
          <ElRadio v-for="item in enableStatusOptions" :key="item.value" :value="item.value" :label="$t(item.label)" />
        </ElRadioGroup>
      </ElFormItem>
      <ElFormItem :label="$t('page.manage.user.userRole')" prop="roles">
        <ElSelect v-model="model.userRoles" multiple :placeholder="$t('page.manage.user.form.userRole')">
          <ElOption v-for="{ label, value } in roleOptions" :key="value" :label="label" :value="value" />
        </ElSelect>
      </ElFormItem>
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
