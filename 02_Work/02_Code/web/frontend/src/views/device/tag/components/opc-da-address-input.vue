<script setup lang="ts">
import { reactive, watch } from 'vue';
import { $t } from '@/locales';

defineOptions({
  name: 'OpcDaAddressInput'
});

interface Props {
  modelValue?: Record<string, any>;
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: () => ({})
});

const emit = defineEmits<{
  'update:modelValue': [value: Record<string, any>];
}>();

const config = reactive({
  itemId: ''
});

// 初始化
watch(
  () => props.modelValue,
  val => {
    if (val) {
      config.itemId = val.itemId || '';
    }
  },
  { immediate: true, deep: true }
);

// 更新
function handleChange() {
  emit('update:modelValue', { ...config });
}
</script>

<template>
  <div class="space-y-3">
    <ElFormItem :label="$t('page.tag.opcda.itemId')">
      <ElInput v-model="config.itemId" :placeholder="$t('page.tag.opcda.itemIdPlaceholder')" @input="handleChange" />
    </ElFormItem>
  </div>
</template>
