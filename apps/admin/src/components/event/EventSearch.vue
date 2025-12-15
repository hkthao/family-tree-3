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
              <v-select v-model="filters.calendarType" :items="calendarTypes" :label="t('event.search.calendarType')"
                clearable data-testid="event-search-calendar-type-select"></v-select>
            </v-col>

            <v-col cols="12" md="4">
              <v-date-input v-model="filters.minSolarDate" :label="t('event.search.minSolarDate')" optional
                data-testid="event-search-min-solar-date-input" />
            </v-col>
            <v-col cols="12" md="4">
              <v-date-input v-model="filters.maxSolarDate" :label="t('event.search.maxSolarDate')" optional
                data-testid="event-search-max-solar-date-input" />
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
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue'; // Import MemberAutocomplete
import { useEventSearch } from '@/composables/event/useEventSearch';
import VDateInput from '@/components/common/VDateInput.vue'; // Assuming you have a custom date input


const emit = defineEmits(['update:filters']);

const {
  t,
  expanded,
  filters,
  eventTypes,
  calendarTypes, // Expose calendarTypes
  applyFilters,
  resetFilters,
} = useEventSearch(emit);
</script>