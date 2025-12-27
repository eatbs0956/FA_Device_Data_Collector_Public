<script setup lang="tsx">
import { ref } from 'vue';
import type { Ref } from 'vue';
import { ElButton, ElPopconfirm, ElTag } from 'element-plus';
import { useBoolean } from '@sa/hooks';
import { yesOrNoRecord } from '@/constants/common';
import { enableStatusRecord, menuTypeRecord } from '@/constants/business';
import { fetchBatchDeleteMenus, fetchDeleteMenu, fetchGetAllPages, fetchGetMenuList } from '@/service/api';
import { useRouteStore } from '@/store/modules/route';
import { useTableOperate, useUIPaginatedTable } from '@/hooks/common/table';
import { $t } from '@/locales';
import SvgIcon from '@/components/custom/svg-icon.vue';
import MenuOperateModal, { type OperateType } from './modules/menu-operate-modal.vue';

// 定义带 children 属性的菜单类型
interface MenuWithChildren extends Api.SystemManage.Menu {
  children?: MenuWithChildren[];
}

// 模态框显示状态控制 - 使用布尔值管理菜单操作模态框的显示/隐藏
const { bool: visible, setTrue: openModal } = useBoolean();

// 路由Store - 用于刷新导航栏
const routeStore = useRouteStore();

// 容器引用 - 页面根容器DOM元素引用
const wrapperRef = ref<HTMLElement | null>(null);

/**
 * 自定义转换函数 - 后端已返回树形结构，直接使用
 */
function transformMenuResponse(response: any) {
  const { data, error } = response;

  if (!error) {
    const { records, current, size, total } = data;
    // 后端已经返回树形结构，直接使用
    return {
      data: records as MenuWithChildren[],
      pageNum: current,
      pageSize: size,
      total
    };
  }

  return {
    data: [] as MenuWithChildren[],
    pageNum: 1,
    pageSize: 10,
    total: 0
  };
}

// 搜索参数 - 存储分页参数
const searchParams = {
  current: 1,
  size: 10
};

// 表格配置和数据管理 - 使用分页表格Hook管理菜单列表的显示、加载和分页
const { columns, columnChecks, data, loading, pagination, getData, getDataByPage } = useUIPaginatedTable({
  // API接口配置 - 获取菜单列表数据的接口函数，使用 searchParams 传递分页参数
  api: () => fetchGetMenuList(searchParams.current, searchParams.size),
  // 数据转换函数 - 将API响应数据转换为树形结构格式
  transform: transformMenuResponse,
  // 分页参数变化回调 - 当分页参数改变时更新 searchParams
  onPaginationParamsChange: params => {
    searchParams.current = params.currentPage ?? 1;
    searchParams.size = params.pageSize ?? 10;
  },
  // 列配置函数 - 定义表格列的结构和渲染方式
  columns: () => [
    { type: 'selection', width: 48 }, // 选择列 - 多选框列，用于批量操作
    { prop: 'id', label: $t('page.manage.menu.id') }, // ID列 - 菜单唯一标识符
    {
      // 菜单类型列 - 显示菜单类型（目录/菜单）并使用标签样式
      prop: 'menuType',
      label: $t('page.manage.menu.menuType'),
      width: 90,
      formatter: row => {
        // 标签颜色映射 - 根据菜单类型设置不同的标签颜色
        const tagMap: Record<Api.SystemManage.MenuType, UI.ThemeColor> = {
          1: 'info', // 目录类型 - 信息色
          2: 'primary' // 菜单类型 - 主色
        };

        // 获取国际化标签文本
        const label = $t(menuTypeRecord[row.menuType]);

        return <ElTag type={tagMap[row.menuType]}>{label}</ElTag>;
      }
    },
    {
      prop: 'menuName',
      label: $t('page.manage.menu.menuName'),
      minWidth: 120,
      formatter: row => {
        const { i18nKey, menuName } = row;

        const label = i18nKey ? $t(i18nKey) : menuName;

        return <span>{label}</span>;
      }
    },
    {
      prop: 'icon',
      label: $t('page.manage.menu.icon'),
      width: 100,
      formatter: row => {
        const icon = row.iconType === '1' ? row.icon : undefined;

        const localIcon = row.iconType === '2' ? row.icon : undefined;

        return (
          <div class="flex-center">
            <SvgIcon icon={icon} localIcon={localIcon} class="text-icon" />
          </div>
        );
      }
    },
    { prop: 'routeName', label: $t('page.manage.menu.routeName'), minWidth: 120 },
    { prop: 'routePath', label: $t('page.manage.menu.routePath'), minWidth: 120 },
    {
      prop: 'status',
      label: $t('page.manage.menu.menuStatus'),
      width: 80,
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
      prop: 'hideInMenu',
      label: $t('page.manage.menu.hideInMenu'),
      width: 80,
      formatter: row => {
        const hide: CommonType.YesOrNo = row.hideInMenu ? 'Y' : 'N';

        const tagMap: Record<CommonType.YesOrNo, UI.ThemeColor> = {
          Y: 'danger',
          N: 'info'
        };

        const label = $t(yesOrNoRecord[hide]);

        return <ElTag type={tagMap[hide]}>{label}</ElTag>;
      }
    },
    { prop: 'parentId', label: $t('page.manage.menu.parentId'), width: 90 },
    { prop: 'order', label: $t('page.manage.menu.order'), width: 60 },
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
      width: 270,
      formatter: row => (
        <div class="flex-center justify-end pr-10px">
          {row.menuType === '1' && (
            <ElButton type="primary" plain size="small" onClick={() => handleAddChildMenu(row)}>
              {$t('page.manage.menu.addChildMenu')}
            </ElButton>
          )}
          <ElButton type="primary" plain size="small" onClick={() => handleEdit(row)}>
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

// 表格操作管理 - 处理表格的选中行、批量删除和单项删除操作
const { checkedRowKeys, onDeleted } = useTableOperate(data, 'id', getData);

// 操作类型 - 当前菜单操作的类型（新增/编辑/添加子菜单）
const operateType = ref<OperateType>('add');

/**
 * 处理新增菜单 - 打开新增菜单的操作模态框
 */
function handleAdd() {
  // 设置操作类型为新增
  operateType.value = 'add';
  // 打开操作模态框
  openModal();
}

/**
 * 处理批量删除 - 删除选中的多个菜单项
 */
async function handleBatchDelete() {
  if (checkedRowKeys.value.length === 0) {
    window.$message?.warning('请选择要删除的数据');
    return;
  }

  // 调用批量删除API - checkedRowKeys 实际上是菜单对象数组（由于 @selection-change 绑定）
  const ids = (checkedRowKeys.value as unknown as MenuWithChildren[]).map(item => item.id.toString());
  const { error } = await fetchBatchDeleteMenus(ids);

  if (error) {
    window.$message?.error(error.message || '删除失败');
    return;
  }

  window.$message?.success($t('common.deleteSuccess'));

  // 清空选中项
  checkedRowKeys.value = [];

  // 刷新数据
  await getData();

  // 刷新路由以更新导航栏
  await routeStore.initAuthRoute();
}

/**
 * 处理单项删除 - 删除指定ID的菜单项
 * @param id 菜单ID - 要删除的菜单唯一标识符
 */
async function handleDelete(id: number) {
  // 调用删除API
  const { error } = await fetchDeleteMenu(id.toString());

  if (error) {
    window.$message?.error(error.message || '删除失败');
    return;
  }

  window.$message?.success($t('common.deleteSuccess'));

  // 执行删除后的UI更新
  onDeleted();

  // 刷新路由以更新导航栏
  await routeStore.initAuthRoute();
}

/** 编辑数据 - 当前正在编辑的菜单数据，或添加子菜单时的父菜单数据 */
const editingData: Ref<Api.SystemManage.Menu | null> = ref(null);

/**
 * 处理编辑菜单 - 打开编辑指定菜单项的操作模态框
 * @param item 菜单项 - 要编辑的菜单数据对象
 */
function handleEdit(item: Api.SystemManage.Menu) {
  // 设置操作类型为编辑
  operateType.value = 'edit';
  // 复制菜单数据到编辑状态（避免直接修改原数据）
  editingData.value = { ...item };

  // 打开操作模态框
  openModal();
}

/**
 * 处理添加子菜单 - 为指定菜单项添加子菜单
 * @param item 父菜单项 - 要添加子菜单的父菜单数据对象
 */
function handleAddChildMenu(item: Api.SystemManage.Menu) {
  // 设置操作类型为添加子菜单
  operateType.value = 'addChild';

  // 设置父菜单数据
  editingData.value = { ...item };

  // 打开操作模态框
  openModal();
}

// 所有页面路由 - 系统中所有可用的页面路由列表
const allPages = ref<string[]>([]);

/**
 * 获取所有页面 - 从服务器获取系统中所有可用的页面路由
 */
async function getAllPages() {
  // 调用API获取页面列表
  const { data: pages } = await fetchGetAllPages();
  // 更新页面列表数据（处理空值情况）
  allPages.value = pages || [];
}

/**
 * 初始化函数 - 组件挂载时执行的初始化操作
 */
function init() {
  // 获取所有可用页面列表
  getAllPages();
}

// 执行初始化
init();
</script>

<template>
  <div ref="wrapperRef" class="flex-col-stretch gap-16px overflow-hidden lt-sm:overflow-auto">
    <ElCard class="card-wrapper sm:flex-1-hidden" body-class="ht50">
      <template #header>
        <div class="flex items-center justify-between">
          <p>{{ $t('page.manage.menu.title') }}</p>
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
          :default-expand-all="false"
          :tree-props="{ children: 'children', hasChildren: 'hasChildren' }"
          @selection-change="checkedRowKeys = $event"
        >
          <ElTableColumn v-for="col in columns" :key="col.prop" v-bind="col" />
        </ElTable>
        <div class="mt-20px flex justify-end">
          <ElPagination
            v-if="pagination.total"
            layout="total,prev,pager,next,sizes"
            v-bind="pagination"
            @current-change="pagination['current-change']"
            @size-change="pagination['size-change']"
          />
        </div>
      </div>
      <MenuOperateModal
        v-model:visible="visible"
        :operate-type="operateType"
        :row-data="editingData"
        :all-pages="allPages"
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
