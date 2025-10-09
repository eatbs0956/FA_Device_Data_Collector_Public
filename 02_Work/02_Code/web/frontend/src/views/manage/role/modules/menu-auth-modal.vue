<script setup lang="ts">
import { computed, nextTick, ref, shallowRef, watch } from 'vue';
import type { ElTree as ElTreeType } from 'element-plus';
import { fetchGetAllPages, fetchGetMenuTree, fetchGetRoleMenus, fetchSaveRoleMenus } from '@/service/api';
import { $t } from '@/locales';

defineOptions({ name: 'MenuAuthModal' });

interface Props {
  /** the roleId */
  roleId: string;
}

const props = defineProps<Props>();

const visible = defineModel<boolean>('visible', {
  default: false
});

// 数据加载状态
const loading = ref(false);

function closeModal() {
  visible.value = false;
}

const title = computed(() => $t('common.edit') + $t('page.manage.role.menuAuth'));

const home = shallowRef('');

async function getHome() {
  // eslint-disable-next-line no-console
  console.log(props.roleId);

  home.value = 'home';
}

const pages = shallowRef<string[]>([]);

async function getPages() {
  const { error, data } = await fetchGetAllPages();

  if (!error) {
    pages.value = data;
  }
}

const pageSelectOptions = computed(() => {
  const opts: CommonType.Option[] = pages.value.map(page => ({
    label: page,
    value: page
  }));

  return opts;
});

const tree = shallowRef<Api.SystemManage.MenuTree[]>([]);

// 树组件引用
const treeRef = ref<InstanceType<typeof ElTreeType> | null>(null);

async function getTree() {
  const { error, data } = await fetchGetMenuTree();

  if (!error) {
    tree.value = data;
  }
}

const checks = shallowRef<number[]>([]);

/**
 * 递归获取所有叶子节点ID（没有children或children为空的节点）
 */
function getLeafNodeIds(nodes: Api.SystemManage.MenuTree[]): number[] {
  const leafIds: number[] = [];

  function traverse(menuList: Api.SystemManage.MenuTree[]) {
    for (const menu of menuList) {
      if (!menu.children || menu.children.length === 0) {
        // 叶子节点
        leafIds.push(menu.id);
      } else {
        // 有子节点，继续遍历
        traverse(menu.children);
      }
    }
  }

  traverse(nodes);
  return leafIds;
}

/**
 * 获取角色已有的菜单权限
 */
async function getChecks() {
  if (!props.roleId) {
    // 如果没有roleId，默认全不选
    checks.value = [];
    return;
  }

  // 调用API获取该角色已有的菜单权限
  const { error, data } = await fetchGetRoleMenus(props.roleId);

  if (!error && data) {
    // Element Plus Tree 组件要求：
    // 只设置叶子节点的ID，父节点会自动根据子节点选中状态计算
    // 否则会导致选中状态异常
    const allMenuIds = data;
    const leafIds = getLeafNodeIds(tree.value);

    // 只保留叶子节点的ID
    checks.value = allMenuIds.filter(id => leafIds.includes(id));
  } else {
    // 如果获取失败，默认全不选
    checks.value = [];
  }
}

function checkChange(val: number) {
  const idx = checks.value.indexOf(val);
  if (idx === -1) {
    checks.value.push(val);
  } else {
    checks.value.splice(idx, 1);
  }
}

async function handleSubmit() {
  if (!props.roleId) {
    window.$message?.error?.('角色ID不能为空');
    return;
  }

  // 获取树组件中所有选中的节点（包括半选中的父节点）
  // getCheckedKeys: 获取完全选中的节点
  // getHalfCheckedKeys: 获取半选中的父节点
  const checkedKeys = treeRef.value?.getCheckedKeys() || [];
  const halfCheckedKeys = treeRef.value?.getHalfCheckedKeys() || [];

  // 合并完全选中和半选中的节点ID
  const allCheckedIds = [...checkedKeys, ...halfCheckedKeys] as number[];

  // 调用API保存角色菜单权限
  const { error } = await fetchSaveRoleMenus(props.roleId, allCheckedIds);

  if (error) {
    window.$message?.error?.(error.message || '保存失败');
    return;
  }

  window.$message?.success?.($t('common.modifySuccess'));

  closeModal();
}

async function init() {
  // 设置加载状态，避免树组件先显示再加载选中状态
  loading.value = true;

  try {
    // 并行加载页面列表和首页信息（不影响主流程）
    getHome();
    getPages();

    // 先获取菜单树
    await getTree();

    // 等待下一个 tick，确保树组件已经渲染
    await nextTick();

    // 然后获取并设置选中状态
    await getChecks();

    // 再等待一个 tick，确保选中状态已经应用到树组件
    await nextTick();
  } finally {
    // 数据加载完成，显示内容
    loading.value = false;
  }
}

watch(visible, val => {
  if (val) {
    init();
  }
});
</script>

<template>
  <ElDialog v-model="visible" :title="title" preset="card" class="w-480px">
    <div v-loading="loading" class="min-h-320px">
      <div class="flex-y-center gap-16px pb-12px">
        <div>{{ $t('page.manage.menu.home') }}</div>
        <ElSelect v-model="home" :options="pageSelectOptions" size="small" class="w-160px">
          <ElOption v-for="{ value, label } in pageSelectOptions" :key="value" :value="value" :label="label"></ElOption>
        </ElSelect>
      </div>
      <ElTree
        v-if="!loading"
        ref="treeRef"
        v-model:checked-keys="checks"
        :data="tree"
        node-key="id"
        show-checkbox
        default-expand-all
        class="h-280px overflow-y-auto"
        :default-checked-keys="checks"
        @check-change="checkChange"
      />
    </div>
    <template #footer>
      <ElSpace class="w-full justify-end">
        <ElButton size="small" class="mt-16px" @click="closeModal">
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
