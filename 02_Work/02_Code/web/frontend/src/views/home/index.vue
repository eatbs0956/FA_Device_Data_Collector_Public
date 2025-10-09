<script setup lang="ts">
// 头部横幅组件 - 首页顶部展示的横幅组件
import HeaderBanner from './modules/header-banner.vue';
</script>

<template>
  <ElSpace direction="vertical" fill class="full-space pb-0" :size="0">
    <HeaderBanner class="mb-16px" />
  </ElSpace>
</template>

<style scoped lang="scss">
.full-space {
  > :deep(.el-space__item) {
    width: 100%;
  }
}
</style>
