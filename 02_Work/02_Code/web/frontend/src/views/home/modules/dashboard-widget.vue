<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, shallowRef } from 'vue';
import * as echarts from 'echarts';
import { fetchGetDashboardSummary } from '@/service/api';

const props = defineProps<{
  widgetId: string;
  widgetType: string;
}>();

const emit = defineEmits<{
  remove: [id: string];
}>();

// chart container and instance
const chartRef = ref<HTMLDivElement>();
const chart = shallowRef<echarts.ECharts>();

// dashboard data
const summaryData = ref<Api.Monitor.DashboardSummary | null>(null);
const loading = ref(false);

// widget title
const widgetTitle = computed(() => {
  const titles: Record<string, string> = {
    summary: 'System Overview',
    deviceStatus: 'Device Status',
    trend: 'Data Trend',
    alerts: 'Alerts'
  };
  return titles[props.widgetType] || 'Unknown';
});

// init chart
function initChart() {
  if (!chartRef.value) return;
  chart.value = echarts.init(chartRef.value);
  window.addEventListener('resize', handleResize);
}

// handle window resize
function handleResize() {
  chart.value?.resize();
}

// load data
async function loadData() {
  loading.value = true;
  try {
    const { data, error } = await fetchGetDashboardSummary();
    if (!error && data) {
      summaryData.value = data;
      if (props.widgetType === 'deviceStatus') {
        renderDeviceStatusChart();
      } else if (props.widgetType === 'trend') {
        renderTrendChart();
      }
    }
  } finally {
    loading.value = false;
  }
}

// render device status pie chart
function renderDeviceStatusChart() {
  if (!chart.value || !summaryData.value) return;

  const { onlineDevices, offlineDevices, errorDevices } = summaryData.value;

  const option: echarts.EChartsOption = {
    tooltip: {
      trigger: 'item',
      formatter: '{b}: {c} ({d}%)'
    },
    legend: {
      orient: 'vertical',
      left: 'left'
    },
    series: [
      {
        name: 'Device Status',
        type: 'pie',
        radius: ['40%', '70%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 10,
          borderColor: '#fff',
          borderWidth: 2
        },
        label: {
          show: false,
          position: 'center'
        },
        emphasis: {
          label: {
            show: true,
            fontSize: 16,
            fontWeight: 'bold'
          }
        },
        labelLine: {
          show: false
        },
        data: [
          { value: onlineDevices, name: 'Online', itemStyle: { color: '#67C23A' } },
          { value: offlineDevices, name: 'Offline', itemStyle: { color: '#909399' } },
          { value: errorDevices, name: 'Error', itemStyle: { color: '#F56C6C' } }
        ]
      }
    ]
  };

  chart.value.setOption(option);
}

// render trend chart
function renderTrendChart() {
  if (!chart.value || !summaryData.value) return;

  const trendData = summaryData.value.collectionTrend || [];
  const times = trendData.map(p => {
    const d = new Date(p.time);
    return `${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}`;
  });
  const values = trendData.map(p => p.value);

  const option: echarts.EChartsOption = {
    tooltip: {
      trigger: 'axis'
    },
    grid: {
      left: '3%',
      right: '4%',
      bottom: '3%',
      top: '10%',
      containLabel: true
    },
    xAxis: {
      type: 'category',
      boundaryGap: false,
      data: times.length > 0 ? times : ['00:00']
    },
    yAxis: {
      type: 'value',
      name: 'Points'
    },
    series: [
      {
        name: 'Collection Points',
        type: 'line',
        smooth: true,
        areaStyle: {
          color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
            { offset: 0, color: 'rgba(64, 158, 255, 0.5)' },
            { offset: 1, color: 'rgba(64, 158, 255, 0.1)' }
          ])
        },
        data: values.length > 0 ? values : [0]
      }
    ]
  };

  chart.value.setOption(option);
}

// handle remove
function handleRemove() {
  emit('remove', props.widgetId);
}

onMounted(() => {
  initChart();
  loadData();
});

onUnmounted(() => {
  window.removeEventListener('resize', handleResize);
  chart.value?.dispose();
});
</script>

<template>
  <ElCard class="dashboard-widget h-full" :body-style="{ padding: '12px', height: 'calc(100% - 40px)' }">
    <template #header>
      <div class="flex items-center justify-between">
        <span class="font-medium">{{ widgetTitle }}</span>
        <div class="flex items-center gap-2">
          <ElButton size="small" text @click="loadData">
            <SvgIcon icon="ep:refresh" />
          </ElButton>
          <ElButton size="small" text type="danger" @click="handleRemove">
            <SvgIcon icon="ep:close" />
          </ElButton>
        </div>
      </div>
    </template>

    <div v-loading="loading" class="h-full">
      <!-- System Overview -->
      <template v-if="widgetType === 'summary' && summaryData">
        <div class="grid grid-cols-2 h-full gap-4 p-2">
          <div class="flex flex-col items-center justify-center rounded-lg bg-blue-50 p-4 dark:bg-blue-900/20">
            <div class="text-32px text-blue-500 font-bold">{{ summaryData.totalDevices }}</div>
            <div class="mt-1 text-gray-500">Total Devices</div>
          </div>
          <div class="flex flex-col items-center justify-center rounded-lg bg-green-50 p-4 dark:bg-green-900/20">
            <div class="text-32px text-green-500 font-bold">{{ summaryData.onlineDevices }}</div>
            <div class="mt-1 text-gray-500">Online Devices</div>
          </div>
          <div class="flex flex-col items-center justify-center rounded-lg bg-orange-50 p-4 dark:bg-orange-900/20">
            <div class="text-32px text-orange-500 font-bold">{{ summaryData.todayDataPoints.toLocaleString() }}</div>
            <div class="mt-1 text-gray-500">Today Data Points</div>
          </div>
          <div class="flex flex-col items-center justify-center rounded-lg bg-purple-50 p-4 dark:bg-purple-900/20">
            <div class="text-32px text-purple-500 font-bold">{{ summaryData.onlineRate.toFixed(1) }}%</div>
            <div class="mt-1 text-gray-500">Online Rate</div>
          </div>
        </div>
      </template>

      <!-- Device Status Pie Chart -->
      <template v-else-if="widgetType === 'deviceStatus'">
        <div ref="chartRef" class="h-full w-full" />
      </template>

      <!-- Data Trend Chart -->
      <template v-else-if="widgetType === 'trend'">
        <div ref="chartRef" class="h-full w-full" />
      </template>

      <!-- Alerts -->
      <template v-else-if="widgetType === 'alerts'">
        <div class="h-full overflow-auto">
          <div class="h-full flex items-center justify-center text-gray-400">
            <div class="text-center">
              <SvgIcon icon="ep:warning" class="mb-2 text-48px" />
              <div>No alerts</div>
            </div>
          </div>
        </div>
      </template>
    </div>
  </ElCard>
</template>

<style scoped>
.dashboard-widget {
  height: 100%;
  min-height: 200px;
}
</style>
