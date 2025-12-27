<script setup lang="tsx">
import { connectionStatusOptions, enableStatusOptions, protocolTypeOptions } from '@/constants/business';
import { useForm } from '@/hooks/common/form';
import { translateOptions } from '@/utils/common';
import { $t } from '@/locales';

defineOptions({ name: 'DeviceSearch' });

interface Emits {
  (e: 'reset'): void;
  (e: 'search'): void;
}

const emit = defineEmits<Emits>();

const { formRef, validate, restoreValidation } = useForm();

const model = defineModel<Api.Device.DeviceSearchParams>('model', { required: true });

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
      <ElCollapseItem :title="$t('common.search')" name="device-search">
        <ElForm ref="formRef" :model="model" label-position="right" :label-width="80">
          <ElRow :gutter="16">
            <ElCol :xl="4" :lg="6" :md="8" :sm="12" :xs="24">
              <ElFormItem :label="$t('page.device.deviceName')" prop="deviceName">
                <ElInput v-model="model.deviceName" :placeholder="$t('page.device.form.deviceName')" clearable />
              </ElFormItem>
            </ElCol>
            <ElCol :xl="4" :lg="6" :md="8" :sm="12" :xs="24">
              <ElFormItem :label="$t('page.device.deviceId')" prop="deviceId">
                <ElInput v-model="model.deviceId" :placeholder="$t('page.device.form.deviceId')" clearable />
              </ElFormItem>
            </ElCol>
            <ElCol :xl="4" :lg="6" :md="8" :sm="12" :xs="24">
              <ElFormItem :label="$t('page.device.protocol')" prop="protocolType">
                <ElSelect v-model="model.protocolType" clearable :placeholder="$t('page.device.form.protocol')">
                  <ElOption
                    v-for="{ label, value } in translateOptions(protocolTypeOptions)"
                    :key="value"
                    :label="label"
                    :value="value"
                  />
                </ElSelect>
              </ElFormItem>
            </ElCol>
            <ElCol :xl="4" :lg="6" :md="8" :sm="12" :xs="24">
              <ElFormItem :label="$t('page.device.connection')" prop="connectionStatus">
                <ElSelect v-model="model.connectionStatus" clearable :placeholder="$t('page.device.connection')">
                  <ElOption
                    v-for="{ label, value } in translateOptions(connectionStatusOptions)"
                    :key="value"
                    :label="label"
                    :value="value"
                  />
                </ElSelect>
              </ElFormItem>
            </ElCol>
            <ElCol :xl="4" :lg="6" :md="8" :sm="12" :xs="24">
              <ElFormItem :label="$t('page.device.enabled')" prop="enabled">
                <ElSelect v-model="model.enabled" clearable :placeholder="$t('page.device.form.enabled')">
                  <ElOption
                    v-for="{ label, value } in translateOptions(enableStatusOptions)"
                    :key="value"
                    :label="label"
                    :value="value === '1' ? true : false"
                  />
                </ElSelect>
              </ElFormItem>
            </ElCol>
            <ElCol :xl="4" :lg="24" :md="24" :sm="24" :xs="24">
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
