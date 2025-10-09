import { locale } from 'dayjs';
import 'dayjs/locale/zh-cn';
import 'dayjs/locale/en';
import { localStg } from '@/utils/storage';

/**
 * 设置 dayjs 的本地化语言。
 *
 * 根据传入的语言类型（默认为 'zh-CN'），将 dayjs 的语言环境切换为对应的本地化设置。
 * 如果未传入语言类型，则优先从本地存储获取语言设置，若仍未获取则默认为中文。
 *
 * @param lang 语言类型，支持 'zh-CN' 和 'en-US'，默认为 'zh-CN'
 */
export function setDayjsLocale(lang: App.I18n.LangType = 'zh-CN') {
  const localMap = {
    'zh-CN': 'zh-cn',
    'en-US': 'en'
  } satisfies Record<App.I18n.LangType, string>;

  const l = lang || localStg.get('lang') || 'zh-CN';

  locale(localMap[l]);
}
