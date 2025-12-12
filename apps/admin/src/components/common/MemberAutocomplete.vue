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
    item-title="fullName"
    item-value="id"
    :fetch-items="fetchItems"
    :loading="isLoadingPreload"
    :disabled="disabled"
    :return-object="true"
  >
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
  </CustomRemoteAutocomplete>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import type { Member } from '@/types';
import CustomRemoteAutocomplete from './CustomRemoteAutocomplete.vue';
import { getAvatarUrl } from '@/utils/avatar.utils';
import { ApiMemberService } from '@/services/member/api.member.service';
import apiClient from '@/plugins/axios';

// Instantiate the service for direct use
const memberService = new ApiMemberService(apiClient);

interface MemberAutocompleteProps {
  modelValue: string | string[] | undefined | null;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean;
  familyId?: string; // Prop for filtering by family
  disabled?: boolean;
  hideChips?: boolean;
}

const props = defineProps<MemberAutocompleteProps>();

const emit = defineEmits(['update:modelValue']);

// Logic for preloading selected item(s) when modelValue is an ID(s)
const preloadedMembers = ref<Member[]>([]);
const internalValue = ref<Member | Member[] | null>(null);
const isLoadingPreload = ref(false);

const fetchMemberByIds = async (ids: string[]) => {
  if (!ids || ids.length === 0) {
    preloadedMembers.value = [];
    return;
  }
  isLoadingPreload.value = true;
  try {
    const result = await memberService.getByIds(ids);
    if (result.ok) {
      preloadedMembers.value = result.value;
    } else {
      console.error('Error preloading members:', result.error);
      preloadedMembers.value = [];
    }
  } finally {
    isLoadingPreload.value = false;
  }
};

watch(() => props.modelValue, async (newModelValue) => {
  if (newModelValue) {
    if (props.multiple && Array.isArray(newModelValue)) {
      await fetchMemberByIds(newModelValue as string[]);
      internalValue.value = preloadedMembers.value;
    } else if (!props.multiple && typeof newModelValue === 'string') {
      await fetchMemberByIds([newModelValue as string]);
      internalValue.value = preloadedMembers.value[0] || null;
    }
  } else {
    internalValue.value = null;
  }
}, { immediate: true });


const handleUpdateModelValue = (value: Member | Member[] | null) => {
  if (props.multiple) {
    const ids = Array.isArray(value) ? value.map((item: Member) => item.id) : [];
    emit('update:modelValue', ids);
  } else {
    const id = value ? (value as Member).id : undefined;
    emit('update:modelValue', id);
  }
};

const fetchItems = async (query: string): Promise<Member[]> => {
  const filters: { [key: string]: any } = {};
  if (query) {
    filters.fullName = query;
  }
  if (props.familyId) { // Apply familyId filter if provided
    filters.familyId = props.familyId;
  }

  // Only search if there's a query or a familyId is provided
  if (!query && !props.familyId) {
    return [];
  }

  const result = await memberService.search({ page: 1, itemsPerPage: 10 }, filters);
  if (result.ok) {
    return result.value.items;
  }
  console.error('Error fetching members:', result.error);
  return [];
};
</script>