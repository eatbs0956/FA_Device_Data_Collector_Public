<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue';
import type { ElTree as ElTreeType } from 'element-plus';
import { fetchGetMenuList, fetchGetRoleButtons, fetchSaveRoleButtons } from '@/service/api';
import { $t } from '@/locales';

defineOptions({ name: 'ButtonAuthModal' });

interface Props {
  /** the roleId */
  roleId: string;
}

const props = defineProps<Props>();

interface Emits {
  (e: 'success'): void;
}

const emit = defineEmits<Emits>();

const visible = defineModel<boolean>('visible', {
  default: false
});

function closeModal() {
  visible.value = false;
}

const title = computed(() => $t('common.edit') + $t('page.manage.role.buttonAuth'));

type ButtonTreeNode = {
  id: string;
  label: string;
  children?: ButtonTreeNode[];
};

const tree = ref<ButtonTreeNode[]>([]);
const loading = ref(false);
const treeRef = ref<InstanceType<typeof ElTreeType> | null>(null);

/**
 * 获取所有菜单及其按钮配置
 */
async function getAllButtons() {
  loading.value = true;
  try {
    // 获取所有菜单（不分页）
    const { data, error } = await fetchGetMenuList(1, 10000);
    if (error) {
      window.$message?.error(error.message || '获取菜单列表失败');
      return;
    }

    if (!data) {
      return;
    }

    // 构建树形结构 - 后端已返回嵌套的树形结构
    const treeData: ButtonTreeNode[] = [];

    /**
     * 递归构建菜单树节点（处理后端返回的嵌套children结构）
     */
    function buildMenuTree(menu: Api.SystemManage.Menu): ButtonTreeNode | null {
      // 解析按钮配置
      let buttons: Api.SystemManage.MenuButton[] = [];
      if (menu.buttons) {
        try {
          buttons = typeof menu.buttons === 'string' ? JSON.parse(menu.buttons) : menu.buttons;
        } catch {
          buttons = [];
        }
      }

      // 获取子菜单 - 后端已经在 children 属性中返回了子菜单
      const childMenus = menu.children || [];

      // 递归构建子菜单节点
      const childNodes: ButtonTreeNode[] = [];
      childMenus.forEach(childMenu => {
        const childNode = buildMenuTree(childMenu);
        if (childNode) {
          childNodes.push(childNode);
        }
      });

      // 构建按钮节点
      const buttonNodes: ButtonTreeNode[] = buttons.map(btn => ({
        id: `${menu.id}:${btn.code}`,
        label: `${btn.desc || btn.code}`
      }));

      // 合并子节点：先显示子菜单，再显示按钮
      const allChildren = [...childNodes, ...buttonNodes];

      // 构建菜单节点
      const menuNode: ButtonTreeNode = {
        id: `menu_${menu.id}`,
        label: menu.menuName,
        children: allChildren.length > 0 ? allChildren : undefined
      };

      return menuNode;
    }

    // 从根菜单开始构建树（后端已返回根菜单列表）
    const rootMenus = data.records || [];
    rootMenus.forEach(rootMenu => {
      const node = buildMenuTree(rootMenu);
      if (node) {
        treeData.push(node);
      }
    });

    tree.value = treeData;
  } catch (err) {
    window.$message?.error('获取按钮配置失败');
    // eslint-disable-next-line no-console
    console.error('getAllButtons error:', err);
  } finally {
    loading.value = false;
  }
}

const checks = ref<string[]>([]);

/**
 * 递归获取所有叶子节点ID（按钮节点，即包含冒号的ID）
 */
function getLeafNodeIds(nodes: ButtonTreeNode[]): string[] {
  const leafIds: string[] = [];

  function traverse(nodeList: ButtonTreeNode[]) {
    for (const node of nodeList) {
      if (!node.children || node.children.length === 0) {
        // 叶子节点
        leafIds.push(node.id);
      } else {
        // 有子节点，继续遍历
        traverse(node.children);
      }
    }
  }

  traverse(nodes);
  return leafIds;
}

/**
 * 获取角色已有的按钮权限
 */
async function getChecks() {
  if (!props.roleId) {
    checks.value = [];
    return;
  }

  loading.value = true;
  try {
    const { data, error } = await fetchGetRoleButtons(props.roleId);
    if (error) {
      window.$message?.error(error.message || '获取按钮权限失败');
      checks.value = [];
      return;
    }

    // Element Plus Tree 组件要求：只设置叶子节点的ID
    const allButtonIds = data || [];
    const leafIds = getLeafNodeIds(tree.value);

    // 只保留叶子节点的ID（按钮节点）
    checks.value = allButtonIds.filter(id => leafIds.includes(id));
  } catch (err) {
    window.$message?.error('获取按钮权限失败');
    // eslint-disable-next-line no-console
    console.error('getChecks error:', err);
    checks.value = [];
  } finally {
    loading.value = false;
  }
}

/**
 * 提交保存按钮权限
 */
async function handleSubmit() {
  if (!props.roleId) {
    window.$message?.error('角色ID不能为空');
    return;
  }

  loading.value = true;
  try {
    // 获取树组件中所有选中的节点
    const checkedKeys = treeRef.value?.getCheckedKeys() || [];

    // 过滤掉菜单节点，只保留按钮节点（包含冒号的）
    const buttonCodes = (checkedKeys as string[]).filter(code => code.includes(':'));

    const { error } = await fetchSaveRoleButtons(props.roleId, buttonCodes);
    if (error) {
      window.$message?.error(error.message || '保存失败');
      return;
    }

    window.$message?.success($t('common.modifySuccess'));
    emit('success');
    closeModal();
  } catch (err) {
    window.$message?.error('保存失败');
    // eslint-disable-next-line no-console
    console.error('handleSubmit error:', err);
  } finally {
    loading.value = false;
  }
}

/**
 * 初始化数据
 */
async function init() {
  loading.value = true;
  try {
    // 先清空选中状态
    checks.value = [];
    // 获取菜单和按钮树
    await getAllButtons();
    // 等待树组件渲染
    await nextTick();
    // 获取角色已有的按钮权限
    await getChecks();
    await nextTick();
  } finally {
    loading.value = false;
  }
}

// 监听弹窗显示，重新加载数据
watch(visible, newVal => {
  if (newVal) {
    init();
  }
});
</script>

<template>
  <ElDialog v-model="visible" :title="title" preset="card" class="w-480px">
    <div v-loading="loading" class="min-h-320px">
      <ElTree
        v-if="!loading"
        ref="treeRef"
        :data="tree"
        node-key="id"
        show-checkbox
        default-expand-all
        class="h-280px overflow-y-auto"
        :default-checked-keys="checks"
        :props="{ label: 'label', children: 'children' }"
      >
        <template #default="{ node, data: nodeData }">
          <div class="flex items-center gap-8px">
            <span :class="{ 'font-medium': !nodeData.id.includes(':') }">{{ node.label }}</span>
            <ElTag v-if="nodeData.id.includes(':')" size="small" type="info" effect="plain">
              {{ nodeData.id.split(':')[1] }}
            </ElTag>
          </div>
        </template>
      </ElTree>
    </div>
    <template #footer>
      <ElSpace class="w-full justify-end">
        <ElButton size="small" class="mt-16px" :disabled="loading" @click="closeModal">
          {{ $t('common.cancel') }}
        </ElButton>
        <ElButton type="primary" size="small" class="mt-16px" :disabled="loading" @click="handleSubmit">
          {{ $t('common.confirm') }}
        </ElButton>
      </ElSpace>
    </template>
  </ElDialog>
</template>

<style scoped></style>
