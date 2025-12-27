<script setup lang="ts">
import { reactive, watch } from 'vue';
import { s7AreaOptions } from '@/constants/business';
import { $t } from '@/locales';

defineOptions({
  name: 'S7AddressInput'
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
  area: 'DB',
  dbNumber: 1,
  offset: 0,
  bitOffset: 0
});

// 初始化
watch(
  () => props.modelValue,
  val => {
    if (val) {
      config.area = val.area || 'DB';
      config.dbNumber = val.dbNumber || 1;
      config.offset = val.offset || 0;
      config.bitOffset = val.bitOffset || 0;
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
    <ElFormItem :label="$t('page.tag.s7.area')">
      <ElSelect v-model="config.area" class="w-full" @change="handleChange">
        <ElOption v-for="option in s7AreaOptions" :key="option.value" :label="option.label" :value="option.value" />
      </ElSelect>
    </ElFormItem>

    <ElFormItem v-if="config.area === 'DB'" :label="$t('page.tag.s7.dbNumber')">
      <ElInputNumber v-model="config.dbNumber" :min="1" :controls="false" class="w-full" @change="handleChange" />
    </ElFormItem>

    <ElFormItem :label="$t('page.tag.s7.offset')">
      <ElInputNumber v-model="config.offset" :min="0" :controls="false" class="w-full" @change="handleChange" />
    </ElFormItem>

    <ElFormItem :label="$t('page.tag.s7.bitOffset')">
      <ElInputNumber
        v-model="config.bitOffset"
        :min="0"
        :max="7"
        :controls="false"
        class="w-full"
        @change="handleChange"
      />
    </ElFormItem>
  </div>
</template>
