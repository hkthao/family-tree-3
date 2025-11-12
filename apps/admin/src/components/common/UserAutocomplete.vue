<template>
  <v-autocomplete
    v-bind="$attrs"
    v-model="internalSelectedItems"
    @update:model-value="handleUpdateModelValue"
    :label="label"
    :rules="rules"
    :readonly="readOnly"
    :clearable="clearable"
    :multiple="multiple"
    :disabled="disabled"
    item-title="name"
    item-value="id"
    :loading="composableLoading"
    :items="combinedItems"
    :search="searchQuery"
    @update:search="onSearchChange"
    chips
    :closable-chips="!disabled"
    return-object
  >
    <template #item="{ props, item }">
      <v-list-item v-bind="props" :subtitle="item.raw.email">
        <template #prepend>
          <v-avatar v-if="item.raw.avatar" :image="item.raw.avatar" size="small"></v-avatar>
        </template>
      </v-list-item>
    </template>
  </v-autocomplete>
</template>

<script setup lang="ts">
import type { UserProfile } from '@/types';
import { useUserAutocomplete } from '@/composables/useUserAutocomplete';
import { ref, watch, onMounted, computed } from 'vue';

interface UserAutocompleteProps {
  modelValue: string | string[] | undefined | null;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean;
  disabled?: boolean;
}

const props = defineProps<UserAutocompleteProps>();

const emit = defineEmits(['update:modelValue']);

const { items, selectedItems, onSearchChange, preloadById, loading: composableLoading } = useUserAutocomplete({
  multiple: props.multiple,
  initialValue: props.modelValue ?? undefined,
});

const searchQuery = ref('');
const internalSelectedItems = ref<UserProfile | UserProfile[] | null>(props.multiple ? [] : null);

const handleUpdateModelValue = (newValues: UserProfile | UserProfile[] | null) => {
  if (props.multiple) {
    const ids = Array.isArray(newValues) ? newValues.map((item: UserProfile) => item.id) : [];
    emit('update:modelValue', ids);
  } else {
    const id = newValues ? (newValues as UserProfile).id : undefined;
    emit('update:modelValue', id);
  }
};

const combinedItems = computed(() => {
  const allItems = [...items.value, ...selectedItems.value];
  // Remove duplicates based on 'id'
  const uniqueItems = Array.from(new Map(allItems.map(item => [item.id, item])).values());
  return uniqueItems;
});

watch(() => props.modelValue, async (newVal) => {
  if (newVal) {
    await preloadById(Array.isArray(newVal) ? newVal : [newVal]);
    internalSelectedItems.value = props.multiple ? selectedItems.value : selectedItems.value[0] || null;
  } else {
    internalSelectedItems.value = props.multiple ? [] : null;
  }
}, { immediate: true });

onMounted(() => {
  onSearchChange('');
});</script>
