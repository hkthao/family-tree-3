<template>
  <v-card class="mb-4">
    <v-card-title class="text-h6 d-flex align-center">
      {{ t('event.search.title') }}
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small" @click="expanded = !expanded">
        <v-tooltip :text="expanded ? t('common.collapse') : t('common.expand')">
          <template v-slot:activator="{ props }">
            <v-icon v-bind="props">{{ expanded ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
          </template>
        </v-tooltip>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="expanded">
        <v-card-text>
          <v-row>
            <v-col cols="12" md="4">
              <v-text-field
                v-model="filters.searchQuery"
                :label="t('event.search.searchQueryPlaceholder')"
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
                v-model="computedFamilyId"
                :label="t('event.search.family')"
                clearable
                :multiple="false"
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

          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters">{{
            t('event.search.apply')
          }}</v-btn>
          <v-btn @click="resetFilters">{{ t('event.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventFilter } from '@/types';
import { EventType } from '@/types'; // Import EventType enum
import { DateInputField, FamilyAutocomplete } from '@/components/common';

const emit = defineEmits(['update:filters']);

const { t } = useI18n();

const expanded = ref(false); // Default to collapsed

const filters = ref<EventFilter>({
  searchQuery: '',
  type: undefined,
  familyId: null, // familyId should be string or null, not undefined
  startDate: undefined,
  endDate: undefined,
});

const eventTypes = [
  { title: t('event.type.birth'), value: EventType.Birth },
  { title: t('event.type.marriage'), value: EventType.Marriage },
  { title: t('event.type.death'), value: EventType.Death },
  { title: t('event.type.migration'), value: EventType.Migration },
  { title: t('event.type.other'), value: EventType.Other },
];

watch(
  filters.value,
  () => {
    // Debounce or apply immediately based on preference
    applyFilters();
  },
  { deep: true },
);

const applyFilters = () => {
  emit('update:filters', filters.value);
};

const resetFilters = () => {
  filters.value = {
    searchQuery: '',
    type: undefined,
    familyId: null, // familyId should be string or null, not undefined
    startDate: undefined,
    endDate: undefined,
  };
  emit('update:filters', filters.value);
};

const computedFamilyId = computed<string | undefined>({
  get: () => filters.value.familyId ?? undefined,
  set: (value: string | undefined) => {
    filters.value.familyId = value ?? null;
  },
});
</script>
