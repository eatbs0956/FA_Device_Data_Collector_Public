<script setup lang="tsx">
import { reactive } from 'vue';
import { ElButton, ElPopconfirm, ElTag } from 'element-plus';
import { connectionStatusRecord, enableStatusRecord, protocolTypeRecord } from '@/constants/business';
import {
  fetchBatchDeleteDevices,
  fetchDeleteDevice,
  fetchGetDeviceList,
  fetchToggleDeviceEnabled
} from '@/service/api';
import { defaultTransform, useTableOperate, useUIPaginatedTable } from '@/hooks/common/table';
import { $t } from '@/locales';
import TableHeaderOperation from '@/components/advanced/table-header-operation.vue';
import DeviceOperateDrawer from './device-list-operate-drawer.vue';
import DeviceSearch from './device-list-search.vue';

defineOptions({ name: 'DeviceList' });

const searchParams = reactive(getInitSearchParams());

function getInitSearchParams(): Api.Device.DeviceSearchParams {
  return {
    current: 1,
    size: 30,
    deviceName: undefined,
    deviceId: undefined,
    protocolType: undefined,
    connectionStatus: undefined,
    edgeNodeId: undefined,
    enabled: undefined
  };
}

// 协议类型映射：前端数字 -> 后端字符串
const protocolTypeMap: Record<string, string> = {
  '1': 'MODBUS_TCP',
  '2': 'MODBUS_RTU',
  '3': 'OPC_UA',
  '4': 'OPC_DA',
  '5': 'S7'
};

// 连接状态映射：前端数字 -> 后端字符串（注意：后端枚举是首字母大写）
const connectionStatusMap: Record<string, string> = {
  '1': 'Connected',
  '2': 'Disconnected',
  '3': 'Error',
  '99': 'Unknown'
};

// 转换搜索参数为后端期望的格式
function transformSearchParams() {
  return {
    ...searchParams,
    protocolType: searchParams.protocolType ? protocolTypeMap[searchParams.protocolType] : undefined,
    connectionStatus: searchParams.connectionStatus ? connectionStatusMap[searchParams.connectionStatus] : undefined
  };
}

const { columns, columnChecks, data, getData, getDataByPage, loading, mobilePagination } = useUIPaginatedTable({
  paginationProps: {
    currentPage: searchParams.current,
    pageSize: searchParams.size
  },
  api: () => fetchGetDeviceList(transformSearchParams()),
  transform: response => defaultTransform(response),
  onPaginationParamsChange: params => {
    searchParams.current = params.currentPage;
    searchParams.size = params.pageSize;
  },
  columns: () => [
    { type: 'selection', width: 48 },
    { type: 'index', label: $t('common.index'), width: 64 },
    { prop: 'deviceName', label: $t('page.device.deviceName'), minWidth: 150 },
    { prop: 'deviceId', label: $t('page.device.deviceId'), minWidth: 150 },
    {
      prop: 'protocolType',
      label: $t('page.device.protocol'),
      width: 120,
      formatter: row => {
        if (!row.protocolType) {
          return '';
        }

        // 后端字符串 -> 前端数字映射
        const protocolTypeToCode: Record<string, Api.Device.ProtocolType> = {
          MODBUS_TCP: '1',
          MODBUS_RTU: '2',
          OPC_UA: '3',
          OPC_DA: '4',
          S7: '5'
        };

        const code = protocolTypeToCode[row.protocolType];
        if (!code) {
          // 如果无法映射，直接返回原值
          return <ElTag type="info">{row.protocolType}</ElTag>;
        }

        // 直接获取翻译key
        const translationKey = protocolTypeRecord[code];

        // 如果没有找到翻译key，返回原始协议类型
        if (!translationKey) {
          return <ElTag type="info">{row.protocolType}</ElTag>;
        }

        const tagMap: Record<Api.Device.ProtocolType, UI.ThemeColor> = {
          '1': 'primary',
          '2': 'primary',
          '3': 'success',
          '4': 'success',
          '5': 'warning'
        };

        const label = $t(translationKey);

        return <ElTag type={tagMap[code]}>{label}</ElTag>;
      }
    },
    {
      prop: 'connectionStatus',
      label: $t('page.device.connection'),
      width: 100,
      align: 'center',
      formatter: row => {
        if (!row.connectionStatus) {
          return '';
        }

        // 后端字符串 -> 前端数字映射（注意：后端枚举是首字母大写）
        const connectionStatusToCode: Record<string, Api.Device.ConnectionStatus> = {
          Connected: '1',
          Disconnected: '2',
          Connecting: '2', // 正在连接也算作断开状态
          Reconnecting: '2', // 重新连接也算作断开状态
          Error: '3',
          Unknown: '99'
        };

        // 尝试映射，如果是数字字符串则直接使用
        const code = connectionStatusToCode[row.connectionStatus] || row.connectionStatus;

        const tagMap: Record<Api.Device.ConnectionStatus, UI.ThemeColor> = {
          '1': 'success',
          '2': 'warning',
          '3': 'danger',
          '99': 'info'
        };

        const translationKey = connectionStatusRecord[code];
        if (!translationKey) {
          return <ElTag type="info">{row.connectionStatus}</ElTag>;
        }

        const label = $t(translationKey);

        return <ElTag type={tagMap[code]}>{label}</ElTag>;
      }
    },
    {
      prop: 'edgeNode',
      label: $t('page.device.edgeNode'),
      minWidth: 120,
      formatter: row => row.edgeNode?.nodeName || '-'
    },
    { prop: 'location', label: $t('page.device.location'), minWidth: 120 },
    {
      prop: 'tagCount',
      label: $t('page.device.tagCount'),
      width: 100,
      align: 'center'
    },
    {
      prop: 'enabled',
      label: $t('page.device.enabled'),
      width: 100,
      align: 'center',
      formatter: row => {
        const status = row.enabled ? '1' : '2';
        const tagMap: Record<Api.Common.EnableStatus, UI.ThemeColor> = {
          '1': 'success',
          '2': 'warning'
        };

        const label = $t(enableStatusRecord[status as Api.Common.EnableStatus]);

        return <ElTag type={tagMap[status as Api.Common.EnableStatus]}>{label}</ElTag>;
      }
    },
    {
      prop: 'lastConnectedAt',
      label: $t('page.device.lastConnectedAt'),
      width: 180,
      formatter: row => (row.lastConnectedAt ? new Date(row.lastConnectedAt).toLocaleString('zh-CN') : '-')
    },
    {
      prop: 'operate',
      label: $t('common.operate'),
      align: 'center',
      width: 280,
      formatter: row => (
        <div class="flex-center gap-8px">
          <ElButton type="primary" plain size="small" onClick={() => edit(row.id)}>
            {$t('common.edit')}
          </ElButton>
          <ElButton
            type={row.enabled ? 'warning' : 'success'}
            plain
            size="small"
            onClick={() => handleToggleEnabled(row.id, row.enabled)}
          >
            {row.enabled ? $t('page.device.disable') : $t('page.device.enable')}
          </ElButton>
          <ElPopconfirm title={$t('common.confirmDelete')} onConfirm={() => handleDelete(row.id)}>
            {{
              reference: () => (
                <ElButton type="danger" plain size="small">
                  {$t('common.delete')}
                </ElButton>
              )
            }}
          </ElPopconfirm>
        </div>
      )
    }
  ]
});

const { drawerVisible, operateType, editingData, handleAdd, handleEdit, checkedRowKeys, onDeleted } = useTableOperate(
  data,
  'id',
  getData
);

/**
 * 批量删除设备
 */
async function handleBatchDelete() {
  if (checkedRowKeys.value.length === 0) {
    window.$message?.warning($t('page.device.pleaseSelectDevicesToDelete'));
    return;
  }

  const deviceIds = checkedRowKeys.value.map((row: any) => row.id);

  const { error } = await fetchBatchDeleteDevices(deviceIds);

  if (!error) {
    window.$message?.success($t('page.device.batchDeleteSuccess', { count: deviceIds.length }));
    checkedRowKeys.value = [];
    await getData();
  }
}

/**
 * 删除单个设备
 */
async function handleDelete(id: string) {
  const { error } = await fetchDeleteDevice(id);

  if (!error) {
    onDeleted();
  }
}

/**
 * 切换设备启用状态
 */
async function handleToggleEnabled(id: string, currentEnabled: boolean) {
  const { error } = await fetchToggleDeviceEnabled(id, !currentEnabled);

  if (!error) {
    window.$message?.success(currentEnabled ? $t('page.device.deviceDisabled') : $t('page.device.deviceEnabled'));

    // 直接更新本地数据，不重新加载整个列表
    const device = data.value.find(item => item.id === id);
    if (device) {
      device.enabled = !currentEnabled;
    }
  }
}

function resetSearchParams() {
  Object.assign(searchParams, getInitSearchParams());
  getDataByPage();
}

function edit(id: string) {
  handleEdit(id);
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <DeviceSearch v-model:model="searchParams" @reset="resetSearchParams" @search="getDataByPage" />
    <ElCard class="card-wrapper sm:flex-1-hidden" body-class="ht50">
      <template #header>
        <div class="flex items-center justify-between">
          <p>{{ $t('page.device.title') }}</p>
          <TableHeaderOperation
            v-model:columns="columnChecks"
            :disabled-delete="checkedRowKeys.length === 0"
            :loading="loading"
            @add="handleAdd"
            @delete="handleBatchDelete"
            @refresh="getData"
          />
        </div>
      </template>
      <div class="h-[calc(100%-50px)]">
        <ElTable
          v-loading="loading"
          height="100%"
          border
          class="sm:h-full"
          :data="data"
          row-key="id"
          @selection-change="checkedRowKeys = $event"
        >
          <ElTableColumn v-for="col in columns" :key="col.prop" v-bind="col" />
        </ElTable>
      </div>
      <div class="mt-20px flex justify-end">
        <ElPagination
          v-if="mobilePagination.total"
          layout="total,prev,pager,next,sizes"
          v-bind="mobilePagination"
          @current-change="mobilePagination['current-change']"
          @size-change="mobilePagination['size-change']"
        />
      </div>
      <DeviceOperateDrawer
        v-model:visible="drawerVisible"
        :operate-type="operateType"
        :row-data="editingData"
        @submitted="getDataByPage"
      />
    </ElCard>
  </div>
</template>

<style lang="scss" scoped>
:deep(.el-card) {
  .ht50 {
    height: calc(100% - 50px);
  }
}
</style>
