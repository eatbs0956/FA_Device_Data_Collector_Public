import { $t } from '@/locales';

/**
 * 记录转选项 - 将记录对象转换为选项数组格式
 *
 * 将键值对对象转换为包含value和label的选项数组，常用于下拉框、单选框等组件
 *
 * @example
 *   ```ts
 *   const record = {
 *     key1: 'label1',
 *     key2: 'label2'
 *   };
 *   const options = transformRecordToOption(record);
 *   // [
 *   //   { value: 'key1', label: 'label1' },
 *   //   { value: 'key2', label: 'label2' }
 *   // ]
 *   ```;
 *
 * @param record 记录对象 - 要转换的键值对对象
 */
export function transformRecordToOption<T extends Record<string, string>>(record: T) {
  // 遍历记录对象并转换为选项格式
  return Object.entries(record).map(([value, label]) => ({
    value, // 选项值 - 对象的键
    label // 选项标签 - 对象的值
  })) as CommonType.Option<keyof T, T[keyof T]>[];
}

/**
 * 翻译选项 - 将包含国际化键的选项数组转换为已翻译的选项数组
 *
 * @param options 选项数组 - 包含国际化标签键的选项列表
 */
export function translateOptions(options: CommonType.Option<string, App.I18n.I18nKey>[]) {
  // 遍历选项并翻译标签
  return options.map(option => ({
    ...option, // 保留原有属性
    label: $t(option.label) // 翻译标签文本
  }));
}

/**
 * 切换HTML类名 - 提供添加和移除HTML根元素类名的功能
 *
 * @param className 类名 - 要操作的CSS类名
 */
export function toggleHtmlClass(className: string) {
  /**
   * 添加类名 - 向HTML根元素添加指定类名
   */
  function add() {
    document.documentElement.classList.add(className);
  }

  /**
   * 移除类名 - 从HTML根元素移除指定类名
   */
  function remove() {
    document.documentElement.classList.remove(className);
  }

  return {
    add, // 添加类名方法
    remove // 移除类名方法
  };
}
