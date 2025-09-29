<template>
  <v-autocomplete
    v-model="internalSelectedItems"
    @update:model-value="handleAutocompleteUpdate"
    :items="families"
    item-title="name"
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
        :text="item.raw.name"
      ></v-chip>
    </template>
    <template #item="{ props, item }">
      <v-list-item v-bind="props" :subtitle="item.raw.address">
        <template #prepend>
          <v-avatar v-if="item.raw.avatarUrl" :image="item.raw.avatarUrl" size="small"></v-avatar>
        </template>
      </v-list-item>
    </template>
  </v-autocomplete>
</template>

<script setup lang="ts">
import { ref, watch, computed, onMounted } from 'vue';
import { useFamilyStore } from '@/stores/family.store';

// A more specific type for the function being debounced
type DebounceableFunction = (...args: any[]) => void;

const debounce = (func: DebounceableFunction, delay: number) => {
  let timeout: ReturnType<typeof setTimeout>;
  return (...args: any[]) => {
    clearTimeout(timeout);
    timeout = setTimeout(() => func(...args), delay);
  };
};

interface FamilyAutocompleteProps {
  modelValue: (string | number)[] | string | number | undefined;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean; // New prop for multiple selection
}

const { modelValue, label, rules, readOnly, clearable, multiple } = defineProps<FamilyAutocompleteProps>();

const emit = defineEmits(['update:modelValue']);

const familyStore = useFamilyStore();
const loading = ref(false);
const searchTerm = ref('');
const internalSelectedItems = ref<any[]>([]); // For return-object handling

const families = computed(() => familyStore.items);

const fetchFamilies = async (query: string = '') => {
  loading.value = true;
  try {
    await familyStore.searchLookup({ searchQuery: query }, 1, 100); // Fetch first 100 items for autocomplete
  } catch (error) {
    console.error('Error fetching families:', error);
  } finally {
    loading.value = false;
  }
};

const debouncedFetchFamilies = debounce(fetchFamilies, 300);

const onSearchInput = (query: string) => {
  searchTerm.value = query;
  debouncedFetchFamilies(query);
};

const handleAutocompleteUpdate = (newValues: any | any[]) => {
  if (multiple) {
    internalSelectedItems.value = newValues; // newValues is an array of objects
    emit('update:modelValue', newValues.map((item: any) => item.id)); // Emit array of IDs
  } else {
    internalSelectedItems.value = newValues ? [newValues] : []; // newValues is a single object
    emit('update:modelValue', newValues ? newValues.id : undefined); // Emit single ID
  }
};

// Preload selected family based on modelValue (IDs)
watch(
  () => modelValue,
  async (newModelValue) => {
    internalSelectedItems.value = []; // Clear current selections

    let idsToFetch: string[] = [];

    if (multiple) {
      if (Array.isArray(newModelValue) && newModelValue.length > 0) {
        idsToFetch = newModelValue.map(id => String(id));
      }
    } else { // Single selection
      if (newModelValue !== undefined && newModelValue !== null) {
        idsToFetch = [String(newModelValue)];
      }
    }

    if (idsToFetch.length > 0 && typeof familyStore.getManyItemsByIds === 'function') {
      loading.value = true;
      try {
        const fetchedItems = await familyStore.getManyItemsByIds(idsToFetch);
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
  fetchFamilies();
});
</script>
