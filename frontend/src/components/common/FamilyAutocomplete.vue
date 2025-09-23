<template>
  <v-autocomplete
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    :items="families"
    item-title="name"
    item-value="id"
    :label="label"
    :rules="rules"
    :readonly="readOnly"
    :clearable="clearable"
    :custom-filter="familyFilter"
  >
    <template #item="{ props, item }">
      <v-list-item v-bind="props" :subtitle="item.raw.address"></v-list-item>
    </template>
  </v-autocomplete>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useFamilyStore } from '@/stores/family.store';
import type { Family } from '@/types/family';

 defineProps<{
  modelValue: string | null | undefined; 
  label?: string;
  rules?: Array<any>;
  readOnly?: boolean;
  clearable?: boolean;
}>();

const familyStore = useFamilyStore();
const families = ref<Family[]>([]);

interface VuetifyInternalItem {
  raw: Family;
}

onMounted(async () => {
  await familyStore.searchFamilies('', 'all');
  families.value = familyStore.families;
});

const familyFilter = (_value: string, query: string, item: VuetifyInternalItem | undefined ) => {
  if (!item || !item.raw) return false;

  const rawItem = item.raw;

  const name = rawItem.name ? String(rawItem.name).toLowerCase() : '';
  const address = rawItem.address ? String(rawItem.address).toLowerCase() : '';
  const searchText = query ? String(query).toLowerCase() : '';

  return name.includes(searchText) || address.includes(searchText);
};
</script>
