<script setup lang="ts">
import { computed } from 'vue';
import type { WatermarkProps } from 'element-plus';
import { useAppStore } from './store/modules/app';
import { useThemeStore } from './store/modules/theme';
import { useAuthStore } from './store/modules/auth';
import { UILocales } from './locales/ui';

defineOptions({ name: 'App' });

// 应用状态存储 - 全局应用配置和状态管理
const appStore = useAppStore();
// 主题状态存储 - 主题样式和水印配置管理
const themeStore = useThemeStore();
// 认证状态存储 - 用户认证信息和权限管理
const authStore = useAuthStore();

// 本地化语言配置 - 根据应用语言设置返回对应的UI组件国际化配置
const locale = computed(() => {
  return UILocales[appStore.locale];
});

// 水印属性配置 - 动态计算水印显示内容和样式属性
const watermarkProps = computed<WatermarkProps>(() => {
  // 水印内容 - 根据配置显示用户名或自定义文本
  const content =
    themeStore.watermark.enableUserName && authStore.userInfo.userName
      ? authStore.userInfo.userName
      : themeStore.watermark.text;

  return {
    content: themeStore.watermark.visible ? content : '', // 显示内容 - 根据可见性设置水印文本
    cross: true, // 交叉显示 - 启用水印交叉排列模式
    fontSize: 16, // 字体大小 - 水印文字字号
    lineHeight: 16, // 行高 - 水印文字行高
    gap: [100, 120], // 间距 - 水印之间的水平和垂直间距
    rotate: -15, // 旋转角度 - 水印倾斜角度
    zIndex: 9999 // 层级 - 水印显示层级，确保在最顶层
  };
});
</script>

<template>
  <ElConfigProvider :locale="locale">
    <AppProvider>
      <ElWatermark class="h-full" v-bind="watermarkProps">
        <RouterView class="bg-layout" />
      </ElWatermark>
    </AppProvider>
  </ElConfigProvider>
</template>

<style scoped></style>
