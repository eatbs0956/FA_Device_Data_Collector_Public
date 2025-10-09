// @unocss-include
import { getRgb } from '@sa/color';
import { DARK_CLASS } from '@/constants/app';
import { localStg } from '@/utils/storage';
import { toggleHtmlClass } from '@/utils/common';
import systemLogo from '@/assets/svg-icon/logo.svg?raw';
import { $t } from '@/locales';

/**
 * 设置加载界面 - 在应用启动时显示品牌化的加载动画界面
 */
export function setupLoading() {
  // 主题颜色 - 从本地存储获取用户设置的主题色，默认为蓝色
  const themeColor = localStg.get('themeColor') || '#646cff';

  // 暗黑模式 - 从本地存储获取暗黑模式设置，默认为浅色模式
  const darkMode = localStg.get('darkMode') || false;

  // RGB颜色分量 - 将主题色转换为RGB分量以供CSS变量使用
  const { r, g, b } = getRgb(themeColor);

  // CSS主色变量 - 构建CSS变量字符串，用于设置加载界面的主色调
  const primaryColor = `--primary-color: ${r} ${g} ${b}`;

  // 应用暗黑模式 - 如果用户启用了暗黑模式则添加对应CSS类
  if (darkMode) {
    toggleHtmlClass(DARK_CLASS).add();
  }

  // 加载动画类名数组 - 定义四个加载点的位置和动画延迟
  const loadingClasses = [
    'left-0 top-0', // 左上角 - 无延迟
    'left-0 bottom-0 animate-delay-500', // 左下角 - 延迟500ms
    'right-0 top-0 animate-delay-1000', // 右上角 - 延迟1000ms
    'right-0 bottom-0 animate-delay-1500' // 右下角 - 延迟1500ms
  ];

  // 带样式的Logo - 为系统Logo SVG添加样式类名
  const logoWithClass = systemLogo.replace('<svg', `<svg class="size-128px text-primary"`);

  // 加载点HTML - 生成四个带动画的加载点元素
  const dot = loadingClasses
    .map(item => {
      return `<div class="absolute w-16px h-16px bg-primary rounded-8px animate-pulse ${item}"></div>`;
    })
    .join('\n');

  // 加载界面HTML模板 - 完整的加载界面结构，包含Logo、动画和标题
  const loading = `
  <div class="fixed-center flex-col bg-layout" style="${primaryColor}">
    ${logoWithClass}
    <div class="w-56px h-56px my-36px">
      <div class="relative h-full animate-spin">
        ${dot}
      </div>
    </div>
    <h2 class="text-28px font-500 text-primary">${$t('system.title')}</h2>
  </div>`;

  // 应用容器元素 - 获取应用挂载点DOM元素
  const app = document.getElementById('app');

  // 设置加载界面 - 将生成的加载HTML插入到应用容器中
  if (app) {
    app.innerHTML = loading;
  }
}
