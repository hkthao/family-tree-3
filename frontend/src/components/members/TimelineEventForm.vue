<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ title }}</span>
    </v-card-title>
    <v-card-text>
      <v-form ref="form" @submit.prevent="submitForm" :disabled="readOnly">
        <v-text-field
          v-model="eventForm.year"
          :label="t('timeline.form.year')"
          :rules="[rules.required, rules.number]"
          variant="outlined"
          type="number"
        ></v-text-field>
        <v-text-field
          v-model="eventForm.title"
          :label="t('timeline.form.title')"
          :rules="[rules.required]"
          variant="outlined"
        ></v-text-field>
        <v-textarea
          v-model="eventForm.description"
          :label="t('timeline.form.description')"
          variant="outlined"
        ></v-textarea>
        <v-select
          v-model="eventForm.color"
          :items="colorOptions"
          :label="t('timeline.form.color')"
          variant="outlined"
        ></v-select>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="blue-darken-1" variant="text" @click="$emit('cancel')">{{ readOnly ? t('common.close') : t('common.cancel') }}</v-btn>
      <v-btn v-if="!readOnly" color="blue-darken-1" variant="text" @click="submitForm">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';

const props = defineProps<{
  initialEventData?: any; // Define a more specific type later
  readOnly?: boolean;
  title: string;
}>();

const emit = defineEmits(['submit', 'cancel']);

const { t } = useI18n();

const form = ref<HTMLFormElement | null>(null);

const eventForm = ref(props.initialEventData || {
  year: '',
  title: '',
  description: '',
  color: 'blue',
});

const colorOptions = [
  { title: 'Blue', value: 'blue' },
  { title: 'Green', value: 'green' },
  { title: 'Purple', value: 'purple' },
  { title: 'Red', value: 'red' },
  { title: 'Orange', value: 'orange' },
];

const rules = {
  required: (value: string) => !!value || t('validation.required'),
  number: (value: string) => !isNaN(parseFloat(value)) && isFinite(value) || t('validation.number'),
};

const submitForm = async () => {
  const { valid } = await form.value!.validate();
  if (valid) {
    emit('submit', eventForm.value);
  }
};
</script>