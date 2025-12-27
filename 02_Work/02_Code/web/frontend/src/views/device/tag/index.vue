<script setup lang="ts">
import { computed, onActivated, onMounted, ref } from 'vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import { useBoolean, useLoading } from '@sa/hooks';
import {
  dataTypeBackendToFrontendMap,
  dataTypeRecord,
  enableStatusRecord,
  protocolTypeFrontendToBackendMap,
  protocolTypeOptions
} from '@/constants/business';
import {
  fetchBatchDeleteTags,
  fetchBatchDisableTags,
  fetchBatchEnableTags,
  fetchDeleteTag,
  fetchExportTags,
  fetchGetDeviceListForTags,
  fetchGetTagList,
  fetchToggleTagEnabled
} from '@/service/api';
import { useAuth } from '@/hooks/business/auth';
import { $t } from '@/locales';
import TagListSearch from './tag-list-search.vue';
import TagListOperateDrawer, { type OperateType } from './tag-list-operate-drawer.vue';

const { hasAuth } = useAuth();
const { loading, startLoading, endLoading } = useLoading();
const { bool: drawerVisible, setTrue: openDrawer } = useBoolean();

// 设备列表
const deviceList = ref<Api.Device.Device[]>([]);
const selectedDevice = ref<Api.Device.Device | null>(null);
const deviceSearchKeyword = ref('');
const deviceProtocolFilter = ref<string>(''); // 协议类型过滤
const deviceEnabledFilter = ref<boolean>(false); // 只显示启用的设备

// 标签列表
const tagList = ref<Api.Device.Tag[]>([]);
const total = ref(0);
const tableSelectedKeys = ref<string[]>([]);

// 搜索参数
const searchParams = ref<Api.Device.TagSearchParams>({
  current: 1,
  size: 20,
  deviceId: undefined,
  tagName: undefined,
  enabled: undefined
});

// 编辑状态
const operateType = ref<OperateType>('add');
const editingData = ref<Api.Device.Tag | null>(null);

// 过滤后的设备列表
const filteredDeviceList = computed(() => {
  let filtered = deviceList.value;

  // 按名称/ID搜索
  if (deviceSearchKeyword.value) {
    const keyword = deviceSearchKeyword.value.toLowerCase();
    filtered = filtered.filter(
      device => device.deviceName.toLowerCase().includes(keyword) || device.deviceId.toLowerCase().includes(keyword)
    );
  }

  // 按协议类型过滤（前端选择的是代码'1','2'，需要转换为后端字符串'MODBUS_TCP'等）
  if (deviceProtocolFilter.value) {
    const backendProtocolType = protocolTypeFrontendToBackendMap[deviceProtocolFilter.value as Api.Device.ProtocolType];
    filtered = filtered.filter(device => device.protocolType === backendProtocolType);
  }

  // 按启用状态过滤
  if (deviceEnabledFilter.value) {
    filtered = filtered.filter(device => device.enabled);
  }

  return filtered;
});

// 获取设备列表
async function getDeviceList() {
  const { data, error } = await fetchGetDeviceListForTags();
  if (!error && data) {
    deviceList.value = data.records || [];
    // 如果之前选中的设备不在列表中，清空选择
    if (selectedDevice.value && !deviceList.value.find(d => d.id === selectedDevice.value!.id)) {
      selectedDevice.value = null;
    }
  }
}

// 选择设备
function handleSelectDevice(device: Api.Device.Device) {
  selectedDevice.value = device;
  searchParams.value.deviceId = device.id;
  searchParams.value.current = 1;
  getTagList();
}

// 获取标签列表
async function getTagList() {
  if (!selectedDevice.value) {
    tagList.value = [];
    total.value = 0;
    return;
  }

  startLoading();
  const { data, error } = await fetchGetTagList(searchParams.value);
  endLoading();

  if (!error && data) {
    tagList.value = data.records || [];
    total.value = data.total || 0;
  }
}

// 搜索
function handleSearch(params: Api.Device.TagSearchParams) {
  Object.assign(searchParams.value, params);
  searchParams.value.current = 1;
  getTagList();
}

// 重置
function handleReset() {
  searchParams.value = {
    current: 1,
    size: 20,
    deviceId: selectedDevice.value?.id,
    tagName: undefined,
    enabled: undefined
  };
  getTagList();
}

// 分页
function handlePageChange(page: number) {
  searchParams.value.current = page;
  getTagList();
}

function handleSizeChange(size: number) {
  searchParams.value.size = size;
  searchParams.value.current = 1;
  getTagList();
}

// 表格选择
function handleSelectionChange(selection: Api.Device.Tag[]) {
  tableSelectedKeys.value = selection.map(item => item.id);
}

// 新增标签
function handleAdd() {
  if (!selectedDevice.value) {
    ElMessage.warning($t('page.tag.selectDeviceFirst'));
    return;
  }
  operateType.value = 'add';
  editingData.value = null;
  openDrawer();
}

// 编辑标签
function handleEdit(row: Api.Device.Tag) {
  operateType.value = 'edit';
  editingData.value = row;
  openDrawer();
}

// 删除标签
async function handleDelete(id: string) {
  try {
    await ElMessageBox.confirm($t('common.confirmDelete'), $t('common.tip'), {
      type: 'warning'
    });
    const { error } = await fetchDeleteTag(id);
    if (!error) {
      ElMessage.success($t('common.deleteSuccess'));
      getTagList();
    }
  } catch {
    // cancelled
  }
}

// 批量删除
async function handleBatchDelete() {
  if (tableSelectedKeys.value.length === 0) {
    ElMessage.warning($t('common.pleaseSelect'));
    return;
  }

  try {
    await ElMessageBox.confirm(
      $t('page.tag.confirmBatchDelete', { count: tableSelectedKeys.value.length }),
      $t('common.tip'),
      { type: 'warning' }
    );
    const { error } = await fetchBatchDeleteTags(tableSelectedKeys.value);
    if (!error) {
      ElMessage.success($t('common.deleteSuccess'));
      tableSelectedKeys.value = [];
      getTagList();
    }
  } catch {
    // cancelled
  }
}

// 切换启用状态
async function handleToggleEnabled(row: Api.Device.Tag) {
  const newEnabled = !row.enabled;
  const { error } = await fetchToggleTagEnabled(row.id, newEnabled);
  if (!error) {
    ElMessage.success(newEnabled ? $t('common.enableSuccess') : $t('common.disableSuccess'));
    getTagList();
  }
}

// 批量启用
async function handleBatchEnable() {
  if (tableSelectedKeys.value.length === 0) {
    ElMessage.warning($t('common.pleaseSelect'));
    return;
  }
  const { error } = await fetchBatchEnableTags(tableSelectedKeys.value);
  if (!error) {
    ElMessage.success($t('common.enableSuccess'));
    tableSelectedKeys.value = [];
    getTagList();
  }
}

// 批量禁用
async function handleBatchDisable() {
  if (tableSelectedKeys.value.length === 0) {
    ElMessage.warning($t('common.pleaseSelect'));
    return;
  }
  const { error } = await fetchBatchDisableTags(tableSelectedKeys.value);
  if (!error) {
    ElMessage.success($t('common.disableSuccess'));
    tableSelectedKeys.value = [];
    getTagList();
  }
}

// 导出标签
async function handleExport() {
  if (!selectedDevice.value) {
    ElMessage.warning($t('page.tag.selectDeviceFirst'));
    return;
  }
  const { data, error } = await fetchExportTags(selectedDevice.value.id);
  if (!error && data) {
    // 下载JSON文件
    const blob = new Blob([JSON.stringify(data, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `tags-${selectedDevice.value.deviceId}-${new Date().toISOString().slice(0, 10)}.json`;
    a.click();
    URL.revokeObjectURL(url);
    ElMessage.success($t('page.tag.exportSuccess'));
  }
}

// 导入标签 (TODO: 实现文件上传)
function handleImport() {
  if (!selectedDevice.value) {
    ElMessage.warning($t('page.tag.selectDeviceFirst'));
    return;
  }
  ElMessage.info($t('page.tag.importNotImplemented'));
}

// 抽屉提交成功
function handleDrawerSubmitted() {
  getTagList();
}

// 获取数据类型显示文本
function getDataTypeLabel(dataType: string): string {
  const code = dataTypeBackendToFrontendMap[dataType];
  if (code) {
    return $t(dataTypeRecord[code]);
  }
  return dataType;
}

// 获取启用状态显示文本
function getEnabledLabel(enabled: boolean): string {
  return $t(enableStatusRecord[enabled ? '1' : '2']);
}

// 获取协议类型显示文本
function getProtocolTypeLabel(protocolType: string): string {
  const record: Record<string, string> = {
    MODBUS_TCP: 'Modbus TCP',
    MODBUS_RTU: 'Modbus RTU',
    OPC_UA: 'OPC UA',
    OPC_DA: 'OPC DA',
    S7: 'S7'
  };
  return record[protocolType] || protocolType;
}

// 格式化标签地址显示
function formatTagAddress(address: string): string {
  try {
    const parsed = JSON.parse(address);
    if (parsed.functionCode) {
      // Modbus
      return `FC${parsed.functionCode} Addr:${parsed.address} Slave:${parsed.slaveId}`;
    } else if (parsed.nodeId) {
      // OPC UA
      return parsed.nodeId;
    } else if (parsed.itemId) {
      // OPC DA
      return parsed.itemId;
    } else if (parsed.area) {
      // S7
      return `${parsed.area}${parsed.dbNumber ? parsed.dbNumber : ''}.${parsed.offset}`;
    }
    return address;
  } catch {
    return address;
  }
}

onMounted(() => {
  getDeviceList();
});

// 页面激活时刷新设备列表（从其他页面切换回来时）
onActivated(() => {
  getDeviceList();
});
</script>

<template>
  <div class="h-full flex">
    <!-- 左侧设备列表 -->
    <div class="w-280px flex flex-col flex-shrink-0 border-r border-gray-200 dark:border-gray-700">
      <div class="border-b border-gray-200 p-3 dark:border-gray-700">
        <div class="mb-2 text-base font-bold">{{ $t('page.tag.deviceList') }}</div>

        <!-- 搜索框 -->
        <ElInput v-model="deviceSearchKeyword" :placeholder="$t('page.tag.searchDevice')" clearable class="mb-2" />

        <!-- 协议类型过滤 -->
        <ElSelect
          v-model="deviceProtocolFilter"
          :placeholder="$t('page.device.protocolTypeLabel')"
          clearable
          class="mb-2 w-full"
        >
          <ElOption
            v-for="option in protocolTypeOptions"
            :key="option.value"
            :label="$t(option.label)"
            :value="option.value"
          />
        </ElSelect>

        <!-- 启用状态过滤 -->
        <ElCheckbox v-model="deviceEnabledFilter">
          {{ $t('page.tag.showEnabledOnly') }}
        </ElCheckbox>
      </div>
      <div class="flex-1 overflow-auto p-2">
        <div
          v-for="device in filteredDeviceList"
          :key="device.id"
          class="device-item mb-2 cursor-pointer rounded p-3 transition-colors"
          :class="{
            'bg-primary-100 dark:bg-primary-900': selectedDevice?.id === device.id,
            'hover:bg-gray-100 dark:hover:bg-gray-800': selectedDevice?.id !== device.id
          }"
          @click="handleSelectDevice(device)"
        >
          <div class="flex items-center justify-between">
            <div class="truncate font-medium" :title="device.deviceName">{{ device.deviceName }}</div>
            <ElTag :type="device.enabled ? 'success' : 'info'" size="small">
              {{ getEnabledLabel(device.enabled) }}
            </ElTag>
          </div>
          <div class="mt-1 truncate text-xs text-gray-500" :title="device.deviceId">
            {{ device.deviceId }}
          </div>
          <div class="mt-1 flex items-center justify-between text-xs text-gray-400">
            <span>{{ getProtocolTypeLabel(device.protocolType) }}</span>
            <span>{{ $t('page.tag.tagCount', { count: device.tagCount }) }}</span>
          </div>
        </div>
        <div v-if="filteredDeviceList.length === 0" class="py-8 text-center text-gray-400">
          {{ $t('page.tag.noDevice') }}
        </div>
      </div>
    </div>

    <!-- 右侧标签列表 -->
    <div class="min-w-0 flex flex-col flex-1">
      <!-- 标题和按钮 -->
      <div class="flex items-center justify-between border-b border-gray-200 p-4 dark:border-gray-700">
        <div class="text-lg font-bold">
          {{ $t('page.tag.tagList') }}
          <span v-if="selectedDevice" class="ml-2 text-primary">
            - {{ selectedDevice.deviceName }} ({{ getProtocolTypeLabel(selectedDevice.protocolType) }})
          </span>
        </div>
        <div class="flex gap-2">
          <ElButton v-if="hasAuth('5:select')" @click="handleImport">
            <icon-ic-round-upload class="mr-1" />
            {{ $t('common.import') }}
          </ElButton>
          <ElButton v-if="hasAuth('5:select')" @click="handleExport">
            <icon-ic-round-download class="mr-1" />
            {{ $t('common.export') }}
          </ElButton>
        </div>
      </div>

      <!-- 搜索区域 -->
      <TagListSearch
        v-model:model="searchParams"
        :disabled="!selectedDevice"
        @search="handleSearch"
        @reset="handleReset"
      />

      <!-- 操作按钮 -->
      <div class="flex gap-2 px-4 py-2">
        <ElButton v-if="hasAuth('5:add')" type="primary" :disabled="!selectedDevice" @click="handleAdd">
          <icon-ic-round-plus class="mr-1" />
          {{ $t('common.add') }}
        </ElButton>
        <ElButton
          v-if="hasAuth('5:delete')"
          type="danger"
          :disabled="tableSelectedKeys.length === 0"
          @click="handleBatchDelete"
        >
          {{ $t('common.batchDelete') }}
        </ElButton>
        <ElButton v-if="hasAuth('5:edit')" :disabled="tableSelectedKeys.length === 0" @click="handleBatchEnable">
          {{ $t('page.tag.batchEnable') }}
        </ElButton>
        <ElButton v-if="hasAuth('5:edit')" :disabled="tableSelectedKeys.length === 0" @click="handleBatchDisable">
          {{ $t('page.tag.batchDisable') }}
        </ElButton>
      </div>

      <!-- 表格 -->
      <div class="flex-1 overflow-auto px-4">
        <ElTable
          v-loading="loading"
          :data="tagList"
          border
          stripe
          row-key="id"
          @selection-change="handleSelectionChange"
        >
          <ElTableColumn type="selection" width="50" fixed="left" />
          <ElTableColumn prop="tagName" :label="$t('page.tag.tagName')" min-width="120" show-overflow-tooltip />
          <ElTableColumn prop="tagId" :label="$t('page.tag.tagId')" min-width="120" show-overflow-tooltip />
          <ElTableColumn prop="tagAddress" :label="$t('page.tag.tagAddress')" min-width="180">
            <template #default="{ row }">
              <span :title="row.tagAddress">{{ formatTagAddress(row.tagAddress) }}</span>
            </template>
          </ElTableColumn>
          <ElTableColumn prop="dataType" :label="$t('page.tag.dataTypeLabel')" width="100">
            <template #default="{ row }">
              {{ getDataTypeLabel(row.dataType) }}
            </template>
          </ElTableColumn>
          <ElTableColumn prop="unit" :label="$t('page.tag.unit')" width="80" />
          <ElTableColumn prop="enabled" :label="$t('common.status')" width="80">
            <template #default="{ row }">
              <ElSwitch v-if="hasAuth('5:edit')" :model-value="row.enabled" @change="() => handleToggleEnabled(row)" />
              <ElTag v-else :type="row.enabled ? 'success' : 'info'" size="small">
                {{ getEnabledLabel(row.enabled) }}
              </ElTag>
            </template>
          </ElTableColumn>
          <ElTableColumn :label="$t('common.operate')" width="200" fixed="right" align="center">
            <template #default="{ row }">
              <div class="flex-center gap-8px">
                <ElButton v-if="hasAuth('5:edit')" type="primary" plain size="small" @click="handleEdit(row)">
                  {{ $t('common.edit') }}
                </ElButton>
                <ElButton v-if="hasAuth('5:delete')" type="danger" plain size="small" @click="handleDelete(row.id)">
                  {{ $t('common.delete') }}
                </ElButton>
              </div>
            </template>
          </ElTableColumn>
        </ElTable>
      </div>

      <!-- 分页 -->
      <div class="flex justify-end p-4">
        <ElPagination
          v-model:current-page="searchParams.current"
          v-model:page-size="searchParams.size"
          :total="total"
          :page-sizes="[10, 20, 50, 100]"
          layout="total, sizes, prev, pager, next, jumper"
          @current-change="handlePageChange"
          @size-change="handleSizeChange"
        />
      </div>
    </div>

    <!-- 编辑抽屉 -->
    <TagListOperateDrawer
      v-model:visible="drawerVisible"
      :operate-type="operateType"
      :row-data="editingData"
      :device="selectedDevice"
      @submitted="handleDrawerSubmitted"
    />
  </div>
</template>

<style scoped>
.device-item {
  border: 1px solid transparent;
}

.device-item:hover {
  border-color: var(--el-color-primary-light-5);
}
</style>
