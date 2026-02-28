<script setup lang="ts">
import { reactive, watch } from 'vue';
import { modbusFunctionCodeOptions } from '@/constants/business';
import { $t } from '@/locales';

defineOptions({
  name: 'ModbusAddressInput'
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

// 注意：从站ID (slaveId) 已移至设备级别配置，不在标签地址中配置
const config = reactive({
  functionCode: '03',
  address: 0,
  quantity: 1
});

// 初始化
watch(
  () => props.modelValue,
  val => {
    if (val) {
      config.functionCode = val.functionCode || '03';
      config.address = val.address || 0;
      config.quantity = val.quantity || 1;
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
    <ElFormItem :label="$t('page.tag.modbus.functionCode')">
      <ElSelect v-model="config.functionCode" class="w-full" @change="handleChange">
        <ElOption
          v-for="option in modbusFunctionCodeOptions"
          :key="option.value"
          :label="option.label"
          :value="option.value"
        />
      </ElSelect>
    </ElFormItem>

    <ElFormItem :label="$t('page.tag.modbus.address')">
      <ElInputNumber
        v-model="config.address"
        :min="0"
        :max="65535"
        :controls="false"
        class="w-full"
        @change="handleChange"
      />
    </ElFormItem>

    <ElFormItem :label="$t('page.tag.modbus.quantity')">
      <ElInputNumber
        v-model="config.quantity"
        :min="1"
        :max="125"
        :controls="false"
        class="w-full"
        @change="handleChange"
      />
    </ElFormItem>
  </div>
</template>
