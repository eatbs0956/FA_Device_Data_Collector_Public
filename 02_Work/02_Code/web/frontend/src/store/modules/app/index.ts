import { effectScope, nextTick, onScopeDispose, ref, watch } from 'vue';
import { breakpointsTailwind, useBreakpoints, useEventListener, useTitle } from '@vueuse/core';
import { defineStore } from 'pinia';
import { useBoolean } from '@sa/hooks';
import { router } from '@/router';
import { localStg } from '@/utils/storage';
import { SetupStoreId } from '@/enum';
import { $t, setLocale } from '@/locales';
import { setDayjsLocale } from '@/locales/dayjs';
import { useRouteStore } from '../route';
import { useTabStore } from '../tab';
import { useThemeStore } from '../theme';

/**
 * 应用主状态管理仓库。
 *
 * 提供全局应用相关的状态和操作方法，包括主题设置、路由缓存、标签页管理、响应式布局（移动端/桌面端）、多语言切换、内容区域控制、侧边栏折叠与固定等功能。
 *
 * - `isMobile`: 是否为移动端布局。
 * - `reloadFlag`: 页面重载标志。
 * - `reloadPage(duration)`: 重载页面，可指定动画持续时间。
 * - `fullContent`: 是否全屏内容显示。
 * - `locale`: 当前语言类型。
 * - `localeOptions`: 支持的语言选项。
 * - `changeLocale(lang)`: 切换语言。
 * - `themeDrawerVisible`: 主题抽屉是否可见。
 * - `openThemeDrawer()`: 打开主题抽屉。
 * - `closeThemeDrawer()`: 关闭主题抽屉。
 * - `toggleFullContent()`: 切换全屏内容显示。
 * - `contentXScrollable`: 内容区域是否可横向滚动。
 * - `setContentXScrollable(bool)`: 设置内容区域横向滚动。
 * - `siderCollapse`: 侧边栏是否折叠。
 * - `setSiderCollapse(bool)`: 设置侧边栏折叠状态。
 * - `toggleSiderCollapse()`: 切换侧边栏折叠状态。
 * - `mixSiderFixed`: 混合侧边栏是否固定。
 * - `setMixSiderFixed(bool)`: 设置混合侧边栏固定状态。
 * - `toggleMixSiderFixed()`: 切换混合侧边栏固定状态。
 *
 * 自动监听移动端布局和语言切换，处理相关状态同步和缓存。
 */
export const useAppStore = defineStore(SetupStoreId.App, () => {
  const themeStore = useThemeStore();
  const routeStore = useRouteStore();
  const tabStore = useTabStore();
  const scope = effectScope();
  const breakpoints = useBreakpoints(breakpointsTailwind);
  const { bool: themeDrawerVisible, setTrue: openThemeDrawer, setFalse: closeThemeDrawer } = useBoolean();
  const { bool: reloadFlag, setBool: setReloadFlag } = useBoolean(true);
  const { bool: fullContent, toggle: toggleFullContent } = useBoolean();
  const { bool: contentXScrollable, setBool: setContentXScrollable } = useBoolean();
  const { bool: siderCollapse, setBool: setSiderCollapse, toggle: toggleSiderCollapse } = useBoolean();
  const {
    bool: mixSiderFixed,
    setBool: setMixSiderFixed,
    toggle: toggleMixSiderFixed
  } = useBoolean(localStg.get('mixSiderFixed') === 'Y');

  /** Is mobile layout */
  const isMobile = breakpoints.smaller('sm');

  /**
   * Reload page
   *
   * @param duration Duration time
   */
  async function reloadPage(duration = 300) {
    setReloadFlag(false);

    const d = themeStore.page.animate ? duration : 40;

    await new Promise(resolve => {
      setTimeout(resolve, d);
    });

    setReloadFlag(true);

    if (themeStore.resetCacheStrategy === 'refresh') {
      routeStore.resetRouteCache();
    }
  }

  const locale = ref<App.I18n.LangType>(localStg.get('lang') || 'zh-CN');

  const localeOptions: App.I18n.LangOption[] = [
    { label: '中文', key: 'zh-CN' },
    { label: 'English', key: 'en-US' }
  ];

  function changeLocale(lang: App.I18n.LangType) {
    locale.value = lang;
    setLocale(lang);
    localStg.set('lang', lang);
  }

  /** Update document title by locale */
  function updateDocumentTitleByLocale() {
    const { i18nKey, title } = router.currentRoute.value.meta;

    const documentTitle = i18nKey ? $t(i18nKey) : title;

    useTitle(documentTitle);
  }

  function init() {
    setDayjsLocale(locale.value);
  }

  // watch store
  scope.run(() => {
    // watch isMobile, if is mobile, collapse sider
    watch(
      isMobile,
      newValue => {
        if (newValue) {
          // backup theme setting before is mobile
          localStg.set('backupThemeSettingBeforeIsMobile', {
            layout: themeStore.layout.mode,
            siderCollapse: siderCollapse.value
          });

          themeStore.setThemeLayout('vertical');
          setSiderCollapse(true);
        } else {
          // when is not mobile, recover the backup theme setting
          const backup = localStg.get('backupThemeSettingBeforeIsMobile');

          if (backup) {
            nextTick(() => {
              themeStore.setThemeLayout(backup.layout);
              setSiderCollapse(backup.siderCollapse);

              localStg.remove('backupThemeSettingBeforeIsMobile');
            });
          }
        }
      },
      { immediate: true }
    );

    // watch locale
    watch(locale, () => {
      // update document title by locale
      updateDocumentTitleByLocale();

      // update global menus by locale
      routeStore.updateGlobalMenusByLocale();

      // update tabs by locale
      tabStore.updateTabsByLocale();

      // set dayjs locale
      setDayjsLocale(locale.value);
    });
  });

  // cache mixSiderFixed
  useEventListener(window, 'beforeunload', () => {
    localStg.set('mixSiderFixed', mixSiderFixed.value ? 'Y' : 'N');
  });

  /** On scope dispose */
  onScopeDispose(() => {
    scope.stop();
  });

  // init
  init();

  return {
    isMobile,
    reloadFlag,
    reloadPage,
    fullContent,
    locale,
    localeOptions,
    changeLocale,
    themeDrawerVisible,
    openThemeDrawer,
    closeThemeDrawer,
    toggleFullContent,
    contentXScrollable,
    setContentXScrollable,
    siderCollapse,
    setSiderCollapse,
    toggleSiderCollapse,
    mixSiderFixed,
    setMixSiderFixed,
    toggleMixSiderFixed
  };
});
