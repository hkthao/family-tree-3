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
    density="compact"
    :fetch-items="fetchItems"
    :loading="isLoadingPreload"
    :disabled="disabled"
    :return-object="true"
    :hide-no-data="true" 
    :closable-chips="!disabled"
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
import type { Member } from '@/types';
import CustomRemoteAutocomplete from './CustomRemoteAutocomplete.vue';
import { getAvatarUrl } from '@/utils/avatar.utils';
import { useMemberAutocompleteData } from '@/composables';

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

const { internalValue, isLoadingPreload, fetchItems } = useMemberAutocompleteData(
  props.modelValue,
  props.multiple || false,
  props.familyId
);

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