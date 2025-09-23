<template>
  <v-chip v-if="displayValue" :color="chipColor" label size="small">
    {{ displayValue }}
  </v-chip>
  <span v-else-if="loading">Loading...</span>
  <span v-else>N/A</span>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';

// Define Props
interface ChipLookupProps {
  dataSource: any[] | any; // Can be an array or a Pinia store
  value: string | number | null; // Current ID to look up
  displayExpr?: string; // Field name to display (e.g., "name")
  valueExpr?: string; // Field name for the value (e.g., "id")
  color?: string; // Color for the chip
}

const props = withDefaults(defineProps<ChipLookupProps>(), {
  displayExpr: 'name',
  valueExpr: 'id',
  color: 'primary',
});

import { useFamilyStore } from '@/stores/family.store'; // Assuming family store is used

const familyStore = useFamilyStore();
const displayValue = ref<string | null>(null);
const loading = ref(false);

watch(
  () => props.value,
  async (newValue) => {
    if (newValue) {
      loading.value = true;
      const item = await familyStore.fetchItemById(newValue as string);
      displayValue.value = item ? item[props.displayExpr] : 'N/A';
      loading.value = false;
    } else {
      displayValue.value = null;
    }
  },
  { immediate: true },
);

// Computed property for chip color
const chipColor = computed(() => {
  return props.color;
});
</script>

<style scoped>
/* Add any specific styles for ChipLookup.vue here */
</style>
