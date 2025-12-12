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
import { ref, watch } from 'vue';
import type { PropType } from 'vue';
import type { Family } from '@/types';
import CustomRemoteAutocomplete from './CustomRemoteAutocomplete.vue';
import { getFamilyAvatarUrl } from '@/utils/avatar.utils';
import { ApiFamilyService } from '@/services/family/api.family.service';
import apiClient from '@/plugins/axios';

// Instantiate the service for direct use when preloading by ID and for fetchItems
const familyService = new ApiFamilyService(apiClient);

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

// Logic for preloading selected item(s) when modelValue is an ID(s)
const preloadedFamilies = ref<Family[]>([]);
const internalValue = ref<Family | Family[] | null>(null);
const isLoadingPreload = ref(false); // New loading state for preloading

const fetchFamilyByIds = async (ids: string[]) => {
  if (!ids || ids.length === 0) {
    preloadedFamilies.value = [];
    return;
  }
  isLoadingPreload.value = true;
  try {
    const result = await familyService.getByIds(ids);
    if (result.ok) {
      preloadedFamilies.value = result.value;
    } else {
      console.error('Error preloading families:', result.error);
      preloadedFamilies.value = [];
    }
  } finally {
    isLoadingPreload.value = false;
  }
};

watch(() => props.modelValue, async (newModelValue) => {
  if (newModelValue) {
    if (props.multiple && Array.isArray(newModelValue)) {
      await fetchFamilyByIds(newModelValue as string[]);
      internalValue.value = preloadedFamilies.value;
    } else if (!props.multiple && typeof newModelValue === 'string') {
      await fetchFamilyByIds([newModelValue as string]);
      internalValue.value = preloadedFamilies.value[0] || null;
    }
  } else {
    internalValue.value = null;
  }
}, { immediate: true });


const handleUpdateModelValue = (value: Family | Family[] | null) => {
  if (props.multiple) {
    const ids = Array.isArray(value) ? value.map((item: Family) => item.id) : [];
    emit('update:modelValue', ids);
  } else {
    const id = value ? (value as Family).id : undefined;
    emit('update:modelValue', id);
  }
};

const fetchItems = async (query: string): Promise<Family[]> => {
  const result = await familyService.search({ page: 1, itemsPerPage: 10 }, { name: query });
  if (result.ok) {
    return result.value.items;
  }
  console.error('Error fetching families:', result.error);
  return [];
};
</script>