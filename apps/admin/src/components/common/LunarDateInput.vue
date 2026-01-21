<template>
  <v-row>
    <v-col cols="12" md="4">
      <v-select
        :model-value="internalLunarDate.day"
        @update:model-value="updateDay"
        :items="lunarDays"
        :label="labelDay"
        :readonly="readOnly"
        data-testid="lunar-date-day-select"
        prepend-inner-icon="mdi-calendar-today"
        item-value="value"
        item-title="title"
        :rules="dayRules"
      ></v-select>
    </v-col>
    <v-col cols="12" md="4">
      <v-select
        :model-value="internalLunarDate.month"
        @update:model-value="updateMonth"
        :items="lunarMonths"
        :label="labelMonth"
        :readonly="readOnly"
        data-testid="lunar-date-month-select"
        prepend-inner-icon="mdi-calendar-range"
        item-value="value"
        item-title="title"
        :rules="monthRules"
      ></v-select>
    </v-col>
    <v-col cols="12" md="4" class="d-flex align-center">
      <v-checkbox
        :model-value="internalLunarDate.isLeapMonth"
        @update:model-value="updateIsLeapMonth"
        :label="labelIsLeapMonth"
        :readonly="readOnly"
        data-testid="lunar-date-is-leap-month-checkbox"
      ></v-checkbox>
    </v-col>
  </v-row>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import type { LunarDate } from '@/types'; // Import LunarDate interface
import { useI18n } from 'vue-i18n';

interface LunarDateInputProps {
  modelValue: LunarDate | null | undefined;
  readOnly?: boolean;
  labelDay?: string;
  labelMonth?: string;
  labelIsLeapMonth?: string;
  rules?: any[];
  required?: boolean;
}

const props = withDefaults(defineProps<LunarDateInputProps>(), {
  readOnly: false,
  labelDay: 'Ngày âm',
  labelMonth: 'Tháng âm',
  labelIsLeapMonth: 'Là tháng nhuận',
  rules: () => [],
  required: false,
});

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();

// Internal state to hold the lunar date, initialized from modelValue
const internalLunarDate = ref<LunarDate>(props.modelValue || { day: 1, month: 1, isLeapMonth: false });

// Watch for changes in the parent's modelValue and update internal state
watch(() => props.modelValue, (newValue) => {
  if (newValue) {
    internalLunarDate.value = { ...newValue };
  } else {
    // If modelValue becomes null/undefined, reset to default or an empty state
    internalLunarDate.value = { day: 1, month: 1, isLeapMonth: false };
  }
}, { deep: true });




const lunarDays = computed(() => {
  return Array.from({ length: 30 }, (_, i) => ({ title: `${i + 1}`, value: i + 1 }));
});

const lunarMonths = computed(() => {
  return Array.from({ length: 12 }, (_, i) => ({ title: `${i + 1}`, value: i + 1 }));
});

// Event handlers to update internal state and trigger emit
const updateDay = (value: number) => {
  const newLunarDate = { ...internalLunarDate.value, day: value };
  internalLunarDate.value = newLunarDate;
  emit('update:modelValue', newLunarDate);
};

const updateMonth = (value: number) => {
  const newLunarDate = { ...internalLunarDate.value, month: value };
  internalLunarDate.value = newLunarDate;
  emit('update:modelValue', newLunarDate);
};

const updateIsLeapMonth = (value: boolean | null) => {
  const newLunarDate = { ...internalLunarDate.value, isLeapMonth: value ?? false };
  internalLunarDate.value = newLunarDate;
  emit('update:modelValue', newLunarDate);
};


// Validation rules
const dayRules = computed(() => {
  const rules = [];
  if (props.required) {
    rules.push((v: number) => !!v || t('common.form.required'));
  }
  rules.push((v: number) => (v >= 1 && v <= 30) || t('common.form.invalidDay'));
  return [...rules, ...props.rules];
});

const monthRules = computed(() => {
  const rules = [];
  if (props.required) {
    rules.push((v: number) => !!v || t('common.form.required'));
  }
  rules.push((v: number) => (v >= 1 && v <= 12) || t('common.form.invalidMonth'));
  return [...rules, ...props.rules];
});
</script>
