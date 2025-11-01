<template>
  <v-autocomplete v-model="internalSelectedItems" @update:model-value="handleAutocompleteUpdate" :items="members"
    item-title="fullName" item-value="id" :label="label" :rules="rules" :readonly="readOnly" :clearable="clearable"
    :loading="loading" :search="searchTerm" @update:search="onSearchInput" :multiple="multiple" :chips="multiple"
    :closable-chips="multiple" return-object :hide-details="hideDetails">
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
  </v-autocomplete>
</template>

<script setup lang="ts">
import { ref, watch, onMounted, computed } from 'vue';
import { useMemberAutocompleteStore } from '@/stores';

// A more specific type for the function being debounced
type DebounceableFunction = (...args: any[]) => void;

const debounce = (func: DebounceableFunction, delay: number) => {
  let timeout: ReturnType<typeof setTimeout>;
  return (...args: any[]) => {
    clearTimeout(timeout);
    timeout = setTimeout(() => func(...args), delay);
  };
};

interface MemberAutocompleteProps {
  modelValue: (string | number)[] | string | number | undefined | null;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean; // New prop for multiple selection
  hideDetails?: boolean | "auto";
  familyId?: string;
}

const props = defineProps<MemberAutocompleteProps>();

const emit = defineEmits(['update:modelValue']);

const memberAutocompleteStore = useMemberAutocompleteStore();
const loading = ref(false);
const searchTerm = ref('');
const internalSelectedItems = ref<any[]>([]); // For return-object handling

const members = computed(() => memberAutocompleteStore.items);

const fetchMembers = async (query: string = '') => {
  loading.value = true;
  try {
    await memberAutocompleteStore.searchMembers({ searchQuery: query, familyId: props.familyId }); // Use new store's action
  } catch (error) {
    console.error('Error fetching members:', error);
  } finally {
    loading.value = false;
  }
};

const debouncedFetchMembers = debounce(fetchMembers, 300);

const onSearchInput = (query: string) => {
  searchTerm.value = query;
  debouncedFetchMembers(query);
};

const handleAutocompleteUpdate = (newValues: any | any[]) => {
  if (props.multiple) {
    internalSelectedItems.value = newValues; // newValues is an array of objects
    emit('update:modelValue', newValues.map((item: any) => item.id)); // Emit array of IDs
  } else {
    internalSelectedItems.value = newValues ? [newValues] : []; // newValues is a single object
    emit('update:modelValue', newValues ? newValues.id : undefined); // Emit single ID
  }
};

// Preload selected member based on modelValue (IDs)
watch(
  () => props.modelValue,
  async (newModelValue) => {
    internalSelectedItems.value = []; // Clear current selections

    let idsToFetch: string[] = [];

    if (props.multiple) {
      if (Array.isArray(newModelValue) && newModelValue.length > 0) {
        idsToFetch = newModelValue.map(id => String(id));
      }
    } else { // Single selection
      if (newModelValue !== undefined && newModelValue !== null) {
        idsToFetch = [String(newModelValue)];
      }
    }

    if (idsToFetch.length > 0) {
      loading.value = true;
      try {
        const fetchedItems = await memberAutocompleteStore.getMemberByIds(idsToFetch);
        internalSelectedItems.value = fetchedItems;
      } catch (error) {
        console.error('Error preloading selected items:', error);
      } finally {
        loading.value = false;
      }
    }
  },
  { immediate: true, deep: true }
);

watch(() => props.familyId, () => {
    // Clear the selection when familyId changes
    internalSelectedItems.value = [];
    emit('update:modelValue', props.multiple ? [] : undefined);
    fetchMembers();
});

// Initial fetch for empty search to show some options
onMounted(() => {
  fetchMembers();
});
</script>
