<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ title }}</span>
    </v-card-title>
    <v-card-text>
      <v-form ref="form" @submit.prevent="submitForm" :disabled="props.readOnly">
        <v-row>
          <v-col cols="12" md="6">
            <v-text-field
              v-model="eventForm.name"
              :label="t('event.form.name')"
              :rules="[rules.required]"
              :readonly="props.readOnly"
            ></v-text-field>
          </v-col>
          <v-col cols="12" md="6">
            <v-select
              v-model="eventForm.type"
              :items="eventTypes"
              :label="t('event.form.type')"
              :rules="[rules.required]"
              :readonly="props.readOnly"
            ></v-select>
          </v-col>
        </v-row>

        <v-row>
          <v-col cols="12">
            <FamilyAutocomplete
              v-model="eventForm.familyId"
              :label="t('event.form.family')"
              :rules="[rules.required]"
              :read-only="props.readOnly"
            />
          </v-col>
        </v-row>

        <v-row>
          <v-col cols="12" md="6">
            <DateInputField
              v-model="eventForm.startDate"
              :label="t('event.form.startDate')"
              :rules="[rules.required]"
              :readonly="props.readOnly"
            />
          </v-col>
          <v-col cols="12" md="6">
            <DateInputField
              v-model="eventForm.endDate"
              :label="t('event.form.endDate')"
              optional
              :readonly="props.readOnly"
            />
          </v-col>
        </v-row>

        <v-row>
          <v-col cols="12">
            <v-text-field
              v-model="eventForm.location"
              :label="t('event.form.location')"
              :readonly="props.readOnly"
            ></v-text-field>
          </v-col>
        </v-row>

        <v-row>
          <v-col cols="12">
            <v-color-picker
              v-model="eventForm.color"
              :label="t('event.form.color')"
              :readonly="props.readOnly"
              hide-canvas
              hide-sliders
              show-swatches
            ></v-color-picker>
          </v-col>
        </v-row>

        <v-row>
          <v-col cols="12">
            <v-textarea
              v-model="eventForm.description"
              :label="t('event.form.description')"
              :readonly="props.readOnly"
            ></v-textarea>
          </v-col>
        </v-row>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="blue-darken-1" variant="text" @click="closeForm">{{ props.readOnly ? t('common.close') : t('common.cancel') }}</v-btn>
      <v-btn v-if="!props.readOnly" color="blue-darken-1" variant="text" @click="submitForm">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
// eslint-disable-next-line @typescript-eslint/no-unused-vars
import type { FamilyEvent } from '@/services/familyEvent.service';
import DateInputField from '@/components/common/DateInputField.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';

const props = defineProps<{
  readOnly?: boolean;
  initialEventData?: Event;
  title: string;
}>();

const emit = defineEmits(['close', 'submit']);

const { t } = useI18n();

const form = ref<HTMLFormElement | null>(null);

const eventForm = ref<Omit<Event, 'id'> | Event>(props.initialEventData || {
  name: '',
  type: 'Other',
  familyId: null,
  startDate: null,
  endDate: null,
  location: '',
  description: '',
  color: '#1976D2',
  relatedMembers: [],
});

const eventTypes = [
  { title: t('event.type.birth'), value: 'Birth' },
  { title: t('event.type.marriage'), value: 'Marriage' },
  { title: t('event.type.death'), value: 'Death' },
  { title: t('event.type.migration'), value: 'Migration' },
  { title: t('event.type.other'), value: 'Other' },
];

const rules = {
  required: (value: string) => !!value || t('validation.required'),
};

onMounted(async () => {
});

const submitForm = async () => {
  if (form.value) {
    const { valid } = await form.value.validate();
    if (valid) {
      emit('submit', eventForm.value);
    }
  }
};

const closeForm = () => {
  emit('close');
};
</script>
