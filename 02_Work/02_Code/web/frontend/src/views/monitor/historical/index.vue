<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, shallowRef } from 'vue';
import * as echarts from 'echarts';
import { fetchGetDeviceHistory } from '@/service/api';
import { $t } from '@/locales';
import DeviceTree from './device-tree.vue';

defineOptions({ name: 'MonitorHistorical' });

const timeRangeShortcuts = computed(() => [
  { text: $t('page.monitor.historical.last15min'), value: () => [Date.now() - 15 * 60 * 1000, Date.now()] },
  { text: $t('page.monitor.historical.last1hour'), value: () => [Date.now() - 60 * 60 * 1000, Date.now()] },
  { text: $t('page.monitor.historical.last6hours'), value: () => [Date.now() - 6 * 60 * 60 * 1000, Date.now()] },
  { text: $t('page.monitor.historical.last1day'), value: () => [Date.now() - 24 * 60 * 60 * 1000, Date.now()] },
  { text: $t('page.monitor.historical.last7days'), value: () => [Date.now() - 7 * 24 * 60 * 60 * 1000, Date.now()] }
]);

const loading = ref(false);
const chartRef = ref<HTMLDivElement>();
const chart = shallowRef<echarts.ECharts>();
const selectedDevice = ref<Api.Monitor.DeviceTreeNode | null>(null);
const selectedTags = ref<string[]>([]);
const timeRange = ref<[number, number]>([Date.now() - 60 * 60 * 1000, Date.now()]);
const aggregation = ref('none');

const aggregationOptions = computed(() => [
  { label: $t('page.monitor.historical.rawData'), value: 'none' },
  { label: $t('page.monitor.historical.avg1min'), value: 'mean_1m' },
  { label: $t('page.monitor.historical.avg5min'), value: 'mean_5m' },
  { label: $t('page.monitor.historical.avg1hour'), value: 'mean_1h' }
]);

const historyData = ref<Api.Monitor.HistoryDataResult | null>(null);

function initChart() {
  if (!chartRef.value) return;
  chart.value = echarts.init(chartRef.value);
  updateChart();
  window.addEventListener('resize', handleResize);
}

function handleResize() {
  chart.value?.resize();
}

function updateChart() {
  if (!chart.value) return;
  const series: echarts.SeriesOption[] = [];
  const legendData: string[] = [];
  if (historyData.value?.series && historyData.value.series.length > 0) {
    // Get all tag names from the first data point
    const firstPoint = historyData.value.series[0];
    const tagNames = Object.keys(firstPoint.values);

    tagNames.forEach(tagName => {
      legendData.push(tagName);
      series.push({
        name: tagName,
        type: 'line',
        smooth: true,
        symbol: 'none',
        data: historyData.value!.series.map(p => [new Date(p.timestamp).getTime(), p.values[tagName]])
      });
    });
  }
  const option: echarts.EChartsOption = {
    tooltip: { trigger: 'axis' },
    legend: { data: legendData, bottom: 0 },
    grid: { left: '3%', right: '4%', bottom: '10%', top: '5%', containLabel: true },
    xAxis: { type: 'time' },
    yAxis: { type: 'value' },
    dataZoom: [
      { type: 'inside', start: 0, end: 100 },
      { type: 'slider', start: 0, end: 100 }
    ],
    series
  };
  chart.value.setOption(option, true);
}

function handleDeviceSelect(node: Api.Monitor.DeviceTreeNode, tags: string[]) {
  selectedDevice.value = node;
  selectedTags.value = tags;
  loadHistoryData();
}

async function loadHistoryData() {
  if (!selectedDevice.value || selectedTags.value.length === 0) {
    historyData.value = null;
    updateChart();
    return;
  }
  const deviceIdMatch = selectedDevice.value.id.match(/device_(\d+)/);
  if (!deviceIdMatch) return;
  const deviceId = deviceIdMatch[1];
  const startTime = new Date(timeRange.value[0]).toISOString();
  const endTime = new Date(timeRange.value[1]).toISOString();
  loading.value = true;
  try {
    const { data, error } = await fetchGetDeviceHistory(deviceId, {
      start: startTime,
      end: endTime,
      tags: selectedTags.value.join(','),
      aggregateFn: aggregation.value === 'none' ? undefined : aggregation.value
    });
    if (!error && data) {
      historyData.value = data;
      updateChart();
    }
  } finally {
    loading.value = false;
  }
}

function handleTimeRangeChange() {
  loadHistoryData();
}
function handleAggregationChange() {
  loadHistoryData();
}

onMounted(() => {
  initChart();
});
onUnmounted(() => {
  window.removeEventListener('resize', handleResize);
  chart.value?.dispose();
});
</script>

<template>
  <div class="h-full flex">
    <div class="h-full w-260px flex-shrink-0">
      <DeviceTree @select="handleDeviceSelect" />
    </div>
    <div class="flex flex-col flex-1 overflow-hidden">
      <div class="flex items-center justify-between border-b p-3">
        <div class="flex items-center gap-4">
          <div class="flex items-center gap-2">
            <span class="text-gray-500">{{ $t('page.monitor.historical.timeRange') }}:</span>
            <ElDatePicker
              v-model="timeRange"
              type="datetimerange"
              :shortcuts="timeRangeShortcuts"
              range-separator="to"
              value-format="x"
              @change="handleTimeRangeChange"
            />
          </div>
          <div class="flex items-center gap-2">
            <span class="text-gray-500">{{ $t('page.monitor.historical.aggregation') }}:</span>
            <ElSelect v-model="aggregation" class="w-30" @change="handleAggregationChange">
              <ElOption v-for="opt in aggregationOptions" :key="opt.value" :label="opt.label" :value="opt.value" />
            </ElSelect>
          </div>
        </div>
        <ElButton type="primary" :loading="loading" @click="loadHistoryData">
          <template #icon><SvgIcon icon="ep:refresh" /></template>
          {{ $t('page.monitor.historical.refresh') }}
        </ElButton>
      </div>
      <div v-if="selectedDevice" class="bg-gray-50 px-3 py-2 dark:bg-gray-800">
        <span class="font-medium">{{ selectedDevice.label }}</span>
        <span class="ml-2 text-gray-500">
          {{ $t('page.monitor.historical.selectedTags', { count: selectedTags.length }) }}
        </span>
        <ElTag v-for="tag in selectedTags.slice(0, 5)" :key="tag" size="small" class="ml-2">{{ tag }}</ElTag>
        <span v-if="selectedTags.length > 5" class="ml-2 text-gray-400">
          {{ $t('page.monitor.historical.moreTags', { count: selectedTags.length - 5 }) }}
        </span>
      </div>
      <div class="relative flex-1 p-4">
        <div v-if="!selectedDevice" class="h-full flex items-center justify-center text-gray-400">
          <div class="text-center">
            <SvgIcon icon="ep:data-line" class="mb-2 text-48px" />
            <div>{{ $t('page.monitor.historical.selectDeviceHint') }}</div>
          </div>
        </div>
        <div v-else-if="loading" class="h-full flex items-center justify-center">
          <ElSkeleton :rows="10" animated />
        </div>
        <div v-else ref="chartRef" class="h-full w-full" />
      </div>
    </div>
  </div>
</template>

<style scoped></style>
