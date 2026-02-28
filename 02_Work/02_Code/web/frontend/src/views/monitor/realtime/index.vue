<script setup lang="ts">
import { computed, onActivated, onDeactivated, onMounted, onUnmounted, ref } from 'vue';
import { fetchGetDevicesLatest } from '@/service/api';
import { $t } from '@/locales';
import DeviceCard from './device-card.vue';
import DeviceDetailDialog from './device-detail-dialog.vue';

defineOptions({ name: 'MonitorRealtime' });

const devices = ref<Api.Monitor.DeviceMonitorData[]>([]);
const loading = ref(false);
const filterStatus = ref<string>('');
const detailVisible = ref(false);
const selectedDeviceId = ref<string>();
const autoRefresh = ref(true);
const refreshInterval = ref(10);
let refreshTimer: ReturnType<typeof setInterval> | null = null;

const statusOptions = computed(() => [
  { label: $t('page.monitor.realtime.statusAll'), value: '' },
  { label: $t('page.monitor.realtime.statusOnline'), value: 'online' },
  { label: $t('page.monitor.realtime.statusOffline'), value: 'offline' }
]);

async function loadDevices() {
  loading.value = true;
  try {
    const params: { connectionStatus?: string } = {};
    if (filterStatus.value) {
      params.connectionStatus = filterStatus.value;
    }
    const { data, error } = await fetchGetDevicesLatest(params);
    if (!error && data) {
      devices.value = data;
    }
  } finally {
    loading.value = false;
  }
}

function handleCardClick(device: Api.Monitor.DeviceMonitorData) {
  selectedDeviceId.value = device.id;
  detailVisible.value = true;
}

function startAutoRefresh() {
  stopAutoRefresh();
  if (autoRefresh.value) {
    refreshTimer = setInterval(() => {
      loadDevices();
    }, refreshInterval.value * 1000);
  }
}

function stopAutoRefresh() {
  if (refreshTimer) {
    clearInterval(refreshTimer);
    refreshTimer = null;
  }
}

function toggleAutoRefresh() {
  autoRefresh.value = !autoRefresh.value;
  if (autoRefresh.value) {
    startAutoRefresh();
  } else {
    stopAutoRefresh();
  }
}

// 首次挂载时加载数据并启动定时器
onMounted(() => {
  loadDevices();
  startAutoRefresh();
});

// 组件销毁时停止定时器（非 keep-alive 场景）
onUnmounted(() => {
  stopAutoRefresh();
});

// keep-alive 场景：页面激活时恢复定时器
onActivated(() => {
  if (autoRefresh.value) {
    startAutoRefresh();
  }
});

// keep-alive 场景：页面离开时停止定时器
onDeactivated(() => {
  stopAutoRefresh();
});
</script>

<template>
  <div class="h-full flex flex-col">
    <div class="flex items-center gap-4 border-b p-4">
      <ElSelect
        v-model="filterStatus"
        :placeholder="$t('page.monitor.realtime.filterStatus')"
        class="w-30"
        @change="loadDevices"
      >
        <ElOption v-for="opt in statusOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
      </ElSelect>
      <ElButton :icon="autoRefresh ? 'Pause' : 'VideoPlay'" @click="toggleAutoRefresh">
        {{ autoRefresh ? $t('page.monitor.realtime.pause') : $t('page.monitor.realtime.resume') }}
      </ElButton>
      <ElButton icon="Refresh" :loading="loading" @click="loadDevices">
        {{ $t('page.monitor.realtime.refresh') }}
      </ElButton>
      <span class="ml-auto text-sm text-gray-500">
        {{ $t('page.monitor.realtime.autoRefresh') }}:
        {{ autoRefresh ? `${refreshInterval}s` : $t('page.monitor.realtime.autoRefreshOff') }} |
        {{ $t('page.monitor.realtime.deviceCount') }}: {{ devices.length }}
      </span>
    </div>
    <div class="flex-1 overflow-auto p-4">
      <div v-loading="loading" class="grid grid-cols-1 gap-4 lg:grid-cols-3 md:grid-cols-2 xl:grid-cols-4">
        <DeviceCard v-for="device in devices" :key="device.id" :device="device" @click="handleCardClick(device)" />
        <ElEmpty v-if="!loading && devices.length === 0" :description="$t('page.monitor.realtime.noDevicesFound')" />
      </div>
    </div>
    <DeviceDetailDialog v-model:visible="detailVisible" :device-id="selectedDeviceId" />
  </div>
</template>

<style scoped></style>
