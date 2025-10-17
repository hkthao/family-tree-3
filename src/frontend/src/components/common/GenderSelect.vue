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

import { Gender } from '@/types'; // Added back
import { GENDER_OPTIONS } from '@/constants/genders'; // Added

const { modelValue, label, rules, readOnly, clearable } = defineProps<{
  modelValue: Gender | null | undefined;
  label?: string;
  rules?: Array<(value: unknown) => boolean | string>;
  readOnly?: boolean;
  clearable?: boolean;
}>();

const updateModelValue = defineEmits(['update:modelValue']);



const genderOptions = GENDER_OPTIONS; // Used GENDER_OPTIONS
</script>
