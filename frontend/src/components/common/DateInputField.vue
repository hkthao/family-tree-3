<template>
  <v-menu
    v-model="menu"
    :close-on-content-click="false"
    transition="scale-transition"
    offset-y
    min-width="auto"
    :disabled="props.readonly"
  >
    <template v-slot:activator="{ props: activatorProps }">
      <v-text-field
        :model-value="formattedDate"
        @click="menu = true"
        :label="label"
        append-inner-icon="mdi-calendar"
        :readonly="props.readonly"
        v-bind="activatorProps"
        :rules="rules"
      ></v-text-field>
    </template>
    <v-date-picker
      :model-value="modelValue"
      @update:model-value="handleDateUpdate"
    ></v-date-picker>
  </v-menu>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';

const props = defineProps({
  modelValue: {
    type: [String, Date, null],
    default: null,
  },
  label: {
    type: String,
    required: true,
  },
  rules: {
    type: Array,
    default: () => [],
  },
  optional: {
    type: Boolean,
    default: false,
  },
  readonly: {
    type: Boolean,
    default: false,
  },
});

const emit = defineEmits(['update:modelValue']);

const menu = ref(false);

const formattedDate = computed(() => {
  if (!props.modelValue) return '';
  const date = props.modelValue instanceof Date ? props.modelValue : new Date(props.modelValue);
  return date.toLocaleDateString('en-GB'); // dd/MM/yyyy
});

const handleDateUpdate = (date: Date | null) => {
  emit('update:modelValue', date);
  menu.value = false;
};
</script>
