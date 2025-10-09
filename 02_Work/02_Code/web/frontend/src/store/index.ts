import type { App } from 'vue';
import { createPinia } from 'pinia';
import { resetSetupStore } from './plugins';

/**
 * 设置并初始化 Pinia 状态管理，并将其注册到 Vue 应用实例中。
 * 此函数会应用 `resetSetupStore` 插件到 Pinia，用于扩展 store 的功能。
 *
 * @param app - Vue 应用实例，用于注册 Pinia。
 */
export function setupStore(app: App) {
  const store = createPinia();

  store.use(resetSetupStore);

  app.use(store);
}
