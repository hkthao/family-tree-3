<template>
  <div>
    <!-- Display selected chips -->
    <div v-if="props.multiple && internalValue && (internalValue as UserDto[]).length > 0 && !hideChips" class="mb-2">
      <v-chip
        v-for="user in (internalValue as UserDto[])"
        :key="user.id"
        size="small"
        :prepend-avatar="getAvatarUrl(user.avatarUrl, undefined)"
        class="mr-1 mb-1"
        closable
        @click:close="removeUser(user)"
      >
        {{ user.email || user.name }}
      </v-chip>
    </div>

    <v-text-field
      v-model="currentSearchText"
      :label="label"
      :rules="rules"
      :readonly="readOnly"
      :clearable="clearable && !props.multiple"
      :disabled="disabled"
      @keydown.enter.prevent="handleEnter"
      @click:clear="handleClear"
      v-bind="$attrs"
      variant="outlined"
      density="comfortable"
    >
      <template #append-inner v-if="shouldShowAppendInner">
        <v-avatar :image="avatarSrc" size="small" class="mr-2"></v-avatar>
      </template>
    </v-text-field>
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
  hideChips?: boolean; // For member-chip display
}

const props = defineProps<UserAutocompleteProps>();

const emit = defineEmits(['update:modelValue']);

// Logic for preloading selected item(s) when modelValue is an ID(s)
const preloadedUsers = ref<UserDto[]>([]);
const internalValue = ref<UserDto | UserDto[] | null>(null);
const isLoadingPreload = ref(false); // Retaining for future loading states if needed, though less critical now.

// Use a local ref for searchText
const currentSearchText = ref('');

// Computed properties for template logic
const shouldShowAppendInner = computed(() => !props.multiple && internalValue.value);

const avatarSrc = computed(() => {
  const user = internalValue.value;
  if (!props.multiple && user && typeof user === 'object' && 'avatarUrl' in user && 'userId' in user) { // Check for 'userId' to confirm it's UserDto-like
    return getAvatarUrl((user as UserDto).avatarUrl, undefined);
  }
  return '';
});

const fetchUserByIds = async (ids: string[]) => {
  if (!ids || ids.length === 0) {
    preloadedUsers.value = [];
    return;
  }
  isLoadingPreload.value = true;
  try {
    const result = await userService.getByIds(ids);
    if (result.ok) {
      preloadedUsers.value = result.value || [];
    } else {
      console.error('Error preloading users:', result.error);
      preloadedUsers.value = [];
    }
  } finally {
    isLoadingPreload.value = false;
  }
};

watch(() => props.modelValue, async (newModelValue) => {
  if (newModelValue) {
    if (props.multiple && Array.isArray(newModelValue)) {
      await fetchUserByIds(newModelValue as string[]);
      internalValue.value = preloadedUsers.value;
    } else if (!props.multiple && typeof newModelValue === 'string') {
      await fetchUserByIds([newModelValue as string]);
      const user = preloadedUsers.value[0];
      if (user) {
        internalValue.value = user;
        currentSearchText.value = user.email || user.name || ''; // Display preloaded user in text field
      } else {
        internalValue.value = null;
        currentSearchText.value = '';
      }
    }
  } else {
    internalValue.value = null;
    currentSearchText.value = '';
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

// Handle Enter key press to add user if not selected from dropdown
const handleEnter = async () => {
  const query = currentSearchText.value.trim();
  if (!query) return;

  // Perform a direct search to check for exact match
  const result = await userService.findUser(query);
  if (result.ok && result.value) { // Check if result is OK and a user was found (not undefined)
    const foundUser = result.value;

    // If multiple selection, add the user to internalValue if not already present
    if (props.multiple) {
      const currentSelected = (internalValue.value as UserDto[] || []);
      if (!currentSelected.some(user => user.id === foundUser.id)) {
        internalValue.value = [...currentSelected, foundUser];
        handleUpdateModelValue(internalValue.value); // Emit updated IDs
      }
    } else {
      // For single selection, set the found user
      internalValue.value = foundUser;
      handleUpdateModelValue(internalValue.value); // Emit updated ID
      currentSearchText.value = foundUser.email || foundUser.name || ''; // Display selected user
    }
    currentSearchText.value = ''; // Clear search text after adding (for multiple) or selection
  } else {
    // Optionally: show a message that user was not found or not unique
    console.log(`User "${query}" not found or not unique.`);
    // Keep search text if user not found, so user can edit
  }
};

const removeUser = (userToRemove: UserDto) => {
  if (props.multiple && Array.isArray(internalValue.value)) {
    internalValue.value = internalValue.value.filter(user => user.id !== userToRemove.id);
    handleUpdateModelValue(internalValue.value);
  }
};

const handleClear = () => {
  if (!props.multiple) {
    internalValue.value = null;
    handleUpdateModelValue(null);
    currentSearchText.value = '';
  }
  // For multiple, clear button would ideally clear only currentSearchText, not selected chips
  // If the clear button for v-text-field is used, it only clears currentSearchText
};

// For single selection, if the input text is manually cleared, also clear the modelValue
watch(currentSearchText, (newValue) => {
  if (!props.multiple && !newValue && internalValue.value) {
    internalValue.value = null;
    handleUpdateModelValue(null);
  }
});
</script>