<template>
  <v-autocomplete v-bind="$attrs" v-model="internalValue" v-model:search="search" :items="items" :loading="loading"
    @update:model-value="handleUpdateModelValue" :label="label" :rules="rules" :readonly="readOnly"
    :clearable="clearable" :multiple="multiple" :item-title="(item: Member) => item.fullName || ''" item-value="id"
    density="compact" :disabled="disabled" :return-object="true" :closable-chips="!disabled" :custom-filter="()=> true">
    <template #chip="{ props: chipProps, item }" v-if="!hideChips">
      <v-chip v-bind="chipProps" size="small" v-if="item.raw"
        :prepend-avatar="getAvatarUrl(item.raw.avatarUrl, item.raw.gender)" :text="item.raw.fullName"></v-chip>
    </template>
    <template #item="{ props: itemProps, item }">
      <v-list-item v-bind="itemProps" :subtitle="item.raw?.birthDeathYears">
        <template #prepend>
          <v-avatar :image="getAvatarUrl(item.raw?.avatarUrl, item.raw?.gender)" size="small"></v-avatar>
        </template>
      </v-list-item>
    </template>
  </v-autocomplete>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { Member } from '@/types';
import { getAvatarUrl } from '@/utils/avatar.utils';
import { ApiMemberService } from '@/services/member/api.member.service';
import apiClient from '@/plugins/axios';

const memberService = new ApiMemberService(apiClient);

interface MemberAutocompleteProps {
  modelValue: string | string[] | undefined | null;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean;
  familyId?: string;
  disabled?: boolean;
  hideChips?: boolean;
  debounceTime?: number;
}

const props = withDefaults(defineProps<MemberAutocompleteProps>(), {
  debounceTime: 300,
});

const emit = defineEmits(['update:modelValue']);

const internalValue = ref<Member | Member[] | null>(null);
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

// Query for preloading selected members by their IDs
const { data: preloadedMembers, isLoading: isLoadingPreload } = useQuery<Member[], Error>({
  queryKey: ['members', 'ids', modelValueIds],
  queryFn: async () => {
    if (!modelValueIds.value || modelValueIds.value.length === 0) {
      return [];
    }
    const result = await memberService.getByIds(modelValueIds.value);
    if (result.ok) {
      return result.value;
    }
    console.error('Error preloading members:', result.error);
    throw result.error;
  },
  enabled: computed(() => modelValueIds.value.length > 0),
  staleTime: 1000 * 60 * 5, // 5 minutes
});

// Watch for changes in preloadedMembers and update internalValue
watch(preloadedMembers, (newMembers) => {
  if (props.multiple) {
    internalValue.value = newMembers || [];
  } else {
    internalValue.value = (newMembers && newMembers.length > 0) ? newMembers[0] : null;
  }
}, { immediate: true });

// Query for searching members based on input
const { data: searchResults, isLoading: isLoadingSearch } = useQuery<Member[], Error>({
  queryKey: ['members', 'search', debouncedSearchTerm, props.familyId],
  queryFn: async () => {
    const filters: { [key: string]: any } = {};
    if (debouncedSearchTerm.value) {
      filters.searchQuery = debouncedSearchTerm.value;
    }
    if (props.familyId) {
      filters.familyId = props.familyId;
    }

    if (!debouncedSearchTerm.value && !props.familyId) {
      return [];
    }

    const result = await memberService.search({ page: 1, itemsPerPage: 10 }, filters);
    if (result.ok) {
      return result.value.items;
    }
    console.error('Error fetching members:', result.error);
    throw result.error;
  },
  enabled: computed(() => !!debouncedSearchTerm.value || !!props.familyId),
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

// Handle familyId changes to clear search results and re-trigger query
watch(() => props.familyId, () => {
  // Clear debounced search term to avoid old search results
  debouncedSearchTerm.value = '';
  // The useQuery with familyId in its queryKey will automatically refetch if enabled
  // Also, clear current selection if not multiple
  if (props.multiple) {
    internalValue.value = [];
  } else {
    internalValue.value = null;
  }
});


const handleUpdateModelValue = (value: Member | Member[] | null) => {
  if (props.multiple) {
    const ids = Array.isArray(value) ? value.map((item: Member) => item.id) : [];
    emit('update:modelValue', ids);
  } else {
    const id = value ? (value as Member).id : undefined;
    emit('update:modelValue', id);
  }
};
</script>