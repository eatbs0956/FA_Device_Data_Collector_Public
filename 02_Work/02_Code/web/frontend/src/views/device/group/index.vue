<script setup lang="tsx">
import { reactive, ref } from 'vue';
import { ElButton, ElPopconfirm, ElTag, ElTree } from 'element-plus';
import { fetchDeleteDeviceGroup, fetchGetDeviceGroupList, fetchGetDeviceGroupTree } from '@/service/api';
import { defaultTransform, useUIPaginatedTable } from '@/hooks/common/table';
import { $t } from '@/locales';
import TableHeaderOperation from '@/components/advanced/table-header-operation.vue';
import DeviceGroupOperateDrawer from './device-group-operate-drawer.vue';

defineOptions({ name: 'DeviceGroup' });

const drawerVisible = ref(false);
const operateType = ref<'add' | 'edit'>('add');
const editingRow = ref<Api.Device.DeviceGroup | null>(null);

const searchParams = reactive(getInitSearchParams());

function getInitSearchParams(): Api.Device.DeviceGroupSearchParams {
  return {
    current: 1,
    size: 20,
    parentId: undefined,
    name: undefined,
    includeAll: false
  };
}

const selectedGroupId = ref<string | undefined>(undefined);
const selectedGroupName = ref<string>('');
const treeData = ref<Api.Device.DeviceGroupTreeNode[]>([]);
const treeLoading = ref(false);
const selectedRows = ref<Api.Device.DeviceGroup[]>([]);

async function loadTreeData() {
  treeLoading.value = true;
  try {
    const { data, error } = await fetchGetDeviceGroupTree();
    if (!error && data) {
      treeData.value = data;
    }
  } finally {
    treeLoading.value = false;
  }
}

loadTreeData();

const { columns, columnChecks, data, getData, getDataByPage, loading, mobilePagination } = useUIPaginatedTable({
  paginationProps: { currentPage: searchParams.current, pageSize: searchParams.size },
  api: () => fetchGetDeviceGroupList(searchParams),
  transform: response => defaultTransform(response),
  onPaginationParamsChange: params => {
    searchParams.current = params.currentPage;
    searchParams.size = params.pageSize;
  },
  columns: () => [
    { type: 'selection', width: 55 },
    { type: 'index', label: $t('common.index'), width: 64 },
    { prop: 'name', label: $t('page.deviceGroup.name'), minWidth: 130 },
    { prop: 'description', label: $t('page.deviceGroup.description'), minWidth: 150 },
    {
      prop: 'level',
      label: $t('page.deviceGroup.level'),
      width: 80,
      align: 'center',
      formatter: row => <ElTag type="info">{row.level}</ElTag>
    },
    { prop: 'sortOrder', label: $t('page.deviceGroup.sortOrder'), width: 100, align: 'center' },
    {
      prop: 'deviceCount',
      label: $t('page.deviceGroup.deviceCount'),
      width: 80,
      align: 'center',
      formatter: row => <ElTag type="primary">{row.deviceCount || 0}</ElTag>
    },
    {
      prop: 'childCount',
      label: $t('page.deviceGroup.childCount'),
      width: 100,
      align: 'center',
      formatter: row => <ElTag type="success">{row.childCount || 0}</ElTag>
    },
    {
      prop: 'createdAt',
      label: $t('common.createdAt'),
      width: 180,
      formatter: row => (row.createdAt ? new Date(row.createdAt).toLocaleString('zh-CN') : '-')
    },
    {
      prop: 'operate',
      label: $t('common.operate'),
      align: 'center',
      width: 250,
      formatter: row => (
        <div class="flex-center gap-8px">
          <ElButton type="success" plain size="small" onClick={() => handleAddChild(row.id, row.level)}>
            {$t('page.deviceGroup.addChild')}
          </ElButton>
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

function handleTreeNodeClick(node: Api.Device.DeviceGroupTreeNode) {
  selectedGroupId.value = node.id;
  selectedGroupName.value = node.name;
  searchParams.parentId = node.id;
  searchParams.current = 1;
  getDataByPage();
}

function resetTreeSelection() {
  selectedGroupId.value = undefined;
  selectedGroupName.value = '';
  searchParams.parentId = undefined;
  searchParams.current = 1;
  getDataByPage();
}

function edit(id: string) {
  const item = data.value.find(d => d.id === id);
  if (item) {
    operateType.value = 'edit';
    editingRow.value = item;
    drawerVisible.value = true;
  }
}

async function handleDelete(id: string) {
  const { error } = await fetchDeleteDeviceGroup(id);
  if (!error) {
    window.$message?.success($t('common.deleteSuccess'));
    await getData();
    await loadTreeData();
  }
}

async function handleBatchDelete() {
  if (selectedRows.value.length === 0) {
    return;
  }

  const promises = selectedRows.value.map(row => fetchDeleteDeviceGroup(row.id));
  const results = await Promise.all(promises);
  const successCount = results.filter(r => !r.error).length;

  if (successCount > 0) {
    window.$message?.success($t('common.deleteSuccess'));
    selectedRows.value = [];
    await getData();
    await loadTreeData();
  }
}

function handleSelectionChange(rows: Api.Device.DeviceGroup[]) {
  selectedRows.value = rows;
}

function openAddDrawer() {
  operateType.value = 'add';
  editingRow.value = {
    id: '',
    name: '',
    parentId: selectedGroupId.value,
    sortOrder: 0,
    description: '',
    level: 0,
    createdAt: '',
    updatedAt: '',
    deviceCount: 0,
    childCount: 0
  };
  drawerVisible.value = true;
}

function handleAddChild(parentId: string, parentLevel: number) {
  if (parentLevel >= 4) {
    window.$message?.warning($t('page.deviceGroup.maxLevelReached'));
    return;
  }
  operateType.value = 'add';
  editingRow.value = {
    id: '',
    name: '',
    parentId,
    sortOrder: 0,
    description: '',
    level: 0,
    createdAt: '',
    updatedAt: '',
    deviceCount: 0,
    childCount: 0
  };
  drawerVisible.value = true;
}

async function handleDrawerSubmitted() {
  drawerVisible.value = false;
  await getData();
  await loadTreeData();
}

function resetSearchParams() {
  Object.assign(searchParams, getInitSearchParams());
  resetTreeSelection();
}

function handleSearch() {
  getDataByPage();
}
</script>

<template>
  <div class="min-h-500px flex gap-16px overflow-hidden lt-sm:overflow-auto">
    <ElCard class="w-280px flex-shrink-0" body-style="padding: 12px;">
      <template #header>
        <div class="flex items-center justify-between">
          <span>{{ $t('page.deviceGroup.tree') }}</span>
          <ElButton type="primary" text size="small" :loading="treeLoading" @click="loadTreeData">
            <template #icon><icon-ic-round-refresh class="text-icon" /></template>
          </ElButton>
        </div>
      </template>
      <div class="mb-8px">
        <ElButton v-if="selectedGroupId" type="info" plain size="small" class="w-full" @click="resetTreeSelection">
          {{ $t('page.deviceGroup.showAll') }}
        </ElButton>
      </div>
      <ElTree
        v-loading="treeLoading"
        :data="treeData"
        :props="{ label: 'name', children: 'children' }"
        node-key="id"
        highlight-current
        default-expand-all
        :expand-on-click-node="false"
        @node-click="handleTreeNodeClick"
      />
    </ElCard>

    <ElCard class="flex-1" body-class="ht50">
      <template #header>
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-8px">
            <span>{{ $t('page.deviceGroup.title') }}</span>
            <ElTag v-if="selectedGroupName" type="primary" size="small">{{ selectedGroupName }}</ElTag>
          </div>
          <TableHeaderOperation
            v-model:columns="columnChecks"
            :disabled-delete="selectedRows.length === 0"
            :loading="loading"
            @add="openAddDrawer"
            @delete="handleBatchDelete"
            @refresh="getData"
          />
        </div>
      </template>
      <div class="mb-16px flex gap-16px">
        <ElInput
          v-model="searchParams.name"
          :placeholder="$t('page.deviceGroup.searchPlaceholder')"
          clearable
          class="w-200px"
          @keyup.enter="handleSearch"
        />
        <ElButton type="primary" plain @click="handleSearch">
          <template #icon>
            <icon-ic-round-search class="text-icon" />
          </template>
          {{ $t('common.search') }}
        </ElButton>
        <ElButton @click="resetSearchParams">
          <template #icon>
            <icon-ic-round-refresh class="text-icon" />
          </template>
          {{ $t('common.reset') }}
        </ElButton>
      </div>
      <div class="h-[calc(100%-130px)]">
        <ElTable
          v-loading="loading"
          height="100%"
          border
          class="sm:h-full"
          :data="data"
          row-key="id"
          @selection-change="handleSelectionChange"
        >
          <ElTableColumn v-for="col in columns" :key="col.prop || col.type" v-bind="col" />
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
    </ElCard>

    <DeviceGroupOperateDrawer
      v-model:visible="drawerVisible"
      :operate-type="operateType"
      :row-data="editingRow"
      :tree-data="treeData"
      @submitted="handleDrawerSubmitted"
    />
  </div>
</template>

<style lang="scss" scoped>
:deep(.el-card) {
  .ht50 {
    height: calc(100% - 50px);
  }
}
</style>
