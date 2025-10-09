import type { PiniaPluginContext } from 'pinia';
import { jsonClone } from '@sa/utils';
import { SetupStoreId } from '@/enum';

/**
 * 重置 Pinia setup 语法的 store 状态插件。
 *
 * 此插件会检查当前 store 的 `$id` 是否属于 `SetupStoreId` 枚举中的值，
 * 如果是，则为该 store 注入 `$reset` 方法。调用 `$reset` 方法时，
 * 会将 store 的状态还原为插件初始化时的默认状态。
 *
 * @param context Pinia 插件上下文对象，包含当前 store 实例等信息。
 */
export function resetSetupStore(context: PiniaPluginContext) {
  const setupSyntaxIds = Object.values(SetupStoreId) as string[];

  if (setupSyntaxIds.includes(context.store.$id)) {
    const { $state } = context.store;

    const defaultStore = jsonClone($state);

    context.store.$reset = () => {
      context.store.$patch(defaultStore);
    };
  }
}
