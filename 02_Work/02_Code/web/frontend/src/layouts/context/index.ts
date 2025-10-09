import { computed, nextTick, ref, watch } from 'vue';
import { useRoute } from 'vue-router';
import { useContext } from '@sa/hooks';
import type { RouteKey } from '@elegant-router/types';
import { useRouteStore } from '@/store/modules/route';
import { useRouterPush } from '@/hooks/common/router';

export const { setupStore: setupMixMenuContext, useStore: useMixMenuContext } = useContext('mix-menu', useMixMenu);

/**
 * 用于菜单混合模式的自定义 Hook，管理菜单的选中状态及层级菜单数据。
 *
 * - `allMenus`：所有菜单项的响应式数据。
 * - `firstLevelMenus`：一级菜单项的响应式数据（不包含子菜单）。
 * - `childLevelMenus`：当前激活的一级菜单下的子菜单项响应式数据。
 * - `isActiveFirstLevelMenuHasChildren`：当前激活的一级菜单是否有子菜单。
 * - `activeFirstLevelMenuKey`：当前激活的一级菜单 key 的响应式引用。
 * - `setActiveFirstLevelMenuKey(key)`：设置激活的一级菜单 key。
 * - `getActiveFirstLevelMenuKey()`：根据当前选中的菜单 key 自动设置激活的一级菜单 key。
 *
 * 该 Hook 会监听路由变化，自动更新激活的一级菜单 key，适用于多级菜单的场景。
 */
function useMixMenu() {
  const route = useRoute();
  const routeStore = useRouteStore();
  const { selectedKey } = useMenu();

  const activeFirstLevelMenuKey = ref('');

  function setActiveFirstLevelMenuKey(key: string) {
    activeFirstLevelMenuKey.value = key;
  }

  function getActiveFirstLevelMenuKey() {
    const [firstLevelRouteName] = selectedKey.value.split('_');

    setActiveFirstLevelMenuKey(firstLevelRouteName);
  }

  const allMenus = computed<App.Global.Menu[]>(() => routeStore.menus);

  const firstLevelMenus = computed<App.Global.Menu[]>(() =>
    routeStore.menus.map(menu => {
      const { children: _, ...rest } = menu;

      return rest;
    })
  );

  const childLevelMenus = computed<App.Global.Menu[]>(
    () => routeStore.menus.find(menu => menu.key === activeFirstLevelMenuKey.value)?.children || []
  );

  const isActiveFirstLevelMenuHasChildren = computed(() => {
    if (!activeFirstLevelMenuKey.value) {
      return false;
    }

    const findItem = allMenus.value.find(item => item.key === activeFirstLevelMenuKey.value);

    return Boolean(findItem?.children?.length);
  });

  watch(
    () => route.name,
    () => {
      getActiveFirstLevelMenuKey();
    },
    { immediate: true }
  );

  return {
    allMenus,
    firstLevelMenus,
    childLevelMenus,
    isActiveFirstLevelMenuHasChildren,
    activeFirstLevelMenuKey,
    setActiveFirstLevelMenuKey,
    getActiveFirstLevelMenuKey
  };
}

export function useMenu() {
  const route = useRoute();
  const { routerPushByKeyWithMetaQuery } = useRouterPush();

  const selectedKey = computed(() => {
    const { hideInMenu, activeMenu } = route.meta;
    const name = route.name as string;

    const routeName = (hideInMenu ? activeMenu : name) || name;

    return routeName;
  });

  const selectedKeyDummy = ref(selectedKey.value);

  watch(
    () => selectedKey.value,
    val => {
      selectedKeyDummy.value = val;
    }
  );

  function handleSelect(key: RouteKey) {
    selectedKeyDummy.value = key;

    routerPushByKeyWithMetaQuery(key);

    if (key.endsWith('-link')) {
      nextTick(() => {
        selectedKeyDummy.value = selectedKey.value;
      });
    }
  }

  return {
    selectedKey,
    selectedKeyDummy,
    handleSelect
  };
}
