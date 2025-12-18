<script setup lang="ts">
import { watch, type PropType } from 'vue';
import { VAutocomplete } from 'vuetify/components';
import { useRemoteAutocomplete } from '@/composables/ui/useRemoteAutocomplete';

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
  multiple: { // Add this prop
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['update:modelValue']);

const {
  loading,
  remoteItems,
  searchText,
  internalNoDataText,
  getItemTitle,
  getItemValue,
  handleFocus,
} = useRemoteAutocomplete({
  fetchItems: props.fetchItems,
  itemTitle: props.itemTitle,
  itemValue: props.itemValue,
  debounceTime: props.debounceTime,
});


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
    emit('update:modelValue', newValue);
  } else {
    // If not returning object, newValue is the item's value.
    // Find the original item from remoteItems to ensure we emit the correct value.
    const selectedItem = remoteItems.value.find(item => getItemValue(item) === newValue);
    emit('update:modelValue', selectedItem ? getItemValue(selectedItem) : null);
  }

  // After selecting, update searchText to show the selected item's title (for single selection)
  // or clear it (for multiple selection)
  if (props.multiple) {
    searchText.value = ''; // Clear search text for multiple selections
  } else if (newValue) {
    const selectedItem = remoteItems.value.find(item => getItemValue(item) === (props.returnObject ? getItemValue(newValue) : newValue));
    if (selectedItem) {
      searchText.value = getItemTitle(selectedItem);
    }
  } else {
    searchText.value = ''; // Clear search text if selection is cleared (single selection)
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
    :hide-selected="props.multiple"
    :chips="props.multiple"
    :multiple="props.multiple"
    no-filter
    @focus="handleFocus"
    v-bind="$attrs"
  >
  </v-autocomplete>
</template>
