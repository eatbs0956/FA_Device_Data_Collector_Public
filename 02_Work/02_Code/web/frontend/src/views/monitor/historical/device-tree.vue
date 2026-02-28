<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import { fetchGetDeviceTree } from '@/service/api';
import { $t } from '@/locales';

const emit = defineEmits<{
  select: [node: Api.Monitor.DeviceTreeNode, selectedTags: string[]];
}>();

const loading = ref(false);
const treeData = ref<Api.Monitor.DeviceTreeNode[]>([]);
const treeRef = ref();
const filterText = ref('');

// Currently selected device node
const selectedDevice = ref<Api.Monitor.DeviceTreeNode | null>(null);
// Currently selected tags
const checkedTags = ref<string[]>([]);

// Tree node props config
const treeProps = {
  label: 'label',
  children: 'children',
  isLeaf: 'isLeaf'
};

// Load device tree
async function loadDeviceTree() {
  loading.value = true;
  try {
    const { data, error } = await fetchGetDeviceTree();
    if (!error && data) {
      treeData.value = data;
    }
  } finally {
    loading.value = false;
  }
}

// Handle node click
function handleNodeClick(node: Api.Monitor.DeviceTreeNode) {
  if (node.type === 'device') {
    selectedDevice.value = node;
    // Select all tags by default
    if (node.children) {
      checkedTags.value = node.children.map(t => t.id);
    }
    emitSelection();
    return;
  }
  if (node.type === 'tag') {
    // Toggle tag selection
    const index = checkedTags.value.indexOf(node.id);
    if (index === -1) {
      checkedTags.value.push(node.id);
    } else {
      checkedTags.value.splice(index, 1);
    }
    emitSelection();
  }
}

// Emit selection event
function emitSelection() {
  if (selectedDevice.value) {
    // Get selected tag names
    const tagNames = checkedTags.value
      .map(id => {
        const tag = selectedDevice.value?.children?.find(t => t.id === id);
        // Extract tagName from tag_deviceId_tagName format
        if (tag?.id.startsWith('tag_')) {
          const parts = tag.id.split('_');
          return parts[parts.length - 1];
        }
        return null;
      })
      .filter((name): name is string => name !== null);
    emit('select', selectedDevice.value, tagNames);
  }
}

// Filter node
function filterNode(value: string, data: Record<string, unknown>) {
  if (!value) return true;
  const label = data.label as string;
  return label.toLowerCase().includes(value.toLowerCase());
}

// Get node icon
function getNodeIcon(node: Api.Monitor.DeviceTreeNode) {
  if (node.type === 'group') {
    return 'ep:folder';
  }
  if (node.type === 'device') {
    return 'icon-park-solid:hard-disk';
  }
  return 'ep:price-tag';
}

// Get node status class
function getStatusClass(node: Api.Monitor.DeviceTreeNode) {
  if (node.type === 'device' && node.status) {
    const statusMap: Record<string, string> = {
      Connected: 'text-green-500',
      Disconnected: 'text-yellow-500',
      Error: 'text-red-500'
    };
    return statusMap[node.status] || '';
  }
  return '';
}

// Check if tag is selected
function isTagChecked(node: Api.Monitor.DeviceTreeNode) {
  return node.type === 'tag' && checkedTags.value.includes(node.id);
}

// Watch filter text
watch(filterText, val => {
  treeRef.value?.filter(val);
});

onMounted(() => {
  loadDeviceTree();
});
</script>

<template>
  <div class="device-tree h-full flex flex-col">
    <!-- Search box -->
    <div class="p-2">
      <ElInput
        v-model="filterText"
        :placeholder="$t('page.monitor.historical.searchPlaceholder')"
        clearable
        prefix-icon="ep:search"
      />
    </div>
    <!-- Tree control -->
    <div class="flex-1 overflow-auto">
      <ElTree
        v-if="!loading"
        ref="treeRef"
        :data="treeData"
        :props="treeProps"
        :filter-node-method="filterNode"
        node-key="id"
        highlight-current
        default-expand-all
        @node-click="handleNodeClick"
      >
        <template #default="{ node, data }">
          <div class="flex items-center gap-2 py-1">
            <SvgIcon :icon="getNodeIcon(data)" :class="getStatusClass(data)" />
            <span :class="{ 'font-medium': data.type === 'device' }">{{ node.label }}</span>
            <ElCheckbox
              v-if="data.type === 'tag'"
              :model-value="isTagChecked(data)"
              size="small"
              @click.stop
              @change="handleNodeClick(data)"
            />
          </div>
        </template>
      </ElTree>
      <div v-else class="p-4">
        <ElSkeleton :rows="8" animated />
      </div>
    </div>
    <!-- Refresh button -->
    <div class="border-t p-2">
      <ElButton size="small" class="w-full" @click="loadDeviceTree">
        <template #icon>
          <SvgIcon icon="ep:refresh" />
        </template>
        {{ $t('page.monitor.historical.refreshTree') }}
      </ElButton>
    </div>
  </div>
</template>

<style scoped>
.device-tree {
  border-right: 1px solid var(--el-border-color);
}
</style>
