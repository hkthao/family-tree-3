<template>
  <v-select
    :model-value="modelValue"
    @update:model-value="updateModelValue('update:modelValue', $event)"
    :items="genderOptions"
    item-title="title"
    item-value="value"
    :label="label"
    :rules="rules"
    :readonly="readOnly"
    :clearable="clearable"
  ></v-select>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { Gender } from '@/types/gender';

const { modelValue, label, rules, readOnly, clearable } = defineProps<{
  modelValue: Gender | null | undefined;
  label?: string;
  rules?: Array<(value: unknown) => boolean | string>;
  readOnly?: boolean;
  clearable?: boolean;
}>();

const updateModelValue = defineEmits(['update:modelValue']);

const { t } = useI18n();

const genderOptions = Object.values(Gender).map((gender) => ({
  title: t(`member.gender.${gender.toLowerCase()}`),
  value: gender,
}));
</script>
