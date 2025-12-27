<script setup lang="ts">
import { computed } from 'vue';
import { enableStatusOptions } from '@/constants/business';
import { $t } from '@/locales';

defineOptions({
  name: 'RoleSearch'
});

interface Props {
  model: Api.SystemManage.RoleSearchParams;
}

const props = defineProps<Props>();

interface Emits {
  (e: 'update:model', model: Api.SystemManage.RoleSearchParams): void;
  (e: 'reset'): void;
  (e: 'search'): void;
}

const emit = defineEmits<Emits>();

const model = computed({
  get() {
    return props.model;
  },
  set(value) {
    emit('update:model', value);
  }
});

function reset() {
  emit('reset');
}

function search() {
  emit('search');
}
</script>

<template>
  <ElCard class="card-wrapper">
    <ElCollapse>
      <ElCollapseItem :title="$t('common.search')" name="role-search">
        <ElForm :model="model" label-width="80px">
          <ElRow :gutter="16">
            <ElCol :xs="24" :sm="12" :md="8" :lg="6">
              <ElFormItem :label="$t('page.manage.role.roleName')">
                <ElInput v-model="model.roleName" :placeholder="$t('page.manage.role.form.roleName')" clearable />
              </ElFormItem>
            </ElCol>
            <ElCol :xs="24" :sm="12" :md="8" :lg="6">
              <ElFormItem :label="$t('page.manage.role.roleCode')">
                <ElInput v-model="model.roleCode" :placeholder="$t('page.manage.role.form.roleCode')" clearable />
              </ElFormItem>
            </ElCol>
            <ElCol :xs="24" :sm="12" :md="8" :lg="6">
              <ElFormItem :label="$t('page.manage.role.roleStatus')">
                <ElSelect
                  v-model="model.status"
                  :placeholder="$t('page.manage.role.form.roleStatus')"
                  clearable
                  class="w-full"
                >
                  <ElOption
                    v-for="item in enableStatusOptions"
                    :key="item.value"
                    :label="$t(item.label)"
                    :value="item.value"
                  />
                </ElSelect>
              </ElFormItem>
            </ElCol>
            <ElCol :xs="24" :sm="12" :md="8" :lg="6">
              <ElFormItem>
                <ElButton type="primary" plain @click="search">
                  <template #icon>
                    <icon-ic-round-search class="text-icon" />
                  </template>
                  {{ $t('common.search') }}
                </ElButton>
                <ElButton @click="reset">
                  <template #icon>
                    <icon-ic-round-refresh class="text-icon" />
                  </template>
                  {{ $t('common.reset') }}
                </ElButton>
              </ElFormItem>
            </ElCol>
          </ElRow>
        </ElForm>
      </ElCollapseItem>
    </ElCollapse>
  </ElCard>
</template>

<style lang="scss" scoped></style>
