<script setup lang="ts">
import { ref, watch } from 'vue';
import { fetchGetDeviceLatest } from '@/service/api';
import { $t } from '@/locales';

const props = defineProps<{
  visible: boolean;
  deviceId?: string;
}>();

const emit = defineEmits<{
  'update:visible': [value: boolean];
}>();

const loading = ref(false);
const deviceDetail = ref<Api.Monitor.DeviceDetailData | null>(null);

watch(
  () => props.visible,
  async val => {
    if (val && props.deviceId) {
      await loadDeviceDetail();
    }
  }
);

async function loadDeviceDetail() {
  if (!props.deviceId) return;
  loading.value = true;
  try {
    const { data, error } = await fetchGetDeviceLatest(props.deviceId);
    if (!error && data) {
      deviceDetail.value = data;
    }
  } finally {
    loading.value = false;
  }
}

function handleClose() {
  emit('update:visible', false);
  deviceDetail.value = null;
}

function formatValue(value: unknown, unit?: string) {
  if (value === null || value === undefined) return '-';
  if (typeof value === 'number') {
    const formatted = Number.isInteger(value) ? value : value.toFixed(2);
    return unit ? `${formatted} ${unit}` : formatted;
  }
  return String(value);
}

function formatTime(time: string | undefined) {
  if (!time) return '-';
  return new Date(time).toLocaleString();
}

function getStatusConfig(status: string) {
  const map: Record<string, { type: 'success' | 'danger' | 'warning' | 'info'; textKey: string }> = {
    online: { type: 'success', textKey: 'page.monitor.deviceCard.statusOnline' },
    offline: { type: 'danger', textKey: 'page.monitor.deviceCard.statusOffline' },
    warning: { type: 'warning', textKey: 'page.monitor.deviceCard.statusError' }
  };
  const config = map[status] || { type: 'info', textKey: 'page.monitor.deviceCard.statusUnknown' };
  return {
    ...config,
    text: $t(config.textKey as App.I18n.I18nKey)
  };
}
</script>

<template>
  <ElDialog :model-value="visible" :title="$t('page.monitor.deviceDetail.title')" width="700px" @close="handleClose">
    <div v-loading="loading" class="min-h-50">
      <template v-if="deviceDetail">
        <ElDescriptions :column="2" border>
          <ElDescriptionsItem :label="$t('page.monitor.deviceDetail.deviceName')">
            {{ deviceDetail.deviceName }}
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.monitor.deviceDetail.deviceId')">
            <span class="font-mono">{{ deviceDetail.deviceId }}</span>
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.monitor.deviceDetail.status')">
            <ElTag :type="getStatusConfig(deviceDetail.connectionStatus).type">
              {{ getStatusConfig(deviceDetail.connectionStatus).text }}
            </ElTag>
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.monitor.deviceDetail.deviceType')">
            {{ deviceDetail.deviceType || '-' }}
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.monitor.deviceDetail.protocol')">
            {{ deviceDetail.protocolType || '-' }}
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.monitor.deviceDetail.location')">
            {{ deviceDetail.location || '-' }}
          </ElDescriptionsItem>
          <ElDescriptionsItem :label="$t('page.monitor.deviceDetail.lastConnect')" :span="2">
            {{ formatTime(deviceDetail.lastConnectTime) }}
          </ElDescriptionsItem>
        </ElDescriptions>

        <div v-if="deviceDetail.allTags && deviceDetail.allTags.length > 0" class="mt-4">
          <h4 class="mb-2 font-bold">{{ $t('page.monitor.deviceDetail.tagValues') }}</h4>
          <ElTable :data="deviceDetail.allTags" border stripe max-height="300">
            <ElTableColumn prop="tagName" :label="$t('page.monitor.deviceDetail.tagName')" width="180" />
            <ElTableColumn :label="$t('page.monitor.deviceDetail.value')">
              <template #default="{ row }">{{ formatValue(row.value, row.unit) }}</template>
            </ElTableColumn>
            <ElTableColumn prop="quality" :label="$t('page.monitor.deviceDetail.quality')" width="100" />
            <ElTableColumn :label="$t('page.monitor.deviceDetail.time')" width="180">
              <template #default="{ row }">{{ formatTime(row.timestamp) }}</template>
            </ElTableColumn>
          </ElTable>
        </div>
      </template>
      <ElEmpty v-else-if="!loading" :description="$t('page.monitor.deviceDetail.noData')" />
    </div>
  </ElDialog>
</template>

<style scoped></style>
