<template>
  <v-autocomplete
    v-bind="$attrs"
    v-model="internalValue"
    v-model:search="search"
    :items="items"
    :loading="loading"
    @update:model-value="handleUpdateModelValue"
    :label="label"
    :rules="rules"
    :readonly="readOnly"
    :clearable="clearable"
    :multiple="multiple"
    item-title="name"
    item-value="id"
    :disabled="disabled"
    density="compact"
    :return-object="true"
    :custom-filter="()=> true"
  >
    <template #item="{ props: itemProps, item }">
      <v-list-item v-bind="itemProps" :subtitle="item.raw.address">
        <template #prepend>
          <v-avatar :image="getFamilyAvatarUrl(item.raw.avatarUrl)" size="small"></v-avatar>
        </template>
      </v-list-item>
    </template>
  </v-autocomplete>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { Family } from '@/types';
import { getFamilyAvatarUrl } from '@/utils/avatar.utils';
import { ApiFamilyService } from '@/services/family/api.family.service';
import apiClient from '@/plugins/axios';

const familyService = new ApiFamilyService(apiClient);

interface FamilyAutocompleteProps {
  modelValue: string | string[] | undefined | null;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean;
  disabled?: boolean;
  debounceTime?: number;
}

const props = withDefaults(defineProps<FamilyAutocompleteProps>(), {
  debounceTime: 300,
});

const emit = defineEmits(['update:modelValue']);

const internalValue = ref<Family | Family[] | null>(null);
const search = ref('');
const debouncedSearchTerm = ref('');

let debounceTimer: ReturnType<typeof setTimeout> | null = null;

// Convert modelValue to an array of IDs for queryKey
const modelValueIds = computed(() => {
  if (props.multiple && Array.isArray(props.modelValue)) {
    return props.modelValue as string[];
  } else if (!props.multiple && typeof props.modelValue === 'string') {
    return [props.modelValue as string];
  }
  return [];
});

// Query for preloading selected families by their IDs
const { data: preloadedFamilies, isLoading: isLoadingPreload } = useQuery<Family[], Error>({
  queryKey: ['families', 'ids', modelValueIds],
  queryFn: async () => {
    if (!modelValueIds.value || modelValueIds.value.length === 0) {
      return [];
    }
    const result = await familyService.getByIds(modelValueIds.value);
    if (result.ok) {
      return result.value;
    }
    console.error('Error preloading families:', result.error);
    throw result.error;
  },
  enabled: computed(() => modelValueIds.value.length > 0),
  staleTime: 1000 * 60 * 5, // 5 minutes
});

// Watch for changes in preloadedFamilies and update internalValue
watch(preloadedFamilies, (newFamilies) => {
  if (props.multiple) {
    internalValue.value = newFamilies || [];
  } else {
    internalValue.value = (newFamilies && newFamilies.length > 0) ? newFamilies[0] : null;
  }
}, { immediate: true });

// Query for searching families based on input
const { data: searchResults, isLoading: isLoadingSearch } = useQuery<Family[], Error>({
  queryKey: ['families', 'search', debouncedSearchTerm],
  queryFn: async () => {
    const filters: { [key: string]: any } = {};
    if (debouncedSearchTerm.value) {
      filters.searchQuery = debouncedSearchTerm.value;
    }

    if (!debouncedSearchTerm.value) {
      return [];
    }

    const result = await familyService.search({ page: 1, itemsPerPage: 10 }, filters);
    if (result.ok) {
      return result.value.items;
    }
    console.error('Error fetching families:', result.error);
    throw result.error;
  },
  enabled: computed(() => !!debouncedSearchTerm.value),
  staleTime: 1000 * 30, // 30 seconds
});

// Update items for v-autocomplete from search results
const items = computed(() => searchResults.value || []);

// Combined loading state for v-autocomplete
const loading = computed(() => isLoadingPreload.value || isLoadingSearch.value);

// Debounce search input
watch(search, (newSearchTerm) => {
  if (debounceTimer) {
    clearTimeout(debounceTimer);
  }
  debounceTimer = setTimeout(() => {
    debouncedSearchTerm.value = newSearchTerm;
  }, props.debounceTime);
});

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