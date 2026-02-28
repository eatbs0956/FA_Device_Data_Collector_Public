<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, shallowRef } from 'vue';
import * as echarts from 'echarts';
import { fetchGetStatisticsByDevices, fetchGetStatisticsByGroups, fetchGetStatisticsByNodes } from '@/service/api';
import { $t } from '@/locales';

defineOptions({ name: 'MonitorStatistics' });

type StatsDimension = 'device' | 'group' | 'node';
const dimension = ref<StatsDimension>('device');

const dimensionOptions = computed(() => [
  { label: $t('page.monitor.statistics.byDevice'), value: 'device' },
  { label: $t('page.monitor.statistics.byGroup'), value: 'group' },
  { label: $t('page.monitor.statistics.byNode'), value: 'node' }
]);

const timeRangeShortcuts = computed(() => [
  { text: $t('page.monitor.historical.last1hour'), value: () => [Date.now() - 60 * 60 * 1000, Date.now()] },
  { text: $t('page.monitor.historical.last1day'), value: () => [Date.now() - 24 * 60 * 60 * 1000, Date.now()] },
  { text: $t('page.monitor.historical.last7days'), value: () => [Date.now() - 7 * 24 * 60 * 60 * 1000, Date.now()] },
  { text: $t('page.monitor.historical.last30days'), value: () => [Date.now() - 30 * 24 * 60 * 60 * 1000, Date.now()] }
]);

const loading = ref(false);
const chartRef = ref<HTMLDivElement>();
const chart = shallowRef<echarts.ECharts>();
const tableData = ref<Api.Monitor.StatisticsResult[]>([]);
const timeRange = ref<[number, number]>([Date.now() - 24 * 60 * 60 * 1000, Date.now()]);
const viewType = ref<'chart' | 'table'>('chart');

function initChart() {
  if (!chartRef.value) return;
  chart.value = echarts.init(chartRef.value);
  window.addEventListener('resize', handleResize);
}

function handleResize() {
  chart.value?.resize();
}

function updateChart() {
  if (!chart.value || tableData.value.length === 0) {
    chart.value?.clear();
    return;
  }
  const names = tableData.value.map(item => item.dimensionName);
  const pointCounts = tableData.value.map(item => {
    return item.items.reduce((sum, i) => sum + i.dataPointCount, 0);
  });
  const chartTitle = dimensionOptions.value.find(d => d.value === dimension.value)?.label || '';
  const option: echarts.EChartsOption = {
    title: { text: chartTitle, left: 'center' },
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
    grid: { left: '3%', right: '4%', bottom: '15%', top: '15%', containLabel: true },
    xAxis: { type: 'category', data: names, axisLabel: { rotate: 30, interval: 0 } },
    yAxis: { type: 'value', name: $t('page.monitor.statistics.pointCount') },
    series: [{ name: $t('page.monitor.statistics.pointCount'), type: 'bar', data: pointCounts }]
  };
  chart.value.setOption(option, true);
}

async function loadStatistics() {
  const start = new Date(timeRange.value[0]).toISOString();
  const end = new Date(timeRange.value[1]).toISOString();
  loading.value = true;
  try {
    let result: { data: Api.Monitor.StatisticsResult[] | null; error: unknown };
    switch (dimension.value) {
      case 'device':
        result = await fetchGetStatisticsByDevices({ start, end });
        break;
      case 'group':
        result = await fetchGetStatisticsByGroups({ start, end });
        break;
      case 'node':
        result = await fetchGetStatisticsByNodes({ start, end });
        break;
      default:
        return;
    }
    if (!result.error && result.data) {
      tableData.value = result.data;
      if (viewType.value === 'chart') {
        updateChart();
      }
    }
  } finally {
    loading.value = false;
  }
}

function handleDimensionChange() {
  loadStatistics();
}

function handleTimeRangeChange() {
  loadStatistics();
}

function handleViewChange(type: 'chart' | 'table') {
  viewType.value = type;
  if (type === 'chart') {
    setTimeout(() => {
      initChart();
      updateChart();
    }, 100);
  }
}

function handleExport() {
  if (tableData.value.length === 0) {
    window.$message?.warning($t('page.monitor.statistics.noDataToExport'));
    return;
  }
  const header = [
    $t('page.monitor.statistics.dimension'),
    $t('page.monitor.statistics.name'),
    $t('page.monitor.statistics.totalPoints'),
    $t('page.monitor.statistics.onlineDevices')
  ];
  const rows = tableData.value.map(item => {
    const totalPoints = item.items.reduce((sum, i) => sum + i.dataPointCount, 0);
    const avgDevices =
      item.items.length > 0
        ? Math.round(item.items.reduce((sum, i) => sum + (i.onlineDeviceCount || 0), 0) / item.items.length)
        : 0;
    return [item.dimension, item.dimensionName, totalPoints.toString(), avgDevices.toString()];
  });
  const csvContent = [header, ...rows].map(row => row.join(',')).join('\n');
  const bom = '\uFEFF';
  const blob = new Blob([bom, csvContent], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = `statistics_${dimension.value}_${new Date().toISOString().slice(0, 10)}.csv`;
  link.click();
  URL.revokeObjectURL(url);
}

function getTotalPoints(row: Api.Monitor.StatisticsResult): number {
  return row.items.reduce((sum, i) => sum + i.dataPointCount, 0);
}

function getAvgDevices(row: Api.Monitor.StatisticsResult): number {
  if (row.items.length === 0) return 0;
  return Math.round(row.items.reduce((sum, i) => sum + (i.onlineDeviceCount || 0), 0) / row.items.length);
}

onMounted(() => {
  initChart();
  loadStatistics();
});

onUnmounted(() => {
  window.removeEventListener('resize', handleResize);
  chart.value?.dispose();
});
</script>

<template>
  <div class="h-full flex flex-col">
    <div class="flex items-center justify-between border-b p-4">
      <div class="flex items-center gap-4">
        <ElRadioGroup v-model="dimension" @change="handleDimensionChange">
          <ElRadioButton v-for="opt in dimensionOptions" :key="opt.value" :value="opt.value">
            {{ opt.label }}
          </ElRadioButton>
        </ElRadioGroup>
        <ElDatePicker
          v-model="timeRange"
          type="datetimerange"
          :shortcuts="timeRangeShortcuts"
          :range-separator="$t('page.monitor.statistics.to')"
          :start-placeholder="$t('page.monitor.statistics.startTime')"
          :end-placeholder="$t('page.monitor.statistics.endTime')"
          value-format="x"
          @change="handleTimeRangeChange"
        />
      </div>
      <div class="flex items-center gap-2">
        <ElButtonGroup>
          <ElButton :type="viewType === 'chart' ? 'primary' : 'default'" @click="handleViewChange('chart')">
            <template #icon><SvgIcon icon="ep:data-analysis" /></template>
            {{ $t('page.monitor.statistics.chart') }}
          </ElButton>
          <ElButton :type="viewType === 'table' ? 'primary' : 'default'" @click="handleViewChange('table')">
            <template #icon><SvgIcon icon="ep:grid" /></template>
            {{ $t('page.monitor.statistics.table') }}
          </ElButton>
        </ElButtonGroup>
        <ElButton :loading="loading" @click="loadStatistics">
          <template #icon><SvgIcon icon="ep:refresh" /></template>
          {{ $t('page.monitor.statistics.refresh') }}
        </ElButton>
        <ElButton type="success" @click="handleExport">
          <template #icon><SvgIcon icon="ep:download" /></template>
          {{ $t('page.monitor.statistics.export') }}
        </ElButton>
      </div>
    </div>
    <div class="flex-1 overflow-hidden p-4">
      <div v-show="viewType === 'chart'" ref="chartRef" class="h-full w-full" />
      <ElTable v-show="viewType === 'table'" v-loading="loading" :data="tableData" height="100%" border stripe>
        <ElTableColumn prop="dimensionName" :label="$t('page.monitor.statistics.name')" min-width="200" />
        <ElTableColumn :label="$t('page.monitor.statistics.totalPoints')" width="150" align="right">
          <template #default="{ row }">
            {{ getTotalPoints(row).toLocaleString() }}
          </template>
        </ElTableColumn>
        <ElTableColumn :label="$t('page.monitor.statistics.onlineDevices')" width="120" align="right">
          <template #default="{ row }">
            {{ getAvgDevices(row) }}
          </template>
        </ElTableColumn>
      </ElTable>
    </div>
  </div>
</template>

<style scoped></style>

<style scoped></style>
