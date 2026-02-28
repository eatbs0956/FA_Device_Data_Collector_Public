<script setup lang="ts">
import { computed } from 'vue';
import { $t } from '@/locales';

const props = defineProps<{
  device: Api.Monitor.DeviceMonitorData;
}>();

const emit = defineEmits<{
  click: [device: Api.Monitor.DeviceMonitorData];
}>();

// Connection status mapping
const statusConfig = computed(() => {
  const statusMap: Record<string, { type: 'success' | 'warning' | 'danger' | 'info'; textKey: string; icon: string }> =
    {
      Connected: { type: 'success', textKey: 'page.monitor.deviceCard.statusOnline', icon: 'ep:circle-check-filled' },
      Disconnected: { type: 'warning', textKey: 'page.monitor.deviceCard.statusOffline', icon: 'ep:warning-filled' },
      Error: { type: 'danger', textKey: 'page.monitor.deviceCard.statusError', icon: 'ep:circle-close-filled' },
      Unknown: { type: 'info', textKey: 'page.monitor.deviceCard.statusUnknown', icon: 'ep:question-filled' }
    };
  const config = statusMap[props.device.connectionStatus] || statusMap.Unknown;
  return {
    ...config,
    text: $t(config.textKey as App.I18n.I18nKey)
  };
});

// Format time
function formatTime(time?: string) {
  if (!time) return '-';
  return new Date(time).toLocaleString('zh-CN');
}

// Format value
function formatValue(value: unknown, unit?: string) {
  if (value === null || value === undefined) return '-';
  if (typeof value === 'number') {
    const formatted = Number.isInteger(value) ? value : value.toFixed(2);
    return unit ? ` ${unit}` : formatted;
  }
  return String(value);
}

function handleClick() {
  emit('click', props.device);
}
</script>

<template>
  <div
    class="device-card cursor-pointer transition-all hover:shadow-lg"
    :class="{ 'border-l-4 border-l-green-500': device.connectionStatus === 'Connected' }"
    @click="handleClick"
  >
    <ElCard shadow="hover" class="h-full">
      <template #header>
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <icon-park-solid:hard-disk class="text-xl text-primary" />
            <span class="max-w-200px truncate font-medium" :title="device.deviceName">
              {{ device.deviceName }}
            </span>
          </div>
          <ElTag :type="statusConfig.type" size="small">
            {{ statusConfig.text }}
          </ElTag>
        </div>
      </template>

      <div class="space-y-3">
        <!-- Device ID -->
        <div class="flex items-center justify-between text-sm text-gray-500">
          <span>{{ $t('page.monitor.deviceCard.deviceId') }}</span>
          <span class="text-gray-700 font-mono">{{ device.deviceId }}</span>
        </div>

        <!-- Belongs to group/node -->
        <div v-if="device.groupName || device.nodeName" class="flex items-center justify-between text-sm text-gray-500">
          <span>{{ $t('page.monitor.deviceCard.belongTo') }}</span>
          <span class="text-gray-700">
            {{ device.groupName || device.nodeName || '-' }}
          </span>
        </div>

        <!-- Last update time -->
        <div class="flex items-center justify-between text-sm text-gray-500">
          <span>{{ $t('page.monitor.deviceCard.updateTime') }}</span>
          <span class="text-xs text-gray-700">{{ formatTime(device.lastUpdateTime) }}</span>
        </div>

        <!-- Divider -->
        <ElDivider class="!my-2" />

        <!-- Key tag data -->
        <div v-if="device.keyTags.length > 0" class="space-y-2">
          <div class="mb-1 text-xs text-gray-400">{{ $t('page.monitor.deviceCard.keyIndicators') }}</div>
          <div v-for="tag in device.keyTags" :key="tag.tagName" class="flex items-center justify-between">
            <span class="max-w-100px truncate text-sm text-gray-600" :title="tag.displayName || tag.tagName">
              {{ tag.displayName || tag.tagName }}
            </span>
            <span class="text-primary font-medium">
              {{ formatValue(tag.value, tag.unit) }}
            </span>
          </div>
        </div>

        <!-- No data hint -->
        <div v-else class="py-2 text-center text-sm text-gray-400">{{ $t('page.monitor.deviceCard.noData') }}</div>
      </div>
    </ElCard>
  </div>
</template>

<style scoped>
.device-card {
  min-height: 200px;
}
</style>
