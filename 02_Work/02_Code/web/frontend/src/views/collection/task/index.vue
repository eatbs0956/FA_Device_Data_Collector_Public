<script setup lang="tsx">
import { nextTick, reactive, ref } from 'vue';
import { ElButton, ElMessageBox, ElTag } from 'element-plus';
import type { TableInstance } from 'element-plus';
import { taskStatusOptions, taskTypeOptions } from '@/constants/business';
import { fetchDeleteCollectionTask, fetchGetCollectionTaskList, fetchUpdateCollectionTask } from '@/service/api';
import { defaultTransform, useTableOperate, useUIPaginatedTable } from '@/hooks/common/table';
import { $t } from '@/locales';
import TableHeaderOperation from '@/components/advanced/table-header-operation.vue';
import TaskOperateDrawer from './task-operate-drawer.vue';
import TaskSearch from './task-search.vue';

defineOptions({ name: 'CollectionTask' });

const searchParams = reactive(getInitSearchParams());

function getInitSearchParams(): Api.CollectionTask.CollectionTaskSearchParams {
  return {
    current: 1,
    size: 30,
    name: undefined,
    taskType: undefined,
    status: undefined
  };
}

const { columns, columnChecks, data, getData, getDataByPage, loading, mobilePagination } = useUIPaginatedTable({
  paginationProps: {
    currentPage: searchParams.current,
    pageSize: searchParams.size
  },
  api: () => fetchGetCollectionTaskList(searchParams),
  transform: response => defaultTransform(response),
  onPaginationParamsChange: params => {
    searchParams.current = params.currentPage;
    searchParams.size = params.pageSize;
  },
  columns: () => [
    { type: 'selection', width: 48 },
    { type: 'index', label: $t('common.index'), width: 64 },
    { prop: 'name', label: $t('page.collectionTask.name'), minWidth: 150 },
    { prop: 'code', label: $t('page.collectionTask.code'), minWidth: 120 },
    {
      prop: 'taskType',
      label: $t('page.collectionTask.taskType'),
      width: 120,
      align: 'center',
      formatter: row => {
        if (!row.taskType) {
          return <ElTag type="info">-</ElTag>;
        }

        const tagMap: Record<string, UI.ThemeColor> = {
          Periodic: 'primary',
          Scheduled: 'success',
          EventDriven: 'warning',
          Hybrid: 'danger'
        };

        const typeOption = taskTypeOptions.find(opt => opt.value === row.taskType);
        const label = typeOption ? $t(typeOption.label) : row.taskType;

        return <ElTag type={tagMap[row.taskType] || 'info'}>{label}</ElTag>;
      }
    },
    {
      prop: 'status',
      label: $t('page.collectionTask.status'),
      width: 100,
      align: 'center',
      formatter: row => {
        if (!row.status) {
          return <ElTag type="info">-</ElTag>;
        }

        const tagMap: Record<string, UI.ThemeColor> = {
          Draft: 'info',
          Active: 'success',
          Paused: 'warning',
          Stopped: 'danger'
        };

        const statusOption = taskStatusOptions.find(opt => opt.value === row.status);
        const label = statusOption ? $t(statusOption.label) : row.status;

        return <ElTag type={tagMap[row.status] || 'info'}>{label}</ElTag>;
      }
    },
    {
      prop: 'priority',
      label: $t('page.collectionTask.priority'),
      width: 80,
      align: 'center',
      formatter: row => String(row.priority ?? 5)
    },
    {
      prop: 'defaultInterval',
      label: $t('page.collectionTask.defaultInterval'),
      width: 120,
      align: 'center',
      formatter: row => {
        if (!row.defaultInterval) return '-';
        return `${row.defaultInterval}ms`;
      }
    },
    {
      prop: 'deviceCount',
      label: $t('page.collectionTask.deviceCount'),
      width: 100,
      align: 'center',
      formatter: row => String(row.deviceCount ?? 0)
    },
    {
      prop: 'isEnabled',
      label: $t('page.collectionTask.isEnabled'),
      width: 80,
      align: 'center',
      formatter: row => (
        <ElTag type={row.isEnabled ? 'success' : 'info'}>
          {row.isEnabled ? $t('common.enable') : $t('common.disable')}
        </ElTag>
      )
    },
    {
      prop: 'operate',
      label: $t('common.operate'),
      align: 'center',
      width: 240,
      fixed: 'right',
      formatter: row => (
        <div class="flex-center gap-8px">
          {row.isEnabled ? (
            <ElButton type="warning" plain size="small" onClick={() => handleToggleEnable(row, false)}>
              {$t('common.disable')}
            </ElButton>
          ) : (
            <ElButton type="success" plain size="small" onClick={() => handleToggleEnable(row, true)}>
              {$t('common.enable')}
            </ElButton>
          )}
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

const { drawerVisible, operateType, editingData, handleAdd, onDeleted } =
  useTableOperate<Api.CollectionTask.CollectionTask>(data, 'id', getData);

// 直接使用行数据编辑
async function editRow(row: Api.CollectionTask.CollectionTask) {
  operateType.value = 'edit';
  editingData.value = { ...row };
  await nextTick();
  drawerVisible.value = true;
}

// 表格实例和选中行
const tableRef = ref<TableInstance>();
const selectedRows = ref<Api.CollectionTask.CollectionTask[]>([]);

function handleSelectionChange(rows: Api.CollectionTask.CollectionTask[]) {
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
      $t('page.collectionTask.confirmBatchDelete', {
        count: selectedRows.value.length
      }),
      $t('common.confirmDelete'),
      {
        type: 'warning',
        confirmButtonText: $t('common.confirm'),
        cancelButtonText: $t('common.cancel')
      }
    );

    // 执行批量删除
    const deletePromises = selectedRows.value.map(row => fetchDeleteCollectionTask(row.id));
    const results = await Promise.allSettled(deletePromises);

    const successCount = results.filter(r => r.status === 'fulfilled').length;
    const failCount = results.length - successCount;

    if (failCount === 0) {
      window.$message?.success($t('page.collectionTask.batchDeleteSuccess', { count: successCount }));
    } else {
      window.$message?.warning(
        $t('page.collectionTask.batchDeletePartialSuccess', {
          success: successCount,
          fail: failCount
        })
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

// 切换任务启用/禁用状态
async function handleToggleEnable(row: Api.CollectionTask.CollectionTask, enable: boolean) {
  const action = enable ? $t('common.enable') : $t('common.disable');
  const confirmMessage = enable
    ? $t('page.collectionTask.confirmEnableTask', { name: row.name })
    : $t('page.collectionTask.confirmDisableTask', { name: row.name });

  try {
    await ElMessageBox.confirm(confirmMessage, $t('common.confirm'), {
      type: 'warning',
      confirmButtonText: $t('common.confirm'),
      cancelButtonText: $t('common.cancel')
    });

    const { error } = await fetchUpdateCollectionTask(row.id, {
      name: row.name,
      taskType: row.taskType,
      isEnabled: enable,
      description: row.description ?? undefined,
      defaultInterval: row.defaultInterval ?? undefined,
      cronExpression: row.cronExpression ?? undefined,
      priority: row.priority,
      effectiveFrom: row.effectiveFrom ?? undefined,
      effectiveTo: row.effectiveTo ?? undefined,
      deviceIds: row.deviceIds
    });

    if (!error) {
      window.$message?.success(`${action}${$t('common.updateSuccess')}`);
      getData();
    }
  } catch {
    // 用户取消
  }
}

// 删除前确认弹窗
async function confirmDelete(row: Api.CollectionTask.CollectionTask) {
  try {
    await ElMessageBox.confirm(
      $t('page.collectionTask.confirmDeleteTask', { name: row.name }),
      $t('common.confirmDelete'),
      {
        type: 'warning',
        confirmButtonText: $t('common.confirm'),
        cancelButtonText: $t('common.cancel')
      }
    );

    await handleDelete(row.id);
  } catch {
    // 用户取消删除
  }
}

/**
 * 删除单个任务
 */
async function handleDelete(id: string) {
  const { error } = await fetchDeleteCollectionTask(id);

  if (!error) {
    window.$message?.success($t('page.collectionTask.deleteSuccess'));
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
    <TaskSearch v-model:model="searchParams" @reset="resetSearchParams" @search="getDataByPage" />
    <ElCard class="card-wrapper sm:flex-1-hidden" body-class="ht50">
      <template #header>
        <div class="flex items-center justify-between">
          <p>{{ $t('page.collectionTask.title') }}</p>
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
      <TaskOperateDrawer
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
