<script setup lang="tsx">
import { reactive } from 'vue';
import { ElButton, ElPopconfirm, ElTag } from 'element-plus';
import { enableStatusRecord } from '@/constants/business';
import { fetchDeleteRole, fetchGetRoleList } from '@/service/api';
import { defaultTransform, useTableOperate, useUIPaginatedTable } from '@/hooks/common/table';
import { $t } from '@/locales';
import TableHeaderOperation from '@/components/advanced/table-header-operation.vue';
import RoleOperateDrawer from './modules/role-operate-drawer.vue';
import RoleSearch from './modules/role-search.vue';

defineOptions({
  name: 'ManageRole'
});

// 搜索参数 - 角色列表搜索条件
const searchParams = reactive(getInitSearchParams());

/**
 * 获取初始搜索参数
 */
function getInitSearchParams(): Api.SystemManage.RoleSearchParams {
  return {
    current: 1,
    size: 10,
    status: undefined,
    roleName: undefined,
    roleCode: undefined
  };
}

// 表格配置和数据管理 - 使用分页表格Hook管理角色列表
const { columns, columnChecks, data, loading, getData, getDataByPage, mobilePagination } = useUIPaginatedTable({
  paginationProps: {
    currentPage: searchParams.current,
    pageSize: searchParams.size
  },
  api: () => fetchGetRoleList(searchParams),
  transform: response => defaultTransform(response),
  onPaginationParamsChange: params => {
    searchParams.current = params.currentPage;
    searchParams.size = params.pageSize;
  },
  columns: () => [
    { type: 'selection', width: 48 },
    { type: 'index', label: $t('common.index'), width: 64 },
    { prop: 'roleName', label: $t('page.manage.role.roleName'), minWidth: 120 },
    { prop: 'roleCode', label: $t('page.manage.role.roleCode'), minWidth: 120 },
    { prop: 'roleDesc', label: $t('page.manage.role.roleDesc'), minWidth: 120 },
    {
      prop: 'status',
      label: $t('page.manage.role.roleStatus'),
      width: 100,
      formatter: row => {
        if (row.status === undefined) {
          return '';
        }

        const tagMap: Record<Api.Common.EnableStatus, UI.ThemeColor> = {
          1: 'success',
          2: 'warning'
        };

        const label = $t(enableStatusRecord[row.status]);

        return <ElTag type={tagMap[row.status]}>{label}</ElTag>;
      }
    },
    {
      prop: 'createdAt',
      label: '创建时间',
      width: 180,
      formatter: row => (row.createdAt ? new Date(row.createdAt).toLocaleString('zh-CN') : '')
    },
    {
      prop: 'updatedAt',
      label: '更新时间',
      width: 180,
      formatter: row => (row.updatedAt ? new Date(row.updatedAt).toLocaleString('zh-CN') : '')
    },
    {
      prop: 'operate',
      label: $t('common.operate'),
      width: 130,
      formatter: row => (
        <div class="flex-center gap-8px">
          <ElButton type="primary" plain size="small" onClick={() => edit(row.id)}>
            {$t('common.edit')}
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

// 表格操作Hook - 管理抽屉、编辑、删除等操作
const { drawerVisible, operateType, editingData, handleAdd, handleEdit, checkedRowKeys, onDeleted } = useTableOperate(
  data,
  'id',
  getData
);

/**
 * 批量删除角色
 */
async function handleBatchDelete() {
  if (checkedRowKeys.value.length === 0) {
    window.$message?.warning('请选择要删除的角色');
    return;
  }

  // 循环调用单个删除API
  let successCount = 0;
  let failCount = 0;

  // 使用Promise.all并发删除，提高效率
  const deletePromises = checkedRowKeys.value.map(async (row: any) => {
    const { error } = await fetchDeleteRole(row.id);
    return { error };
  });

  const results = await Promise.all(deletePromises);

  results.forEach(result => {
    if (!result.error) {
      successCount += 1;
    } else {
      failCount += 1;
    }
  });

  // 显示删除结果
  if (failCount === 0) {
    window.$message?.success(`成功删除 ${successCount} 个角色`);
  } else {
    window.$message?.warning(`成功删除 ${successCount} 个角色，失败 ${failCount} 个`);
  }

  // 清空选中项并刷新列表
  checkedRowKeys.value = [];
  await getData();
}

/**
 * 删除单个角色
 */
async function handleDelete(id: string) {
  const { error } = await fetchDeleteRole(id);

  if (!error) {
    onDeleted();
  }
}

/**
 * 重置搜索参数
 */
function resetSearchParams() {
  Object.assign(searchParams, getInitSearchParams());
}

/**
 * 编辑角色
 */
function edit(id: string) {
  handleEdit(id);
}
</script>

<template>
  <div class="min-h-500px flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <RoleSearch v-model:model="searchParams" @reset="resetSearchParams" @search="getDataByPage" />
    <ElCard class="card-wrapper sm:flex-1-hidden" body-class="ht50">
      <template #header>
        <div class="flex items-center justify-between">
          <p>{{ $t('page.manage.role.title') }}</p>
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
        <div class="mt-20px flex justify-end">
          <ElPagination
            v-if="mobilePagination.total"
            layout="total,prev,pager,next,sizes"
            v-bind="mobilePagination"
            @current-change="mobilePagination['current-change']"
            @size-change="mobilePagination['size-change']"
          />
        </div>
      </div>
      <RoleOperateDrawer
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
