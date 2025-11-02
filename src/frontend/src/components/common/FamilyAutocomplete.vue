<template>
  <RemoteAutocomplete
    v-bind="$attrs"
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    :label="label"
    :rules="rules"
    :read-only="readOnly"
    :clearable="clearable"
    :multiple="multiple"
    item-title="name"
    item-value="id"
    :search-function="searchFamiliesFunction"
    :preload-function="getFamiliesByIdsFunction"
    :clear-items-function="clearItemsFunction"
    :loading="composableLoading"
    :items="items"
  >
    <template #chip="{ props, item }">
      <v-chip v-bind="props" size="small" v-if="item.raw"
        :prepend-avatar="item.raw.avatarUrl ? item.raw.avatarUrl : undefined" :text="item.raw.name"></v-chip>
    </template>
    <template #item="{ props, item }">
      <v-list-item v-bind="props" :subtitle="item.raw.address">
        <template #prepend>
          <v-avatar v-if="item.raw.avatarUrl" :image="item.raw.avatarUrl" size="small"></v-avatar>
        </template>
      </v-list-item>
    </template>
  </RemoteAutocomplete>
</template>

<script setup lang="ts">
import type { Family } from '@/types';
import RemoteAutocomplete from './RemoteAutocomplete.vue';
import { useFamilyAutocomplete } from '@/composables/useFamilyAutocomplete';

interface FamilyAutocompleteProps {
  modelValue: string | string[] | undefined | null;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean;
}

const props = defineProps<FamilyAutocompleteProps>();

const emit = defineEmits(['update:modelValue']);

const { items, selectedItems, onSearchChange, preloadById, loading: composableLoading } = useFamilyAutocomplete({
  multiple: props.multiple,
  initialValue: props.modelValue ?? undefined,
});

const searchFamiliesFunction = async (query: string): Promise<Family[]> => {
  onSearchChange(query);
  return items.value;
};

const getFamiliesByIdsFunction = async (ids: string[]): Promise<Family[]> => {
  await preloadById(ids);
  return selectedItems.value;
};

const clearItemsFunction = () => {
  items.value = [];
  selectedItems.value = [];
};
</script>