<template>
  <v-select
    :model-value="modelValue"
    @update:model-value="$emit('update:modelValue', $event)"
    :items="genderOptions"
    :label="label"
    :rules="rules"
    :readonly="readOnly"
    :clearable="clearable"
  ></v-select>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const props = defineProps<{
  modelValue: any;
  label?: string;
  rules?: Array<any>;
  readOnly?: boolean;
  clearable?: boolean;
}>();

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();

const genderOptions = [
  { title: t('member.gender.male'), value: 'Male' },
  { title: t('member.gender.female'), value: 'Female' },
  { title: t('member.gender.other'), value: 'Other' },
];
</script>
