<template>
  <v-select
    :model-value="modelValue"
    @update:model-value="updateValue"
    :items="items"
    :item-title="displayExpr"
    :item-value="valueExpr"
    :label="label"
    :loading="loading"
    :clearable="clearable"
    :rules="rules"
    :readonly="readonly"
    variant="outlined"
    density="compact"
  ></v-select>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue';

// Define Props
interface LookupProps {
  dataSource: any[] | any; // Can be an array or a Pinia store
  modelValue: string | number | null; // Current selected ID
  displayExpr?: string; // Field name to display (e.g., "name")
  valueExpr?: string; // Field name for the value (e.g., "id")
  label?: string; // Label for the select component
  clearable?: boolean; // Whether the select can be cleared
  rules?: any[]; // Validation rules
  readonly?: boolean; // Whether the select is read-only
}

const props = withDefaults(defineProps<LookupProps>(), {
  displayExpr: 'name',
  valueExpr: 'id',
  label: '',
  clearable: false,
  readonly: false,
  rules: () => [],
});

// Define Emits
const emit = defineEmits(['update:modelValue']);

// Internal state for items and loading
const items = ref<any[]>([]);
const loading = ref(false);

// Determine if dataSource is a Pinia store
const isStore = computed(() => {
  return (
    props.dataSource &&
    typeof props.dataSource === 'object' &&
    'id' in props.dataSource &&
    'state' in props.dataSource
  );
});

// Function to fetch data from store
const fetchDataFromStore = async () => {
  if (!isStore.value) return;

  const store = props.dataSource;
  // Prioritize getting from state
  if (store.families && store.families.length > 0) {
    // Assuming 'families' is the list state
    items.value = store.families;
    return;
  }

  // If state is empty, call fetch function if available
  if (typeof store._loadFamilies === 'function') {
    // Assuming '_loadFamilies' is the fetch function
    loading.value = true;
    try {
      await store._loadFamilies();
      items.value = store.families;
    } catch (error) {
      console.error('Error fetching data from store:', error);
    } finally {
      loading.value = false;
    }
  }
};

// Watch for changes in dataSource
watch(
  () => props.dataSource,
  (newDataSource) => {
    if (Array.isArray(newDataSource)) {
      items.value = newDataSource;
    } else if (isStore.value) {
      fetchDataFromStore();
    }
  },
  { immediate: true, deep: true },
);

// Update v-model
const updateValue = (newValue: string | number | null) => {
  emit('update:modelValue', newValue);
};

// Initial fetch on mounted if dataSource is a store
onMounted(() => {
  if (isStore.value) {
    fetchDataFromStore();
  }
});
</script>

<style scoped>
/* Add any specific styles for Lookup.vue here */
</style>
