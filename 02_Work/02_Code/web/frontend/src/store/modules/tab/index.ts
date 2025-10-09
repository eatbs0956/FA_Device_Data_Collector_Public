import { computed, ref } from 'vue';
import { useEventListener } from '@vueuse/core';
import { defineStore } from 'pinia';
import type { RouteKey } from '@elegant-router/types';
import { router } from '@/router';
import { useRouteStore } from '@/store/modules/route';
import { useRouterPush } from '@/hooks/common/router';
import { localStg } from '@/utils/storage';
import { SetupStoreId } from '@/enum';
import { useThemeStore } from '../theme';
import {
  extractTabsByAllRoutes,
  filterTabsByIds,
  findTabByRouteName,
  getAllTabs,
  getDefaultHomeTab,
  getFixedTabIds,
  getTabByRoute,
  getTabIdByRoute,
  isTabInTabs,
  updateTabByI18nKey,
  updateTabsByI18nKey
} from './shared';

/**
 * 用于管理应用标签页（Tab）的 Pinia Store。
 *
 * 该 Store 提供了标签页的增删、切换、缓存、国际化等功能，支持多标签页的动态操作和持久化。
 *
 * ## 主要功能
 * - 初始化首页标签
 * - 初始化标签页（支持缓存恢复）
 * - 添加、移除标签页
 * - 按标签ID或路由名移除标签
 * - 清空标签页（支持排除指定标签）
 * - 清空左侧/右侧标签页
 * - 切换标签页路由
 * - 设置/重置标签页标题
 * - 判断标签页是否为固定标签
 * - 根据当前语言环境更新标签页
 * - 缓存标签页到本地存储
 *
 * ## 返回值
 * 返回包含所有标签页操作方法和状态的对象，包括：
 * - `tabs`: 所有标签页（包含首页）
 * - `activeTabId`: 当前激活标签页ID
 * - `initHomeTab`: 初始化首页标签
 * - `initTabStore`: 初始化标签页
 * - `addTab`: 添加标签页
 * - `removeTab`: 移除指定标签页
 * - `removeActiveTab`: 移除当前激活标签页
 * - `removeTabByRouteName`: 按路由名移除标签页
 * - `clearTabs`: 清空标签页
 * - `clearLeftTabs`: 清空左侧标签页
 * - `clearRightTabs`: 清空右侧标签页
 * - `switchRouteByTab`: 切换到指定标签页
 * - `setTabLabel`: 设置标签页标题
 * - `resetTabLabel`: 重置标签页标题
 * - `isTabRetain`: 判断标签页是否为固定标签
 * - `updateTabsByLocale`: 根据语言环境更新标签页
 * - `getTabIdByRoute`: 根据路由获取标签ID
 * - `cacheTabs`: 缓存标签页
 *
 * @see App.Global.Tab
 * @see App.Global.TabRoute
 */
export const useTabStore = defineStore(SetupStoreId.Tab, () => {
  const routeStore = useRouteStore();
  const themeStore = useThemeStore();
  const { routerPush } = useRouterPush(false);

  /** Tabs */
  const tabs = ref<App.Global.Tab[]>([]);

  /** Get active tab */
  const homeTab = ref<App.Global.Tab>();

  /** Init home tab */
  function initHomeTab() {
    homeTab.value = getDefaultHomeTab(router, routeStore.routeHome);
  }

  /** Get all tabs */
  const allTabs = computed(() => getAllTabs(tabs.value, homeTab.value));

  /** Active tab id */
  const activeTabId = ref<string>('');

  /**
   * Set active tab id
   *
   * @param id Tab id
   */
  function setActiveTabId(id: string) {
    activeTabId.value = id;
  }

  /**
   * Init tab store
   *
   * @param currentRoute Current route
   */
  function initTabStore(currentRoute: App.Global.TabRoute) {
    const storageTabs = localStg.get('globalTabs');

    if (themeStore.tab.cache && storageTabs) {
      const extractedTabs = extractTabsByAllRoutes(router, storageTabs);
      tabs.value = updateTabsByI18nKey(extractedTabs);
    }

    addTab(currentRoute);
  }

  /**
   * Add tab
   *
   * @param route Tab route
   * @param active Whether to activate the added tab
   */
  function addTab(route: App.Global.TabRoute, active = true) {
    const tab = getTabByRoute(route);

    const isHomeTab = tab.id === homeTab.value?.id;

    if (!isHomeTab && !isTabInTabs(tab.id, tabs.value)) {
      tabs.value.push(tab);
    }

    if (active) {
      setActiveTabId(tab.id);
    }
  }

  /**
   * Remove tab when remove active tab, switch to next tab or home tab
   *
   * @param tabId Tab id
   */
  async function removeTab(tabId: string) {
    const removeTabIndex = tabs.value.findIndex(tab => tab.id === tabId);
    if (removeTabIndex === -1) return;

    const removedTabRouteKey = tabs.value[removeTabIndex].routeKey;
    const isRemoveActiveTab = activeTabId.value === tabId;

    // if remove the last tab, then switch to the second last tab
    const nextTab = tabs.value[removeTabIndex + 1] || tabs.value[removeTabIndex - 1] || homeTab.value;

    // remove tab
    tabs.value.splice(removeTabIndex, 1);

    // if current tab is removed, then switch to next tab
    if (isRemoveActiveTab && nextTab) {
      await switchRouteByTab(nextTab);
    }

    // reset route cache if cache strategy is close
    if (themeStore.resetCacheStrategy === 'close') {
      routeStore.resetRouteCache(removedTabRouteKey);
    }
  }

  /** remove active tab */
  async function removeActiveTab() {
    await removeTab(activeTabId.value);
  }

  /**
   * remove tab by route name
   *
   * @param routeName route name
   */
  async function removeTabByRouteName(routeName: RouteKey) {
    const tab = findTabByRouteName(routeName, tabs.value);
    if (!tab) return;

    await removeTab(tab.id);
  }

  /**
   * Clear tabs
   *
   * @param excludes Exclude tab ids
   */
  async function clearTabs(excludes: string[] = []) {
    const remainTabIds = [...getFixedTabIds(tabs.value), ...excludes];

    // Identify tabs to be removed and collect their routeKeys if strategy is 'close'
    const tabsToRemove = tabs.value.filter(tab => !remainTabIds.includes(tab.id));
    const routeKeysToReset: RouteKey[] = [];

    if (themeStore.resetCacheStrategy === 'close') {
      for (const tab of tabsToRemove) {
        routeKeysToReset.push(tab.routeKey);
      }
    }

    const removedTabsIds = tabsToRemove.map(tab => tab.id);

    // If no tabs are actually being removed based on excludes and fixed tabs, exit
    if (removedTabsIds.length === 0) {
      return;
    }

    const isRemoveActiveTab = removedTabsIds.includes(activeTabId.value);
    // filterTabsByIds returns tabs NOT in removedTabsIds, so these are the tabs that will remain
    const updatedTabs = filterTabsByIds(removedTabsIds, tabs.value);

    function update() {
      tabs.value = updatedTabs;
    }

    if (isRemoveActiveTab) {
      const activeTabCandidate = updatedTabs[updatedTabs.length - 1] || homeTab.value;

      if (activeTabCandidate) {
        // Ensure there's a tab to switch to
        await switchRouteByTab(activeTabCandidate);
      }
    }
    // Update the tabs array regardless of switch success or if a candidate was found
    update();

    // After tabs are updated and route potentially switched, reset cache for removed tabs
    for (const routeKey of routeKeysToReset) {
      routeStore.resetRouteCache(routeKey);
    }
  }

  /**
   * Switch route by tab
   *
   * @param tab
   */
  async function switchRouteByTab(tab: App.Global.Tab) {
    const fail = await routerPush(tab.fullPath);
    if (!fail) {
      setActiveTabId(tab.id);
    }
  }

  /**
   * Clear left tabs
   *
   * @param tabId
   */
  async function clearLeftTabs(tabId: string) {
    const tabIds = tabs.value.map(tab => tab.id);
    const index = tabIds.indexOf(tabId);
    if (index === -1) return;

    const excludes = tabIds.slice(index);
    await clearTabs(excludes);
  }

  /**
   * Clear right tabs
   *
   * @param tabId
   */
  async function clearRightTabs(tabId: string) {
    const isHomeTab = tabId === homeTab.value?.id;
    if (isHomeTab) {
      clearTabs();
      return;
    }

    const tabIds = tabs.value.map(tab => tab.id);
    const index = tabIds.indexOf(tabId);
    if (index === -1) return;

    const excludes = tabIds.slice(0, index + 1);
    await clearTabs(excludes);
  }

  /**
   * Set new label of tab
   *
   * @default activeTabId
   * @param label New tab label
   * @param tabId Tab id
   */
  function setTabLabel(label: string, tabId?: string) {
    const id = tabId || activeTabId.value;

    const tab = tabs.value.find(item => item.id === id);
    if (!tab) return;

    tab.oldLabel = tab.label;
    tab.newLabel = label;
  }

  /**
   * Reset tab label
   *
   * @default activeTabId
   * @param tabId Tab id
   */
  function resetTabLabel(tabId?: string) {
    const id = tabId || activeTabId.value;

    const tab = tabs.value.find(item => item.id === id);
    if (!tab) return;

    tab.newLabel = undefined;
  }

  /**
   * Is tab retain
   *
   * @param tabId
   */
  function isTabRetain(tabId: string) {
    if (tabId === homeTab.value?.id) return true;

    const fixedTabIds = getFixedTabIds(tabs.value);

    return fixedTabIds.includes(tabId);
  }

  /** Update tabs by locale */
  function updateTabsByLocale() {
    tabs.value = updateTabsByI18nKey(tabs.value);

    if (homeTab.value) {
      homeTab.value = updateTabByI18nKey(homeTab.value);
    }
  }

  /** Cache tabs */
  function cacheTabs() {
    if (!themeStore.tab.cache) return;

    localStg.set('globalTabs', tabs.value);
  }

  // cache tabs when page is closed or refreshed
  useEventListener(window, 'beforeunload', () => {
    cacheTabs();
  });

  return {
    /** All tabs */
    tabs: allTabs,
    activeTabId,
    initHomeTab,
    initTabStore,
    addTab,
    removeTab,
    removeActiveTab,
    removeTabByRouteName,
    clearTabs,
    clearLeftTabs,
    clearRightTabs,
    switchRouteByTab,
    setTabLabel,
    resetTabLabel,
    isTabRetain,
    updateTabsByLocale,
    getTabIdByRoute,
    cacheTabs
  };
});
