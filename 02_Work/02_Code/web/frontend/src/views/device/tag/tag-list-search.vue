<script setup lang="ts">
import { enableStatusOptions } from '@/constants/business';
import { $t } from '@/locales';

defineOptions({
  name: 'TagListSearch'
});

interface Props {
  disabled?: boolean;
}

withDefaults(defineProps<Props>(), {
  disabled: false
});

const model = defineModel<Api.Device.TagSearchParams>('model', { required: true });

const emit = defineEmits<{
  search: [params: Api.Device.TagSearchParams];
  reset: [];
}>();

function handleSearch() {
  emit('search', model.value);
}

function handleReset() {
  emit('reset');
}
</script>

<template>
  <div class="border-b border-gray-200 px-4 py-3 dark:border-gray-700">
    <ElForm :model="model" label-width="80px" inline :disabled="disabled">
      <ElFormItem :label="$t('page.tag.tagName')">
        <ElInput
          v-model="model.tagName"
          :placeholder="$t('page.tag.tagNamePlaceholder')"
          clearable
          class="w-180px"
          @keyup.enter="handleSearch"
        />
      </ElFormItem>
      <ElFormItem :label="$t('common.status')">
        <ElSelect v-model="model.enabled" :placeholder="$t('page.tag.statusPlaceholder')" clearable class="w-120px">
          <ElOption
            v-for="option in enableStatusOptions"
            :key="option.value"
            :label="$t(option.label)"
            :value="option.value === '1'"
          />
        </ElSelect>
      </ElFormItem>
      <ElFormItem>
        <ElButton type="primary" @click="handleSearch">
          <icon-ic-round-search class="mr-1" />
          {{ $t('common.search') }}
        </ElButton>
        <ElButton @click="handleReset">
          <icon-ic-round-refresh class="mr-1" />
          {{ $t('common.reset') }}
        </ElButton>
      </ElFormItem>
    </ElForm>
  </div>
</template>
