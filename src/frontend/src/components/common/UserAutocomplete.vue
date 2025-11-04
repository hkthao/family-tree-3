<template>
  <RemoteAutocomplete v-bind="$attrs" :model-value="modelValue" @update:model-value="handleRemoteAutocompleteUpdate"
    :label="label" :rules="rules" :read-only="readOnly" :clearable="clearable" :multiple="multiple" item-title="name"
    item-value="id" :search-function="searchFunction" :preload-function="getByIdsFunction"
    :clear-items-function="clearItemsFunction" :loading="composableLoading" :items="items">
    <template #item="{ props, item }">
      <v-list-item v-bind="props" :subtitle="item.raw.email">
        <template #prepend>
          <v-avatar v-if="item.raw.avatar" :image="item.raw.avatar" size="small"></v-avatar>
        </template>
      </v-list-item>
    </template>
  </RemoteAutocomplete>
</template>

<script setup lang="ts">
import type { UserProfile } from '@/types';
import RemoteAutocomplete from './RemoteAutocomplete.vue';
import { useUserAutocomplete } from '@/composables/useUserAutocomplete';

interface UserAutocompleteProps {
  modelValue: string | string[] | undefined | null;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean;
}

const props = defineProps<UserAutocompleteProps>();

const emit = defineEmits(['update:modelValue']);

const { items, selectedItems, onSearchChange, preloadById, loading: composableLoading } = useUserAutocomplete({
  multiple: props.multiple,
  initialValue: props.modelValue ?? undefined,
});

const searchFunction = async (query: string): Promise<UserProfile[]> => {
  onSearchChange(query);
  return items.value;
};

const getByIdsFunction = async (ids: string[]): Promise<UserProfile[]> => {
  await preloadById(ids);
  return selectedItems.value;
};

const clearItemsFunction = () => {
  items.value = [];
  selectedItems.value = [];
};

const handleRemoteAutocompleteUpdate = (newValues: UserProfile | UserProfile[] | null) => {
  if (props.multiple) {
    const ids = Array.isArray(newValues) ? newValues.map((item: UserProfile) => item.id) : [];
    emit('update:modelValue', ids);
  } else {
    const id = newValues ? (newValues as UserProfile).id : undefined;
    emit('update:modelValue', id);
  }
};
</script>
