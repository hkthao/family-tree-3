<template>
  <v-autocomplete v-bind="$attrs" v-model="internalSelectedItems" @update:model-value="handleUpdateModelValue"
    :items="items" :item-title="itemTitle" :item-value="itemValue" :label="label" :rules="rules" :readonly="readOnly"
    :clearable="clearable" :loading="loading || internalLoading" :search="searchQuery" @update:search="onSearchChange"
    :multiple="multiple" :chips="chips" :closable-chips="closableChips" :return-object="true" >
    <template v-if="$slots.chip" #chip="scope">
      <slot name="chip" v-bind="scope"></slot>
    </template>
    <template v-if="$slots.item" #item="scope">
      <slot name="item" v-bind="scope"></slot>
    </template>
  </v-autocomplete>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { debounce } from 'lodash';

interface RemoteAutocompleteProps {
  modelValue: any | any[];
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean;
  itemTitle?: string;
  itemValue?: string;
  searchFunction: (query: string) => Promise<any[]>;
  preloadFunction: (ids: any | any[]) => Promise<any[]>;
  clearItemsFunction: () => void;
  chips?: boolean;
  closableChips?: boolean;
  returnObject?: boolean;
  loading?: boolean; // Add loading prop
  disabled?: boolean; // Add disabled prop
}

const props = withDefaults(defineProps<RemoteAutocompleteProps>(), {
  itemTitle: 'title',
  itemValue: 'value',
  chips: false, // Default to false, chips should only be true when multiple is true
  closableChips: true,
  returnObject: true,
  loading: false, // Default value for the prop
});

const emit = defineEmits(['update:modelValue']);

const searchQuery = ref('');
const internalLoading = ref(false);
const items = ref<any[]>([]);
const internalSelectedItems = ref<any | any[]>(props.multiple ? [] : null);

const loadItems = async (query: string) => {
  internalLoading.value = true;
  try {
    items.value = await props.searchFunction(query);
  } catch (error) {
    console.error('Error loading items:', error);
  } finally {
    internalLoading.value = false;
  }
};

const debouncedLoadItems = debounce(loadItems, 300);

const onSearchChange = (query: string) => {
  searchQuery.value = query;

  // Trong chế độ chọn đơn, nếu một mục đã được chọn và truy vấn khớp với tiêu đề của nó,
  // điều đó có nghĩa là người dùng chưa nhập bất kỳ thứ gì mới, vì vậy chúng ta không nên lọc lại.
  if (!props.multiple && internalSelectedItems.value && query === internalSelectedItems.value[props.itemTitle]) {
    // Không kích hoạt tìm kiếm mới, vì truy vấn chỉ phản ánh mục đã chọn.
    // Mảng `items` phải chứa các mục đã tải trước hoặc được điền bởi `loadItems('')` khi mount.
    return;
  }

  if (query) {
    debouncedLoadItems(query);
  } else {
    // Nếu truy vấn trống, tải tất cả các mục (hoặc một tập hợp mặc định)
    loadItems('');
  }
};

const handleUpdateModelValue = (newValues: any | any[] | null) => {
  internalSelectedItems.value = newValues;
  emit('update:modelValue', newValues);
};

const preloadSelectedItems = async (value: any | any[]) => {
  if (!value || (Array.isArray(value) && value.length === 0)) {
    internalSelectedItems.value = props.multiple ? [] : null;
    return;
  }

  internalLoading.value = true;
  try {

    const idsToPreload = Array.isArray(value) ? value : [value];
    const preloaded = await props.preloadFunction(idsToPreload);
    internalSelectedItems.value = props.multiple ? preloaded : preloaded[0] || null;
    
    // Ensure preloaded items are also in the general items list if not already present
    preloaded.forEach((item: any) => {
      if (!items.value.some(existingItem => existingItem[props.itemValue] === item[props.itemValue])) {
        items.value.push(item);
      }
    });
  } catch (error) {
    console.error('Error preloading selected items:', error);
  } finally {
    internalLoading.value = false;
  }
};

watch(() => props.modelValue, (newVal) => {
  preloadSelectedItems(newVal);
}, { immediate: true });

onMounted(() => {
  // Always load initial items on mount
  loadItems('');
});
</script>
