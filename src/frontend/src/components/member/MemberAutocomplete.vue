<template>
  <v-autocomplete
    v-model="selectedMember"
    :items="memberStore.list.items"
    :loading="memberStore.list.loading"
    :search-input.sync="search"
    item-text="fullName"
    item-value="id"
    :label="label"
    placeholder="Start typing to search for a member"
    prepend-icon="mdi-account-search"
    return-object
    clearable
  ></v-autocomplete>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useMemberStore } from '@/stores/member.store';
import type { Member } from '@/types';

interface MemberAutocompleteProps {
  label: string;
}

const props = defineProps<MemberAutocompleteProps>();
const emit = defineEmits(['update:model-value']);

const memberStore = useMemberStore();
const selectedMember = ref<Member | null>(null);
const search = ref('');

watch(search, (val) => {
  if (val && val.length > 2) {
    memberStore.list.filters = { searchQuery: val };
    memberStore._loadItems();
  }
});

watch(selectedMember, (val) => {
  emit('update:model-value', val);
});
</script>
