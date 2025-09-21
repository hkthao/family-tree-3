<template>
  <v-autocomplete
    :model-value="modelValue"
    @update:model-value="updateModelValue('update:modelValue', $event)"
    :items="families"
    item-title="name"
    item-value="id"
    :label="label"
    :rules="rules"
    :readonly="readOnly"
    :clearable="clearable"
    :custom-filter="familyFilter"
  >
    <template #item="{ item }">
      <v-list-item :title="item.raw.name" :subtitle="item.raw.address"></v-list-item>
    </template>
  </v-autocomplete>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useFamilies } from '@/data/families';
import type { Family } from '@/types/family';

const { modelValue, label, rules, readOnly, clearable } = defineProps<{
  modelValue: string | number | null | undefined; // The selected family ID
  label?: string;
  rules?: Array<(value: unknown) => boolean | string>;
  readOnly?: boolean;
  clearable?: boolean;
}>();

const updateModelValue = defineEmits(['update:modelValue']);

const { getFamilies } = useFamilies();
const families = ref<Family[]>([]);

interface VuetifyInternalItem {
  raw: Family;
}

onMounted(async () => {
  const { families: fetchedFamilies } = await getFamilies('', 'All', 1, -1); // Fetch all families
  families.value = fetchedFamilies;
});

const familyFilter = (_value: string, query: string, item: VuetifyInternalItem | undefined) => {
  if (!item || !item.raw) return false;

  const rawItem = item.raw;

  const name = rawItem.name ? String(rawItem.name).toLowerCase() : '';
  const address = rawItem.address ? String(rawItem.address).toLowerCase() : '';
  const id = String(rawItem.id);
  const searchText = query ? String(query).toLowerCase() : '';

  return name.includes(searchText) || address.includes(searchText) || id.includes(searchText);
};
</script>
