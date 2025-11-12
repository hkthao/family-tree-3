<template>
  <RemoteAutocomplete v-bind="$attrs" :model-value="modelValue" @update:model-value="handleRemoteAutocompleteUpdate"
    :label="label" :rules="rules" :read-only="readOnly" :clearable="clearable" :multiple="multiple"
    item-title="fullName" item-value="id" :search-function="searchFunction" :preload-function="getByIdsFunction"
    :clear-items-function="clearItemsFunction" :loading="composableLoading" :items="items" :disabled="disabled">
    <template #chip="{ props, item }" v-if="!hideChips">
      <v-chip v-bind="props" size="small" v-if="item.raw"
        :prepend-avatar="getMemberAvatar(item.raw)" :text="item.raw.fullName"></v-chip>
    </template>
    <template #item="{ props, item }">
      <v-list-item v-bind="props" :subtitle="item.raw?.birthDeathYears">
        <template #prepend>
          <v-avatar :image="getMemberAvatar(item.raw)" size="small"></v-avatar>
        </template>
      </v-list-item>
    </template>
  </RemoteAutocomplete>
</template>

<script setup lang="ts">
import type { Member } from '@/types';
import RemoteAutocomplete from './RemoteAutocomplete.vue';
import { useMemberAutocomplete } from '@/composables/useMemberAutocomplete';
import maleAvatar from '@/assets/images/male_avatar.png';
import femaleAvatar from '@/assets/images/female_avatar.png';
import { Gender } from '@/types';

interface MemberAutocompleteProps {
  modelValue: string | string[] | undefined | null;
  label?: string;
  rules?: any[];
  readOnly?: boolean;
  clearable?: boolean;
  multiple?: boolean;
  familyId?: string;
  loading?: boolean; // Add loading prop
  disabled?: boolean; // Add disabled prop
  hideChips?: boolean; // New prop to hide selected chips
}

const props = defineProps<MemberAutocompleteProps>();

const emit = defineEmits(['update:modelValue']);

const { items, selectedItems, onSearchChange, preloadById, loading: composableLoading } = useMemberAutocomplete({
  familyId: props.familyId,
  multiple: props.multiple,
  initialValue: props.modelValue ?? undefined,
});

const getMemberAvatar = (member: Member) => {
  if (member.avatarUrl) {
    return member.avatarUrl;
  }
  if (member.gender === Gender.Male) {
    return maleAvatar;
  }
  if (member.gender === Gender.Female) {
    return femaleAvatar;
  }
  return maleAvatar; // Fallback for 'Other' or undefined gender
};

const searchFunction = async (query: string): Promise<Member[]> => {
  onSearchChange(query);
  return items.value;
};

const getByIdsFunction = async (ids: string[]): Promise<Member[]> => {
  await preloadById(ids);
  return selectedItems.value;
};

const clearItemsFunction = () => {
  items.value = [];
  selectedItems.value = [];
};

const handleRemoteAutocompleteUpdate = (newValues: Member | Member[] | null) => {
  if (props.multiple) {
    const ids = Array.isArray(newValues) ? newValues.map((item: Member) => item.id) : [];
    emit('update:modelValue', ids);
  } else {
    const id = newValues ? (newValues as Member).id : undefined;
    emit('update:modelValue', id);
  }
};
</script>