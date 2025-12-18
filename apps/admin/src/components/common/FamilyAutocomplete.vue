<template>
  <CustomRemoteAutocomplete
    v-bind="$attrs"
    v-model="internalValue"
    @update:model-value="handleUpdateModelValue"
    :label="label"
    :rules="rules"
    :read-only="readOnly"
    :clearable="clearable"
    :multiple="multiple"
    item-title="name"
    item-value="id"
    :fetch-items="fetchItems"
    :loading="isLoadingPreload"
    :disabled="disabled"
    density="compact"
    :return-object="true"
  >
    <template #item="{ props: itemProps, item }">
      <v-list-item v-bind="itemProps" :subtitle="item.raw.address">
        <template #prepend>
          <v-avatar :image="getFamilyAvatarUrl(item.raw.avatarUrl)" size="small"></v-avatar>
        </template>
      </v-list-item>
    </template>
  </CustomRemoteAutocomplete>
</template>

<script setup lang="ts">
import type { Family } from '@/types';
import CustomRemoteAutocomplete from './CustomRemoteAutocomplete.vue';
import { getFamilyAvatarUrl } from '@/utils/avatar.utils';
import { useFamilyAutocompleteData } from '@/composables/autocomplete/useFamilyAutocompleteData';

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

const { internalValue, isLoadingPreload, fetchItems } = useFamilyAutocompleteData(
  props.modelValue,
  props.multiple || false
);

const handleUpdateModelValue = (value: Family | Family[] | null) => {
  if (props.multiple) {
    const ids = Array.isArray(value) ? value.map((item: Family) => item.id) : [];
    emit('update:modelValue', ids);
  } else {
    const id = value ? (value as Family).id : undefined;
    emit('update:modelValue', id);
  }
};
</script>