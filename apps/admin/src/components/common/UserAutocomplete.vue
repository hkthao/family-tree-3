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
    item-title="email"
    item-value="id"
    :fetch-items="fetchItems"
    :loading="isLoadingPreload"
    :disabled="disabled"
    :return-object="true"
    :hide-details="hideDetails"
  >
    <template #chip="{ props: chipProps, item }" v-if="!hideChips">
      <v-chip v-bind="chipProps" size="small" v-if="item.raw"
        :prepend-avatar="getAvatarUrl(item.raw.avatarUrl, undefined)" :text="item.raw.email || item.raw.name"></v-chip>
    </template>
    <template #item="{ props: itemProps, item }">
      <v-list-item v-bind="itemProps" :subtitle="item.raw?.name">
        <template #prepend>
          <v-avatar :image="getAvatarUrl(item.raw?.avatarUrl, undefined)" size="small"></v-avatar>
        </template>
      </v-list-item>
    </template>
  </CustomRemoteAutocomplete>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import type { UserDto } from '@/types';
import { getAvatarUrl } from '@/utils/avatar.utils';
import { ApiUserService } from '@/services/user/api.user.service';
import apiClient from '@/plugins/axios';
import CustomRemoteAutocomplete from './CustomRemoteAutocomplete.vue';
import { useUserByIdsQuery } from '@/composables/user/useUserByIdsQuery';

// Instantiate the service for direct use
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
}

const props = defineProps<UserAutocompleteProps>();

const emit = defineEmits(['update:modelValue']);

// Logic for preloading selected item(s) when modelValue is an ID(s)
const internalValue = ref<UserDto | UserDto[] | null>(null);

const userIdsToPreload = computed(() => {
  if (props.multiple && Array.isArray(props.modelValue)) {
    return props.modelValue as string[];
  } else if (!props.multiple && typeof props.modelValue === 'string') {
    return [props.modelValue as string];
  }
  return [];
});

const { users: preloadedUsersData, isLoading: isLoadingPreload } = useUserByIdsQuery(userIdsToPreload);

watch(preloadedUsersData, (newUsers) => {
  if (props.multiple) {
    internalValue.value = newUsers;
  } else {
    internalValue.value = newUsers[0] || null;
  }
}, { immediate: true });

watch(() => props.modelValue, (newModelValue) => {
  if (!newModelValue) {
    internalValue.value = null;
  }
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

const fetchItems = async (query: string): Promise<UserDto[]> => {
  if (!query) {
    return [];
  }

  const result = await userService.search(query, 1, 10);
  if (result.ok) {
    return result.value.items;
  }
  console.error('Error fetching users:', result.error);
  return [];
};
</script>