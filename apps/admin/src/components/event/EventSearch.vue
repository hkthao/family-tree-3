<template>
  <v-card :elevation="0" class="mb-4">
    <v-card-title class="text-h6 d-flex align-center pa-0">
      <span data-testid="event-search-title">{{ t('event.search.title') }}</span>
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small" @click="expanded = !expanded" data-testid="event-search-expand-button">
        <v-tooltip :text="expanded ? t('common.collapse') : t('common.expand')">
          <template v-slot:activator="{ props }">
            <v-icon v-bind="props">{{ expanded ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
          </template>
        </v-tooltip>
      </v-btn>
    </v-card-title>
    <v-expand-transition>
      <div v-show="expanded">
        <v-card-text class="pa-0">
          <v-row>
            <v-col cols="12" md="4">
              <v-select v-model="filters.type" :items="eventTypes" :label="t('event.search.type')" clearable
                data-testid="event-search-type-select"></v-select>
            </v-col>

            <v-col cols="12" md="4">
              <v-date-input v-model="filters.startDate" :label="t('event.search.startDate')" optional
                data-testid="event-search-start-date-input" append-inner-icon="mdi-calendar" />
            </v-col>
            <v-col cols="12" md="4">
              <v-date-input v-model="filters.endDate" :label="t('event.search.endDate')" optional
                data-testid="event-search-end-date-input" append-inner-icon="mdi-calendar" />
            </v-col>
            <v-col cols="12" md="4">
              <MemberAutocomplete v-model="filters.memberId" :label="t('event.search.member')" clearable
                :multiple="false" data-testid="event-search-member-autocomplete" />
            </v-col>

          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters" data-testid="event-search-apply-button">{{
            t('event.search.apply')
            }}</v-btn>
          <v-btn @click="resetFilters" data-testid="event-search-reset-button">{{ t('event.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import type { EventFilter } from '@/types';
import { EventType } from '@/types'; // Import EventType enum
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue'; // Import MemberAutocomplete

const emit = defineEmits(['update:filters']);

const { t } = useI18n();

const expanded = ref(false); // Default to collapsed

const filters = ref<Omit<EventFilter, 'searchQuery'>>({
  type: undefined,
  memberId: null,
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
    type: undefined,
    memberId: null, // Change to memberId
    startDate: undefined,
    endDate: undefined,
  };
  emit('update:filters', filters.value);
};


</script>
