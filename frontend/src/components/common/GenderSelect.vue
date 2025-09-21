<template>
  <v-select
    :model-value="modelValue"
    @update:model-value="updateModelValue('update:modelValue', $event)"
    :items="genderOptions"
    :label="label"
    :rules="rules"
    :readonly="readOnly"
    :clearable="clearable"
  ></v-select>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { modelValue, label, rules, readOnly, clearable } = defineProps<{
  modelValue: string | null | undefined;
  label?: string;
  // eslint-disable-next-line no-unused-vars
  rules?: Array<(value: unknown) => boolean | string>;
  readOnly?: boolean;
  clearable?: boolean;
}>();

const updateModelValue = defineEmits(['update:modelValue']);

const { t } = useI18n();

const genderOptions = [
  { title: t('member.gender.male'), value: 'Male' },
  { title: t('member.gender.female'), value: 'Female' },
  { title: t('member.gender.other'), value: 'Other' },
];
</script>
