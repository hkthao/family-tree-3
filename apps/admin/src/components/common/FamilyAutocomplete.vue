<template>
  <RemoteAutocomplete v-bind="$attrs" :model-value="modelValue" @update:model-value="handleRemoteAutocompleteUpdate"
    :label="label" :rules="rules" :read-only="readOnly" :clearable="clearable" :multiple="false" item-title="name"
    item-value="id" :search-function="searchFunction" :preload-function="getByIdsFunction"
    :clear-items-function="clearItemsFunction" :loading="composableLoading" :items="items" :disabled="disabled">
    <template #item="{ props, item }">
      <v-list-item v-bind="props" :subtitle="item.raw.address">
        <template #prepend>
          <v-avatar :image="getFamilyAvatarUrl(item.raw.avatarUrl)" size="small"></v-avatar>
        </template>
      </v-list-item>
    </template>
  </RemoteAutocomplete>
</template>

<script setup lang="ts">
import type { Family } from '@/types';
import RemoteAutocomplete from './RemoteAutocomplete.vue';
import { useFamilyAutocomplete } from '@/composables';
import { getFamilyAvatarUrl } from '@/utils/avatar.utils'; // NEW

interface FamilyAutocompleteProps {
  modelValue: string | string[] | undefined | null;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean;
  disabled?: boolean;
}

const props = defineProps<FamilyAutocompleteProps>();

const emit = defineEmits(['update:modelValue']);

const { items, selectedItems, onSearchChange, preloadById, loading: composableLoading } = useFamilyAutocomplete({
  multiple: props.multiple,
  initialValue: props.modelValue ?? undefined,
});

const searchFunction = async (query: string): Promise<Family[]> => {
  onSearchChange(query);
  return items.value;
};

const getByIdsFunction = async (ids: string[]): Promise<Family[]> => {
  await preloadById(ids);
  return selectedItems.value;
};

const clearItemsFunction = () => {
  items.value = [];
  selectedItems.value = [];
};

const handleRemoteAutocompleteUpdate = (newValues: Family | Family[] | null) => {
  if (props.multiple) {
    const ids = Array.isArray(newValues) ? newValues.map((item: Family) => item.id) : [];
    emit('update:modelValue', ids);
  } else {
    const id = newValues ? (newValues as Family).id : undefined;
    emit('update:modelValue', id);
  }
};
</script>