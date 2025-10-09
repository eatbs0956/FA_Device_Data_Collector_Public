<script setup lang="ts">
import { computed, shallowRef } from 'vue';
import { $t } from '@/locales';

defineOptions({ name: 'ButtonAuthModal' });

interface Props {
  /** the roleId */
  roleId: string;
}

const props = defineProps<Props>();

const visible = defineModel<boolean>('visible', {
  default: false
});

function closeModal() {
  visible.value = false;
}

const title = computed(() => $t('common.edit') + $t('page.manage.role.buttonAuth'));

type ButtonConfig = {
  id: number;
  label: string;
  code: string;
};

const tree = shallowRef<ButtonConfig[]>([]);

async function getAllButtons() {
  // request
}

const checks = shallowRef<number[]>([]);

async function getChecks() {
  // eslint-disable-next-line no-console
  console.log(props.roleId);
  // request
  checks.value = [1, 2, 3, 4, 5];
}

function checkChange(val: ButtonConfig) {
  const idx = checks.value.indexOf(val.id);
  if (idx === -1) {
    checks.value.push(val.id);
  } else {
    checks.value.splice(idx, 1);
  }
}

function handleSubmit() {
  // eslint-disable-next-line no-console
  console.log(checks.value, props.roleId);
  // request

  window.$message?.success?.($t('common.modifySuccess'));

  closeModal();
}

function init() {
  getAllButtons();
  getChecks();
}

// init
init();
</script>

<template>
  <ElDialog v-model="visible" :title="title" preset="card" class="w-480px">
    <ElTree
      v-model:checked-keys="checks"
      :data="tree"
      node-key="id"
      show-checkbox
      class="h-280px overflow-y-auto"
      :default-checked-keys="checks"
      @check-change="checkChange"
    />
    <template #footer>
      <ElSpace class="w-full justify-end">
        <ElButton size="small" class="mt-16px" @click="closeModal">{{ $t('common.cancel') }}</ElButton>
        <ElButton type="primary" size="small" class="mt-16px" @click="handleSubmit">
          {{ $t('common.confirm') }}
        </ElButton>
      </ElSpace>
    </template>
  </ElDialog>
</template>

<style scoped></style>
