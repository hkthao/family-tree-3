<template>
  <v-card class="mb-4" elevation="0" data-testid="family-location-search">
    <v-card-title class="text-h6 d-flex align-center pa-0">
      {{ t('familyLocation.search.title') }}
      <v-spacer></v-spacer>
      <v-btn variant="text" icon size="small" @click="expanded = !expanded"
        data-testid="family-location-search-expand-button">
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
              <v-select
                v-model="filters.locationType"
                :items="locationTypeOptions"
                :label="t('familyLocation.form.locationType')"
                clearable
                hide-details
                single-line
              ></v-select>
            </v-col>
            <v-col cols="12" md="4">
              <v-select
                v-model="filters.locationSource"
                :items="locationSourceOptions"
                :label="t('familyLocation.form.source')"
                clearable
                hide-details
                single-line
              ></v-select>
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="applyFilters" data-testid="apply-filters-button">{{
            t('familyLocation.search.apply')
          }}</v-btn>
          <v-btn @click="resetFilters" data-testid="reset-filters-button">{{ t('familyLocation.search.reset') }}</v-btn>
        </v-card-actions>
      </div>
    </v-expand-transition>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { LocationType, LocationSource } from '@/types';
import type { FamilyLocationSearchCriteria } from '@/composables';

const emit = defineEmits<{
  (e: 'update:filters', value: FamilyLocationSearchCriteria): void;
}>();

const { t } = useI18n();

const expanded = ref(false);

const filters = ref<FamilyLocationSearchCriteria>({
  locationType: undefined,
  locationSource: undefined,
});

const locationTypeOptions = computed(() => {
  return Object.keys(LocationType)
    .filter((key) => isNaN(Number(key)))
    .map((type) => ({
      title: t(`familyLocation.locationType.${String(type)}`),
      value: LocationType[type as keyof typeof LocationType],
    }));
});

const locationSourceOptions = computed(() => {
  return Object.keys(LocationSource)
    .filter((key) => isNaN(Number(key)))
    .map((source) => ({
      title: t(`familyLocation.source.${String(source)}`),
      value: LocationSource[source as keyof typeof LocationSource],
    }));
});

const applyFilters = () => {
  emit('update:filters', filters.value);
};

const resetFilters = () => {
  filters.value = {
    locationType: undefined,
    locationSource: undefined,
  };
  emit('update:filters', filters.value);
};

</script>