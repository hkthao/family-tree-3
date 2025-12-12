<script setup lang="ts">
import { ref, watch, computed, type PropType } from 'vue';
import { VAutocomplete } from 'vuetify/components';
import { useI18n } from 'vue-i18n';

interface Item {
  [key: string]: any;
}

const props = defineProps({
  modelValue: {
    type: [String, Number, Object] as PropType<any>,
    default: null,
  },
  label: {
    type: String,
    default: '',
  },
  fetchItems: {
    type: Function as PropType<(query: string) => Promise<Item[]>>,
    required: true,
  },
  itemTitle: {
    type: [String, Function] as PropType<string | ((item: Item) => string)>,
    default: 'title',
  },
  itemValue: {
    type: [String, Function] as PropType<string | ((item: Item) => any)>,
    default: 'id',
  },
  debounceTime: {
    type: Number,
    default: 300,
  },
  returnObject: {
    type: Boolean,
    default: false,
  },
  clearable: {
    type: Boolean,
    default: true,
  },
  variant: {
    type: String as PropType<'solo' | 'solo-filled' | 'filled' | 'outlined' | 'underlined' | 'plain'>,
    default: 'outlined',
  },
  density: {
    type: String as PropType<'default' | 'comfortable' | 'compact'>,
    default: 'comfortable',
  },
  noDataText: {
    type: String,
    default: '',
  },
});

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();

const loading = ref(false);
const remoteItems = ref<Item[]>([]);
const searchText = ref('');
let debounceTimer: ReturnType<typeof setTimeout> | null = null;
const hasBeenFocused = ref(false); // Track if the input has been focused

const handleFocus = () => {
  // If not focused before and search text is empty, trigger initial fetch
  if (!hasBeenFocused.value && !searchText.value) {
    fetchRemoteItems('');
    hasBeenFocused.value = true;
  }
};

const internalNoDataText = computed(() => t('common.no_data'));

const getItemTitle = (item: Item): string => {
  if (typeof props.itemTitle === 'function') {
    return props.itemTitle(item);
  }
  return item[props.itemTitle] !== undefined ? String(item[props.itemTitle]) : '';
};

const getItemValue = (item: Item): any => {
  if (typeof props.itemValue === 'function') {
    return props.itemValue(item);
  }
  return item[props.itemValue];
};

const fetchRemoteItems = async (query: string) => {
  if (!query) {
    remoteItems.value = [];
    return;
  }
  loading.value = true;
  try {
    const result = await props.fetchItems(query);
    remoteItems.value = result.map(item => ({
      ...item,
      title: getItemTitle(item),
      value: getItemValue(item),
    }));
  } catch (error) {
    console.error('Error fetching remote items:', error);
    remoteItems.value = [];
  } finally {
    loading.value = false;
  }
};

watch(
  searchText,
  (newSearchText) => {
    if (debounceTimer) {
      clearTimeout(debounceTimer);
    }
    debounceTimer = setTimeout(() => {
      fetchRemoteItems(newSearchText);
    }, props.debounceTime);
  },
  { immediate: false }
);

watch(
  () => props.modelValue,
  (newValue) => {
    // When modelValue changes externally, we might need to update searchText
    // to reflect the selected item's title if returnObject is true and an object is passed
    if (newValue && props.returnObject && typeof newValue === 'object') {
      searchText.value = getItemTitle(newValue);
    } else if (!newValue) {
      // If modelValue becomes null/undefined, clear search text
      searchText.value = '';
    }
  },
  { immediate: true }
);

const updateModelValue = (newValue: any) => {
  if (props.returnObject) {
    // If returning object, newValue is already the full object.
    emit('update:modelValue', newValue);
  } else {
    // If not returning object, newValue is the item's value.
    // Find the original item from remoteItems to ensure we emit the correct value.
    const selectedItem = remoteItems.value.find(item => getItemValue(item) === newValue);
    emit('update:modelValue', selectedItem ? getItemValue(selectedItem) : null);
  }

  // After selecting, update searchText to show the selected item's title
  if (newValue) {
    const selectedItem = remoteItems.value.find(item => getItemValue(item) === (props.returnObject ? getItemValue(newValue) : newValue));
    if (selectedItem) {
      searchText.value = getItemTitle(selectedItem);
    }
  } else {
    searchText.value = ''; // Clear search text if selection is cleared
  }
};

</script>

<template>
  <v-autocomplete
    :model-value="modelValue"
    @update:modelValue="updateModelValue"
    v-model:search="searchText"
    :items="remoteItems"
    :loading="loading"
    :label="label"
    :item-title="itemTitle"
    :item-value="itemValue"
    :return-object="returnObject"
    :clearable="clearable"
    :variant="variant"
    :density="density"
    :no-data-text="internalNoDataText"
    no-filter
    @focus="handleFocus"
    v-bind="$attrs"
  >
    <template v-for="(_, name) in $slots" #[name]="slotProps">
      <slot :name="name" v-bind="slotProps || {}" />
    </template>
  </v-autocomplete>
</template>
