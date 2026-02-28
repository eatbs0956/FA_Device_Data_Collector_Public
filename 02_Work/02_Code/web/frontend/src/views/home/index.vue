<script setup lang="ts">
import { ref } from 'vue';
import { VueDraggable } from 'vue-draggable-plus';
import DashboardWidget from './modules/dashboard-widget.vue';
import HeaderBanner from './modules/header-banner.vue';

// 看板组件配置
interface DashboardItem {
  id: string;
  type: string;
  width: number; // 1-4，表示占几列
  height: number; // 组件高度（px）
}

// 默认布局
const dashboardItems = ref<DashboardItem[]>([
  { id: 'summary', type: 'summary', width: 2, height: 240 },
  { id: 'deviceStatus', type: 'deviceStatus', width: 2, height: 240 },
  { id: 'trend', type: 'trend', width: 4, height: 280 },
  { id: 'alerts', type: 'alerts', width: 4, height: 200 }
]);

// 编辑模式
const isEditing = ref(false);

// 获取可添加的组件类型
const availableWidgets = [
  { type: 'summary', label: '系统概览' },
  { type: 'deviceStatus', label: '设备状态' },
  { type: 'trend', label: '数据趋势' },
  { type: 'alerts', label: '告警信息' }
];

// 切换编辑模式
function toggleEditMode() {
  isEditing.value = !isEditing.value;
}

// 添加组件
function addWidget(type: string) {
  const newId = `${type}_${Date.now()}`;
  dashboardItems.value.push({
    id: newId,
    type,
    width: type === 'trend' ? 4 : 2,
    height: 240
  });
}

// 删除组件
function removeWidget(id: string) {
  const index = dashboardItems.value.findIndex(item => item.id === id);
  if (index !== -1) {
    dashboardItems.value.splice(index, 1);
  }
}

// 获取组件宽度类
function getWidthClass(width: number) {
  const widthMap: Record<number, string> = {
    1: 'col-span-1',
    2: 'col-span-2',
    3: 'col-span-3',
    4: 'col-span-4'
  };
  return widthMap[width] || 'col-span-2';
}
</script>

<template>
  <div class="h-full overflow-auto p-4">
    <!-- 头部横幅 -->
    <HeaderBanner class="mb-4" />

    <!-- 操作栏 -->
    <div class="mb-4 flex items-center justify-between">
      <h2 class="text-lg font-medium">数据看板</h2>
      <div class="flex items-center gap-2">
        <ElDropdown v-if="isEditing" trigger="click" @command="addWidget">
          <ElButton type="primary" size="small">
            <template #icon>
              <SvgIcon icon="ep:plus" />
            </template>
            添加组件
          </ElButton>
          <template #dropdown>
            <ElDropdownMenu>
              <ElDropdownItem v-for="widget in availableWidgets" :key="widget.type" :command="widget.type">
                {{ widget.label }}
              </ElDropdownItem>
            </ElDropdownMenu>
          </template>
        </ElDropdown>
        <ElButton size="small" :type="isEditing ? 'success' : 'default'" @click="toggleEditMode">
          <template #icon>
            <SvgIcon :icon="isEditing ? 'ep:check' : 'ep:edit'" />
          </template>
          {{ isEditing ? '完成' : '编辑布局' }}
        </ElButton>
      </div>
    </div>

    <!-- 可拖拽看板 -->
    <VueDraggable
      v-model="dashboardItems"
      :disabled="!isEditing"
      :animation="200"
      handle=".el-card__header"
      class="grid grid-cols-4 gap-4"
    >
      <div
        v-for="item in dashboardItems"
        :key="item.id"
        :class="[getWidthClass(item.width), isEditing ? 'cursor-move' : '']"
        :style="{ height: `${item.height}px` }"
      >
        <DashboardWidget :widget-id="item.id" :widget-type="item.type" @remove="removeWidget" />
      </div>
    </VueDraggable>

    <!-- 空状态 -->
    <div v-if="dashboardItems.length === 0" class="h-64 flex items-center justify-center text-gray-400">
      <div class="text-center">
        <SvgIcon icon="ep:grid" class="mb-2 text-48px" />
        <div>暂无看板组件</div>
        <ElButton v-if="isEditing" class="mt-4" type="primary" @click="addWidget('summary')">添加组件</ElButton>
      </div>
    </div>
  </div>
</template>

<style scoped>
/* 可添加自定义样式 */
</style>
