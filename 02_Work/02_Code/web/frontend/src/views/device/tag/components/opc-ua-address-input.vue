<script setup lang="ts">
import { reactive, watch } from 'vue';
import { $t } from '@/locales';

defineOptions({
  name: 'OpcUaAddressInput'
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
  nodeId: '',
  namespaceIndex: 2
});

// 初始化
watch(
  () => props.modelValue,
  val => {
    if (val) {
      config.nodeId = val.nodeId || '';
      config.namespaceIndex = val.namespaceIndex || 2;
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
    <ElFormItem :label="$t('page.tag.opcua.nodeId')">
      <ElInput v-model="config.nodeId" :placeholder="$t('page.tag.opcua.nodeIdPlaceholder')" @input="handleChange" />
    </ElFormItem>

    <ElFormItem :label="$t('page.tag.opcua.namespaceIndex')">
      <ElInputNumber v-model="config.namespaceIndex" :min="0" :controls="false" class="w-full" @change="handleChange" />
    </ElFormItem>
  </div>
</template>
