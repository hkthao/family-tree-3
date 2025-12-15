<template>
  <v-menu
    v-model="menu"
    :close-on-content-click="false"
    location="end"
    offset-y
    max-width="290px"
    min-width="auto"
  >
    <template v-slot:activator="{ props: menuProps }">
      <v-text-field
        v-bind="{ ...$attrs, ...menuProps }"
        :model-value="formattedDate"
        :label="label"
        :readonly="readonly"
        :disabled="disabled"
        :error-messages="errorMessages as string[]"
        :clearable="clearable"
        :rules="rules as any[]"
        @click:clear="clearDate"
        @blur="$emit('blur')"
        @input="$emit('input')"
      >
        <template v-slot:append-inner>
          <v-icon @click="menu = true">mdi-calendar</v-icon>
        </template>
      </v-text-field>
    </template>
    <v-date-picker
      v-model="internalDate"
      :show-current="true"
      :color="color"
      @update:model-value="selectDate"
    ></v-date-picker>
  </v-menu>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { format, parseISO } from 'date-fns';

const props = defineProps({
  modelValue: {
    type: [Date, String, null], // Allow null for modelValue
    default: null,
  },
  label: {
    type: String,
    default: '',
  },
  format: {
    type: String,
    default: 'dd/MM/yyyy',
  },
  readonly: {
    type: Boolean,
    default: false,
  },
  disabled: {
    type: Boolean,
    default: false,
  },
  errorMessages: {
    type: [String, Array],
    default: () => [],
  },
  clearable: {
    type: Boolean,
    default: false,
  },
  rules: {
    type: Array,
    default: () => [],
  },
  color: {
    type: String,
    default: 'primary',
  },
});

const emit = defineEmits(['update:modelValue', 'blur', 'input']);

const menu = ref(false);
const internalDate = ref<Date | null>(null);

// Initialize internalDate from modelValue
watch(() => props.modelValue, (newVal) => {
  if (newVal) {
    internalDate.value = typeof newVal === 'string' ? parseISO(newVal) : newVal;
  } else {
    internalDate.value = null;
  }
}, { immediate: true });

const formattedDate = computed(() => {
  if (internalDate.value) {
    return format(internalDate.value, props.format);
  }
  return '';
});

const selectDate = (date: Date | null) => { // Allow null for date
  internalDate.value = date;
  emit('update:modelValue', date);
  menu.value = false;
};

const clearDate = () => {
  internalDate.value = null;
  emit('update:modelValue', null);
};
</script>