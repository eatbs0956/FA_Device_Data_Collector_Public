import type { App } from 'vue';
import { createI18n } from 'vue-i18n';
import { localStg } from '@/utils/storage';
import messages from './locale';

/**
 * 创建并配置 i18n 实例，用于应用的国际化支持。
 *
 * - `locale`: 当前语言环境，优先从本地存储获取，否则默认为 'zh-CN'。
 * - `fallbackLocale`: 当找不到对应语言时的回退语言，设为 'en'。
 * - `messages`: 包含所有语言的翻译内容。
 * - `legacy`: 是否启用旧版 API，这里设为 false，使用 Composition API。
 *
 * @remarks
 * 该实例用于 Vue 应用的多语言切换和文本本地化。
 */
const i18n = createI18n({
  locale: localStg.get('lang') || 'zh-CN',
  fallbackLocale: 'en',
  messages,
  legacy: false
});

/**
 * 设置应用的国际化插件。
 *
 * 此函数将 `i18n` 插件注册到传入的 Vue 应用实例上，实现多语言支持。
 *
 * @param app - Vue 应用实例。
 */
export function setupI18n(app: App) {
  app.use(i18n);
}

/**
 * 返回本地化字符串的函数。
 *
 * 使用指定的键和可选参数，从 i18n 配置中获取对应的本地化文本。
 *
 * @param key 本地化字符串的键
 * @param args 可选参数，用于字符串插值
 * @returns 本地化后的字符串
 */
export const $t = i18n.global.t as App.I18n.$T;

/**
 * 设置应用的当前语言环境。
 *
 * @param locale - 要设置的语言类型，属于 `App.I18n.LangType`。
 */
export function setLocale(locale: App.I18n.LangType) {
  i18n.global.locale.value = locale;
}
