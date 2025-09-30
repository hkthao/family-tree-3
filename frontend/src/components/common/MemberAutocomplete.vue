<template>
  <v-autocomplete
    v-model="internalSelectedItems"
    @update:model-value="handleAutocompleteUpdate"
    :items="items"
    item-title="fullName"
    item-value="id"
    :label="label"
    :rules="rules"
    :readonly="readOnly"
    :clearable="clearable"
    :loading="loading"
    :search="searchTerm"
    @update:search="onSearchInput"
    :multiple="multiple"
    :chips="multiple"
    :closable-chips="multiple"
    return-object
  >
    <template #chip="{ props, item }">
      <v-chip
        v-bind="props"
        size="small"
        v-if="item.raw"
        :prepend-avatar="item.raw.avatarUrl ? item.raw.avatarUrl : undefined"
        :text="item.raw.fullName"
      ></v-chip>
    </template>
    <template #item="{ props, item }">
      <v-list-item v-bind="props" :subtitle="item.raw?.birthDeathYears">
        <template #prepend>
          <v-avatar v-if="item.raw?.avatarUrl" :image="item.raw.avatarUrl" size="small"></v-avatar>
        </template>
      </v-list-item>
    </template>
  </v-autocomplete>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useMemberStore } from '@/stores/member.store'; // Import memberStore directly

// A more specific type for the function being debounced
type DebounceableFunction = (...args: any[]) => void;

const debounce = (func: DebounceableFunction, delay: number) => {
  let timeout: ReturnType<typeof setTimeout>;
  return (...args: any[]) => {
    clearTimeout(timeout);
    timeout = setTimeout(() => func(...args), delay);
  };
};

interface MemberAutocompleteProps {
  modelValue: (string | number)[] | string | number | undefined | null;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  additionalFilters?: any;
  multiple?: boolean; // New prop for multiple selection
}

const props = withDefaults(defineProps<MemberAutocompleteProps>(), {
  label: undefined,
  rules: () => [],
  readOnly: false,
  clearable: false,
  additionalFilters: () => ({}),
  multiple: false, // Default to single selection
});

const emit = defineEmits(['update:modelValue']);

const memberStore = useMemberStore(); // Use memberStore directly

const loading = ref(false);
const items = ref<any[]>([]);
const searchTerm = ref('');

// Internal state for selected items to handle `return-object`
const internalSelectedItems = ref<any[]>([]);

const fetchItems = async (query: string = '') => {
  if (typeof memberStore.searchLookup !== 'function') {
    console.warn('memberStore is missing searchLookup method.');
    return;
  }

  loading.value = true;
  try {
    const filter = {
      searchQuery: query,
      ...props.additionalFilters,
    };
    // Assuming searchLookup handles pagination internally or returns all matching for autocomplete
    await memberStore.searchLookup(filter, 1, 100); // Fetch first 100 items for autocomplete
    items.value = memberStore.items; // Assuming searchLookup updates memberStore.items
  } catch (error) {
    console.error('Error fetching items:', error);
  }
  finally {
    loading.value = false;
  }
};

const debouncedFetchItems = debounce(fetchItems, 300);

const onSearchInput = (query: string) => {
  searchTerm.value = query;
  debouncedFetchItems(query);
};

const handleAutocompleteUpdate = (newValues: any | any[]) => {
  if (props.multiple) {
    internalSelectedItems.value = newValues; // newValues is an array of objects
    emit('update:modelValue', newValues.map((item: any) => item.id)); // Emit array of IDs
  } else {
    internalSelectedItems.value = newValues ? [newValues] : []; // newValues is a single object
    emit('update:modelValue', newValues ? newValues.id : undefined); // Emit single ID
  }
};

// Preload selected items based on modelValue (IDs)
watch(
  () => props.modelValue,
  async (newModelValue) => {
    internalSelectedItems.value = []; // Clear current selections

    let idsToFetch: string[] = [];

    if (props.multiple) {
      if (Array.isArray(newModelValue) && newModelValue.length > 0) {
        idsToFetch = newModelValue.map(id => String(id));
      }
    } else { // Single selection
      if (newModelValue !== undefined && newModelValue !== null) {
        idsToFetch = [String(newModelValue)];
      }
    }

    if (idsToFetch.length > 0 && typeof memberStore.getManyItemsByIds === 'function') {
      loading.value = true;
      try {
        const fetchedItems = await memberStore.getManyItemsByIds(idsToFetch);
        internalSelectedItems.value = fetchedItems;
      } catch (error) {
        console.error('Error preloading selected items:', error);
      } finally {
        loading.value = false;
      }
    }
  },
  { immediate: true, deep: true }
);

// Initial fetch for empty search to show some options
onMounted(() => {
  fetchItems();
});

// Watch for changes in additionalFilters and reload items
watch(
  () => props.additionalFilters,
  () => {
    fetchItems(searchTerm.value);
  },
  { deep: true }
);
</script>
