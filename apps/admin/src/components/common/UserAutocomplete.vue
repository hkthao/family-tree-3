<template>
  <v-autocomplete v-bind="$attrs" v-model="internalValue" v-model:search="search" :items="items" :loading="loading"
    @update:model-value="handleUpdateModelValue" :label="label" :rules="rules" :readonly="readOnly"
    :clearable="clearable" :multiple="multiple" item-title="email" item-value="id" :disabled="disabled"
    density="compact" :return-object="true" :hide-details="hideDetails" chips :closable-chips="!disabled"
    :custom-filter="() => true">
  </v-autocomplete>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useQuery } from '@tanstack/vue-query';
import type { UserDto } from '@/types';
import { ApiUserService } from '@/services/user/api.user.service';
import apiClient from '@/plugins/axios';

const userService = new ApiUserService(apiClient);

interface UserAutocompleteProps {
  modelValue: string | string[] | undefined | null;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean;
  disabled?: boolean;
  hideChips?: boolean;
  hideDetails?: boolean;
  debounceTime?: number;
}

const props = withDefaults(defineProps<UserAutocompleteProps>(), {
  debounceTime: 300,
});

const emit = defineEmits(['update:modelValue']);

const internalValue = ref<UserDto | UserDto[] | null>(null);
const search = ref('');
const debouncedSearchQuery = ref('');

let debounceTimer: ReturnType<typeof setTimeout> | null = null;

const modelValueIds = computed(() => {
  if (props.multiple && Array.isArray(props.modelValue)) {
    return props.modelValue as string[];
  } else if (!props.multiple && typeof props.modelValue === 'string') {
    return [props.modelValue as string];
  }
  return [];
});

const { data: preloadedUsers, isLoading: isLoadingPreload } = useQuery<UserDto[], Error>({
  queryKey: ['users', 'ids', modelValueIds],
  queryFn: async () => {
    if (!modelValueIds.value || modelValueIds.value.length === 0) {
      return [];
    }
    const result = await userService.getByIds(modelValueIds.value);
    if (result.ok) {
      return result.value;
    }
    console.error('Error preloading users:', result.error);
    throw result.error;
  },
  enabled: computed(() => modelValueIds.value.length > 0),
  staleTime: 1000 * 60 * 5, // 5 minutes
});

watch(preloadedUsers, (newUsers) => {
  if (props.multiple) {
    internalValue.value = newUsers || [];
  } else {
    internalValue.value = (newUsers && newUsers.length > 0) ? newUsers[0] : null;
  }
}, { immediate: true });

const { data: searchResults, isLoading: isLoadingSearch } = useQuery<UserDto[], Error>({
  queryKey: ['users', 'search', debouncedSearchQuery],
  queryFn: async () => {
    const result = await userService.search(debouncedSearchQuery.value, 1, 10);
    if (result.ok) {
      return result.value.items;
    }
    console.error('Error fetching users:', result.error);
    throw result.error;
  },
  enabled: computed(() => !!debouncedSearchQuery.value),
  staleTime: 1000 * 30, // 30 seconds
});

const items = computed(() => searchResults.value || []);

const loading = computed(() => isLoadingPreload.value || isLoadingSearch.value);

watch(search, (newSearchQuery) => {
  if (debounceTimer) {
    clearTimeout(debounceTimer);
  }
  debounceTimer = setTimeout(() => {
    debouncedSearchQuery.value = newSearchQuery;
  }, props.debounceTime);
});

const handleUpdateModelValue = (value: UserDto | UserDto[] | null) => {
  if (props.multiple) {
    const ids = Array.isArray(value) ? value.map((item: UserDto) => item.id) : [];
    emit('update:modelValue', ids);
  } else {
    const id = value ? (value as UserDto).id : undefined;
    emit('update:modelValue', id);
  }
};
</script>