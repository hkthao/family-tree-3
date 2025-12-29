<template>
  <v-select v-bind="$attrs" :model-value="props.modelValue" @update:model-value="emit('update:modelValue', $event)"
    :items="genderOptions" item-title="title" item-value="value" :label="props.label" :rules="props.rules"
    :readonly="props.readOnly" :clearable="props.clearable"></v-select>
</template>

<script setup lang="ts">
import { Gender } from '@/types';
import { GENDER_OPTIONS } from '@/constants/genders';

interface GenderSelectProps {
  modelValue: Gender | null | undefined;
  label?: string;
  rules?: Array<(value: unknown) => boolean | string>;
  readOnly?: boolean;
  clearable?: boolean;
}

const props = defineProps<GenderSelectProps>();

const emit = defineEmits(['update:modelValue']);

const genderOptions = GENDER_OPTIONS;
</script>