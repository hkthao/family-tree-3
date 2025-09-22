<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('event.search.title') }}
      <v-spacer></v-spacer>
      <v-btn icon size="small" variant="text" @click="expanded = !expanded">
        <v-icon>{{ expanded ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="expanded">
        <v-card-text>
          <v-row>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="filters.name"
                :label="t('event.search.name')"
                clearable
                prepend-inner-icon="mdi-magnify"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="4">
              <v-select
                v-model="filters.type"
                :items="eventTypes"
                :label="t('event.search.type')"
                clearable
              ></v-select>
            </v-col>
            <v-col cols="12" md="4">
              <FamilyAutocomplete
                v-model="filters.familyId"
                :label="t('event.search.family')"
                clearable
              />
            </v-col>
            <v-col cols="12" md="4">
              <DateInputField
                v-model="filters.startDate"
                :label="t('event.search.startDate')"
                optional
              />
            </v-col>
            <v-col cols="12" md="4">
              <DateInputField
                v-model="filters.endDate"
                :label="t('event.search.endDate')"
                optional
              />
            </v-col>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="filters.location"
                :label="t('event.search.location')"
                clearable
              ></v-text-field>
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters">{{ t('event.search.apply') }}</v-btn>
          <v-btn @click="resetFilters">{{ t('event.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventFilter } from '@/services/family-event/family-event.service.interface';
import DateInputField from '@/components/common/DateInputField.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';

const emit = defineEmits(['update:filters']);

const { t } = useI18n();

const expanded = ref(false); // Default to collapsed

const filters = ref<EventFilter>({
  name: '',
  type: undefined,
  familyId: null, // familyId should be string or null, not undefined
  startDate: undefined,
  endDate: undefined,
  location: '',
});

const eventTypes = [
  { title: t('event.type.birth'), value: 'Birth' },
  { title: t('event.type.marriage'), value: 'Marriage' },
  { title: t('event.type.death'), value: 'Death' },
  { title: t('event.type.migration'), value: 'Migration' },
  { title: t('event.type.other'), value: 'Other' },
];

watch(filters.value, () => {
  // Debounce or apply immediately based on preference
  applyFilters();
}, { deep: true });

const applyFilters = () => {
  emit('update:filters', filters.value);
};

const resetFilters = () => {
  filters.value = {
    name: '',
    type: undefined,
    familyId: null, // familyId should be string or null, not undefined
    startDate: undefined,
    endDate: undefined,
    location: '',
  };
  emit('update:filters', filters.value);
};
</script>