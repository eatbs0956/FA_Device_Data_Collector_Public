<script setup lang="tsx">
import { nextTick, reactive, ref } from 'vue';
import { ElButton, ElMessageBox, ElTag } from 'element-plus';
import type { TableInstance } from 'element-plus';
import { nodeStatusOptions, platformTypeOptions, registrationTypeOptions } from '@/constants/business';
import { fetchDeleteEdgeNode, fetchGetEdgeNodeDeviceCount, fetchGetEdgeNodeList } from '@/service/api';
import { defaultTransform, useTableOperate, useUIPaginatedTable } from '@/hooks/common/table';
import { $t } from '@/locales';
import TableHeaderOperation from '@/components/advanced/table-header-operation.vue';
import NodeOperateDrawer from './node-operate-drawer.vue';
import NodeSearch from './node-search.vue';

defineOptions({ name: 'CollectionNode' });

const searchParams = reactive(getInitSearchParams());

function getInitSearchParams(): Api.EdgeNode.EdgeNodeSearchParams {
  return {
    current: 1,
    size: 30,
    nodeName: undefined,
    nodeId: undefined,
    status: undefined,
    platform: undefined
  };
}

// 节点状态映射：前端字符串 -> 后端字符串
const statusMap: Record<string, string> = {
  Online: 'Online',
  Offline: 'Offline',
  Error: 'Error'
};

// 转换搜索参数为后端期望的格式
function transformSearchParams() {
  return {
    ...searchParams,
    status: searchParams.status ? statusMap[searchParams.status] : undefined
  };
}

const { columns, columnChecks, data, getData, getDataByPage, loading, mobilePagination } = useUIPaginatedTable({
  paginationProps: {
    currentPage: searchParams.current,
    pageSize: searchParams.size
  },
  api: () => fetchGetEdgeNodeList(transformSearchParams()),
  transform: response => defaultTransform(response),
  onPaginationParamsChange: params => {
    searchParams.current = params.currentPage;
    searchParams.size = params.pageSize;
  },
  columns: () => [
    { type: 'selection', width: 48 },
    { type: 'index', label: $t('common.index'), width: 64 },
    { prop: 'nodeName', label: $t('page.edgeNode.nodeName'), minWidth: 150 },
    { prop: 'nodeId', label: $t('page.edgeNode.nodeId'), minWidth: 150 },
    {
      prop: 'platform',
      label: $t('page.edgeNode.platform'),
      width: 150,
      formatter: row => {
        if (!row.platform) {
          return <ElTag type="info">-</ElTag>;
        }

        const tagMap: Record<string, UI.ThemeColor> = {
          'NET8.0': 'primary',
          NET45: 'warning'
        };

        const platformOption = platformTypeOptions.find(opt => opt.value === row.platform);
        const label = platformOption ? $t(platformOption.label) : row.platform;

        return <ElTag type={tagMap[row.platform] || 'info'}>{label}</ElTag>;
      }
    },
    { prop: 'version', label: $t('page.edgeNode.version'), width: 100 },
    {
      prop: 'status',
      label: $t('page.edgeNode.status'),
      width: 100,
      align: 'center',
      formatter: row => {
        if (!row.status) {
          return <ElTag type="info">-</ElTag>;
        }

        const tagMap: Record<string, UI.ThemeColor> = {
          Online: 'success',
          Offline: 'warning',
          Error: 'danger'
        };

        const statusOption = nodeStatusOptions.find(opt => opt.value === row.status);
        const label = statusOption ? $t(statusOption.label) : row.status;

        return <ElTag type={tagMap[row.status] || 'info'}>{label}</ElTag>;
      }
    },
    {
      prop: 'registrationType',
      label: $t('page.edgeNode.registrationType'),
      width: 100,
      align: 'center',
      formatter: row => {
        const type = row.registrationType || 'auto';
        const tagMap: Record<string, UI.ThemeColor> = {
          auto: 'primary',
          manual: 'success'
        };

        const regOption = registrationTypeOptions.find(opt => opt.value === type);
        const label = regOption ? $t(regOption.label) : type;

        return <ElTag type={tagMap[type] || 'info'}>{label}</ElTag>;
      }
    },
    { prop: 'ipAddress', label: $t('page.edgeNode.ipAddress'), width: 140 },
    { prop: 'location', label: $t('page.edgeNode.location'), minWidth: 120 },
    {
      prop: 'deviceCount',
      label: $t('page.edgeNode.deviceCount'),
      width: 100,
      align: 'center',
      formatter: row => String(row.deviceCount ?? 0)
    },
    {
      prop: 'lastHeartbeat',
      label: $t('page.edgeNode.lastHeartbeat'),
      width: 180,
      formatter: row => (row.lastHeartbeat ? new Date(row.lastHeartbeat).toLocaleString('zh-CN') : '-')
    },
    {
      prop: 'operate',
      label: $t('common.operate'),
      align: 'center',
      width: 200,
      formatter: row => (
        <div class="flex-center gap-8px">
          <ElButton type="primary" plain size="small" onClick={() => editRow(row)}>
            {$t('common.edit')}
          </ElButton>
          <ElButton type="danger" plain size="small" onClick={() => confirmDelete(row)}>
            {$t('common.delete')}
          </ElButton>
        </div>
      )
    }
  ]
});

const { drawerVisible, operateType, editingData, handleAdd, onDeleted } = useTableOperate<Api.EdgeNode.EdgeNode>(
  data,
  'id',
  getData
);

// 直接使用行数据编辑，避免通过 id 查找可能导致的问题
async function editRow(row: Api.EdgeNode.EdgeNode) {
  // 先设置数据，再打开 drawer
  operateType.value = 'edit';
  editingData.value = { ...row }; // 深拷贝行数据
  // 等待数据更新后再打开 drawer
  await nextTick();
  drawerVisible.value = true;
}

// 表格实例和选中行
const tableRef = ref<TableInstance>();
const selectedRows = ref<Api.EdgeNode.EdgeNode[]>([]);

function handleSelectionChange(rows: Api.EdgeNode.EdgeNode[]) {
  selectedRows.value = rows;
}

// 批量删除
async function handleBatchDelete() {
  if (selectedRows.value.length === 0) {
    window.$message?.warning($t('common.pleaseSelectData'));
    return;
  }

  try {
    await ElMessageBox.confirm(
      $t('page.edgeNode.confirmBatchDelete', { count: selectedRows.value.length }),
      $t('common.confirmDelete'),
      {
        type: 'warning',
        confirmButtonText: $t('common.confirm'),
        cancelButtonText: $t('common.cancel')
      }
    );

    // 执行批量删除
    const deletePromises = selectedRows.value.map(row => fetchDeleteEdgeNode(row.id));
    const results = await Promise.allSettled(deletePromises);

    const successCount = results.filter(r => r.status === 'fulfilled').length;
    const failCount = results.length - successCount;

    if (failCount === 0) {
      window.$message?.success($t('page.edgeNode.batchDeleteSuccess', { count: successCount }));
    } else {
      window.$message?.warning(
        $t('page.edgeNode.batchDeletePartialSuccess', { success: successCount, fail: failCount })
      );
    }

    // 清空选中并刷新数据
    tableRef.value?.clearSelection();
    selectedRows.value = [];
    getData();
  } catch {
    // 用户取消删除
  }
}

// 删除前确认弹窗
const deletingNode = ref<Api.EdgeNode.EdgeNode | null>(null);

async function confirmDelete(row: Api.EdgeNode.EdgeNode) {
  deletingNode.value = row;

  try {
    // 获取该节点关联的设备数量
    const { error, data: countData } = await fetchGetEdgeNodeDeviceCount(row.id);

    if (error) {
      return;
    }

    const deviceCount = countData?.count ?? 0;
    let message = $t('page.edgeNode.confirmDeleteNode', { name: row.nodeName });
    if (deviceCount > 0) {
      message += `\n\n${$t('page.edgeNode.deleteNodeWithDevicesWarning', { count: deviceCount })}`;
    }

    ElMessageBox.confirm(message, $t('common.confirmDelete'), {
      type: 'warning',
      confirmButtonText: $t('common.confirm'),
      cancelButtonText: $t('common.cancel')
    }).then(async () => {
      await handleDelete(row.id);
    });
  } catch {
    // 用户取消删除
  }
}

/**
 * 删除单个节点
 */
async function handleDelete(id: string) {
  const { error } = await fetchDeleteEdgeNode(id);

  if (!error) {
    window.$message?.success($t('page.edgeNode.deleteSuccess'));
    onDeleted();
  }
}

function resetSearchParams() {
  Object.assign(searchParams, getInitSearchParams());
  getDataByPage();
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <NodeSearch v-model:model="searchParams" @reset="resetSearchParams" @search="getDataByPage" />
    <ElCard class="card-wrapper sm:flex-1-hidden" body-class="ht50">
      <template #header>
        <div class="flex items-center justify-between">
          <p>{{ $t('page.edgeNode.title') }}</p>
          <TableHeaderOperation
            v-model:columns="columnChecks"
            :loading="loading"
            :disabled-delete="selectedRows.length === 0"
            @add="handleAdd"
            @delete="handleBatchDelete"
            @refresh="getData"
          />
        </div>
      </template>
      <div class="h-[calc(100%-50px)]">
        <ElTable
          ref="tableRef"
          v-loading="loading"
          height="100%"
          border
          class="sm:h-full"
          :data="data"
          row-key="id"
          @selection-change="handleSelectionChange"
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
      <NodeOperateDrawer
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
