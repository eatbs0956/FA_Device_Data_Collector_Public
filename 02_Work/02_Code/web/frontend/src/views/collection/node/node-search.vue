<script setup lang="tsx">
import { nodeStatusOptions, platformTypeOptions } from '@/constants/business';
import { useForm } from '@/hooks/common/form';
import { translateOptions } from '@/utils/common';
import { $t } from '@/locales';

defineOptions({ name: 'NodeSearch' });

interface Emits {
  (e: 'reset'): void;
  (e: 'search'): void;
}

const emit = defineEmits<Emits>();

const { formRef, validate, restoreValidation } = useForm();

const model = defineModel<Api.EdgeNode.EdgeNodeSearchParams>('model', { required: true });

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
      <ElCollapseItem :title="$t('common.search')" name="node-search">
        <ElForm ref="formRef" :model="model" label-position="right" :label-width="80">
          <ElRow :gutter="16">
            <ElCol :xl="4" :lg="6" :md="8" :sm="12" :xs="24">
              <ElFormItem :label="$t('page.edgeNode.nodeName')" prop="nodeName">
                <ElInput v-model="model.nodeName" :placeholder="$t('page.edgeNode.form.nodeName')" clearable />
              </ElFormItem>
            </ElCol>
            <ElCol :xl="4" :lg="6" :md="8" :sm="12" :xs="24">
              <ElFormItem :label="$t('page.edgeNode.nodeId')" prop="nodeId">
                <ElInput v-model="model.nodeId" :placeholder="$t('page.edgeNode.form.nodeId')" clearable />
              </ElFormItem>
            </ElCol>
            <ElCol :xl="4" :lg="6" :md="8" :sm="12" :xs="24">
              <ElFormItem :label="$t('page.edgeNode.status')" prop="status">
                <ElSelect v-model="model.status" clearable :placeholder="$t('page.edgeNode.form.status')">
                  <ElOption
                    v-for="{ label, value } in translateOptions(nodeStatusOptions)"
                    :key="value"
                    :label="label"
                    :value="value"
                  />
                </ElSelect>
              </ElFormItem>
            </ElCol>
            <ElCol :xl="4" :lg="6" :md="8" :sm="12" :xs="24">
              <ElFormItem :label="$t('page.edgeNode.platform')" prop="platform">
                <ElSelect v-model="model.platform" clearable :placeholder="$t('page.edgeNode.form.platform')">
                  <ElOption
                    v-for="{ label, value } in translateOptions(platformTypeOptions)"
                    :key="value"
                    :label="label"
                    :value="value"
                  />
                </ElSelect>
              </ElFormItem>
            </ElCol>
            <ElCol :xl="8" :lg="24" :md="24" :sm="24" :xs="24">
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
