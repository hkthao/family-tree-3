<template>
  <RemoteAutocomplete v-bind="$attrs" :model-value="modelValue" @update:model-value="handleRemoteAutocompleteUpdate"
    :label="label" :rules="rules" :read-only="readOnly" :clearable="clearable" :multiple="multiple"
    item-title="fullName" item-value="id" :search-function="searchFunction" :preload-function="getByIdsFunction"
    :clear-items-function="clearItemsFunction" :loading="composableLoading" :items="items">
    <template #chip="{ props, item }">
      <v-chip v-bind="props" size="small" v-if="item.raw"
        :prepend-avatar="item.raw.avatarUrl ? item.raw.avatarUrl : undefined" :text="item.raw.fullName"></v-chip>
    </template>
    <template #item="{ props, item }">
      <v-list-item v-bind="props" :subtitle="item.raw?.birthDeathYears">
        <template #prepend>
          <v-avatar v-if="item.raw?.avatarUrl" :image="item.raw.avatarUrl" size="small"></v-avatar>
        </template>
      </v-list-item>
    </template>
  </RemoteAutocomplete>
</template>

<script setup lang="ts">
import type { Member } from '@/types';
import RemoteAutocomplete from './RemoteAutocomplete.vue';
import { useMemberAutocomplete } from '@/composables/useMemberAutocomplete';

interface MemberAutocompleteProps {
  modelValue: string | string[] | undefined | null;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean;
  familyId?: string;
  loading?: boolean; // Add loading prop
}

const props = defineProps<MemberAutocompleteProps>();

const emit = defineEmits(['update:modelValue']);

const { items, selectedItems, onSearchChange, preloadById, loading: composableLoading } = useMemberAutocomplete({
  familyId: props.familyId,
  multiple: props.multiple,
  initialValue: props.modelValue ?? undefined,
});

const searchFunction = async (query: string): Promise<Member[]> => {
  onSearchChange(query);
  return items.value;
};

const getByIdsFunction = async (ids: string[]): Promise<Member[]> => {
  await preloadById(ids);
  return selectedItems.value;
};

const clearItemsFunction = () => {
  items.value = [];
  selectedItems.value = [];
};

const handleRemoteAutocompleteUpdate = (newValues: Member | Member[] | null) => {
  if (props.multiple) {
    const ids = Array.isArray(newValues) ? newValues.map((item: Member) => item.id) : [];
    emit('update:modelValue', ids);
  } else {
    const id = newValues ? (newValues as Member).id : undefined;
    emit('update:modelValue', id);
  }
};
</script>