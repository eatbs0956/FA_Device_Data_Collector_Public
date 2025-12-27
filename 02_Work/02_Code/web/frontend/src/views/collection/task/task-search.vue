<script setup lang="tsx">
import { taskStatusOptions, taskTypeOptions } from '@/constants/business';
import { useForm } from '@/hooks/common/form';
import { translateOptions } from '@/utils/common';
import { $t } from '@/locales';

defineOptions({ name: 'TaskSearch' });

interface Emits {
  (e: 'reset'): void;
  (e: 'search'): void;
}

const emit = defineEmits<Emits>();

const { formRef, validate, restoreValidation } = useForm();

const model = defineModel<Api.CollectionTask.CollectionTaskSearchParams>('model', { required: true });

async function reset() {
  await restoreValidation();
  emit('reset');
}

async function search() {
  await validate();
  emit('search');
}
</script>

<template>
  <ElCard class="card-wrapper">
    <ElCollapse>
      <ElCollapseItem :title="$t('common.search')" name="task-search">
        <ElForm ref="formRef" :model="model" label-position="right" :label-width="80">
          <ElRow :gutter="16">
            <ElCol :xl="4" :lg="6" :md="8" :sm="12" :xs="24">
              <ElFormItem :label="$t('page.collectionTask.name')" prop="name">
                <ElInput v-model="model.name" :placeholder="$t('page.collectionTask.form.name')" clearable />
              </ElFormItem>
            </ElCol>
            <ElCol :xl="4" :lg="6" :md="8" :sm="12" :xs="24">
              <ElFormItem :label="$t('page.collectionTask.taskType')" prop="taskType">
                <ElSelect v-model="model.taskType" clearable :placeholder="$t('page.collectionTask.form.taskType')">
                  <ElOption
                    v-for="{ label, value } in translateOptions(taskTypeOptions)"
                    :key="value"
                    :label="label"
                    :value="value"
                  />
                </ElSelect>
              </ElFormItem>
            </ElCol>
            <ElCol :xl="4" :lg="6" :md="8" :sm="12" :xs="24">
              <ElFormItem :label="$t('page.collectionTask.status')" prop="status">
                <ElSelect
                  v-model="model.status"
                  clearable
                  :placeholder="$t('page.collectionTask.taskStatusOptions.draft')"
                >
                  <ElOption
                    v-for="{ label, value } in translateOptions(taskStatusOptions)"
                    :key="value"
                    :label="label"
                    :value="value"
                  />
                </ElSelect>
              </ElFormItem>
            </ElCol>
            <ElCol :xl="12" :lg="6" :md="8" :sm="24" :xs="24">
              <ElFormItem label-width="0">
                <ElSpace class="w-full justify-end">
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
                </ElSpace>
              </ElFormItem>
            </ElCol>
          </ElRow>
        </ElForm>
      </ElCollapseItem>
    </ElCollapse>
  </ElCard>
</template>

<style scoped></style>
