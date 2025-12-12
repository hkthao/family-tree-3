<template>
  <div>
    <v-autocomplete
      v-model="internalValue"
      v-model:search="currentSearchText"
      :items="autocompleteItems"
      :label="label"
      :rules="rules"
      :readonly="readOnly"
      :clearable="clearable"
      :disabled="disabled"
      :multiple="props.multiple"
      chips
      closable-chips
      item-title="email"
      item-value="id"
      return-object
      no-filter
      @update:modelValue="handleUpdateModelValue"
      @click:clear="handleClear"
      v-bind="$attrs"
      variant="outlined"
      density="comfortable"
      :loading="isLoadingAutocomplete"
      :hide-details="props.hideDetails"
    >
      <template v-slot:chip="{ props, item }">
        <v-chip
          v-bind="props"
          :prepend-avatar="getAvatarUrl(item.raw.avatarUrl, undefined)"
          :text="item.raw.email || item.raw.name"
        ></v-chip>
      </template>

      <template v-slot:item="{ props, item }">
        <v-list-item
          v-bind="props"
          :prepend-avatar="getAvatarUrl(item.raw.avatarUrl, undefined)"
          :title="item.raw.email || item.raw.name"
          :subtitle="item.raw.email && item.raw.name ? item.raw.name : ''"
        ></v-list-item>
      </template>
    </v-autocomplete>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import type { UserDto } from '@/types';
import { getAvatarUrl } from '@/utils/avatar.utils';
import { ApiUserService } from '@/services/user/api.user.service';
import apiClient from '@/plugins/axios';

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
  hideChips?: boolean; // For member-chip display (no longer needed with v-autocomplete chips)
  hideDetails?: boolean; // For member-chip display
}

const props = defineProps<UserAutocompleteProps>();

const emit = defineEmits(['update:modelValue']);

const internalValue = ref<UserDto | UserDto[] | null>(null);
const currentSearchText = ref('');
const autocompleteItems = ref<UserDto[]>([]);
const isLoadingAutocomplete = ref(false);
let debounceTimer: ReturnType<typeof setTimeout> | null = null;

const fetchAutocompleteItems = async (query: string) => {
  if (!query) {
    autocompleteItems.value = [];
    return;
  }
  isLoadingAutocomplete.value = true;
  try {
    const result = await userService.search(query, 1, 10);
    if (result.ok) {
      autocompleteItems.value = result.value.items;
    } else {
      console.error('Error fetching autocomplete items:', result.error);
      autocompleteItems.value = [];
    }
  } finally {
    isLoadingAutocomplete.value = false;
  }
};

watch(currentSearchText, (newSearchText) => {
  if (debounceTimer) {
    clearTimeout(debounceTimer);
  }
  debounceTimer = setTimeout(() => {
    fetchAutocompleteItems(newSearchText);
  }, 300); // Debounce time
});


// Logic for preloading selected item(s) when modelValue is an ID(s)
const fetchUserByIds = async (ids: string[]) => {
  if (!ids || ids.length === 0) {
    return [];
  }
  isLoadingAutocomplete.value = true; // Use common loading indicator
  try {
    const result = await userService.getByIds(ids);
    if (result.ok) {
      return result.value || [];
    } else {
      console.error('Error preloading users:', result.error);
      return [];
    }
  } finally {
    isLoadingAutocomplete.value = false;
  }
};

watch(() => props.modelValue, async (newModelValue) => {
  if (props.multiple) {
    if (Array.isArray(newModelValue) && newModelValue.length > 0) {
      internalValue.value = await fetchUserByIds(newModelValue as string[]);
    } else {
      internalValue.value = [];
    }
  } else {
    if (typeof newModelValue === 'string' && newModelValue) {
      const users = await fetchUserByIds([newModelValue]);
      internalValue.value = users[0] || null;
    } else {
      internalValue.value = null;
    }
  }
}, { immediate: true });


const handleUpdateModelValue = (value: UserDto | UserDto[] | null) => {
  if (props.multiple) {
    const ids = Array.isArray(value) ? value.map((item: UserDto) => item.id) : [];
    emit('update:modelValue', ids);
  } else {
    const id = value ? (value as UserDto).id : undefined;
    emit('update:modelValue', id);
  }
};

const handleClear = () => {
  internalValue.value = props.multiple ? [] : null;
  currentSearchText.value = '';
  autocompleteItems.value = [];
  handleUpdateModelValue(internalValue.value);
};

// No longer need handleEnter or removeUser explicitly as v-autocomplete handles them

</script>
