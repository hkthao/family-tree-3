<template>
  <v-menu
    v-model="menu"
    :close-on-content-click="false"
    location="bottom"
    offset-y
  >
    <template v-slot:activator="{ props: menuProps }">
      <v-text-field
        v-bind="{ ...$attrs, ...menuProps }"
        :model-value="modelValue"
        :label="label"
        :readonly="readonly"
        :disabled="disabled"
        :error-messages="errorMessages as string[]"
        :clearable="clearable"
        :rules="rules as any[]"
        @click:clear="clearColor"
        @blur="$emit('blur')"
        @input="$emit('input')"
      >
        <template v-slot:prepend-inner>
          <v-sheet :color="modelValue" width="24" height="24" class="rounded-circle"></v-sheet>
        </template>
      </v-text-field>
    </template>
    <v-color-picker
      v-model="internalColor"
      @update:model-value="selectColor"
      :swatches="swatches"
      show-swatches
    ></v-color-picker>
  </v-menu>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';

const props = defineProps({
  modelValue: {
    type: String,
    default: '#1976D2', // Default primary color
  },
  label: {
    type: String,
    default: '',
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
const internalColor = ref<string>(props.modelValue);

// Default swatches for the color picker
const swatches = [
  ['#FF0000', '#AA0000', '#550000'],
  ['#FFFF00', '#AAAA00', '#555500'],
  ['#00FF00', '#00AA00', '#005500'],
  ['#00FFFF', '#00AAAA', '#005555'],
  ['#0000FF', '#0000AA', '#000055'],
  ['#FF00FF', '#AA00AA', '#550055'],
  ['#FFFFFF', '#AAAAAA', '#555555'],
  ['#000000', '#111111', '#222222'],
];

watch(() => props.modelValue, (newVal) => {
  if (newVal !== internalColor.value) {
    internalColor.value = newVal;
  }
});

const selectColor = (color: string) => {
  internalColor.value = color;
  emit('update:modelValue', color);
  // Do not close menu on select, allow user to pick multiple shades
};

const clearColor = () => {
  internalColor.value = '#1976D2'; // Reset to default primary color
  emit('update:modelValue', '#1976D2');
};
</script>