<template>
  <v-card :elevation="0">
    <v-progress-linear v-if="isLoadingFamilyLocations" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <v-alert v-if="!mapboxAccessToken" type="warning" prominent class="mb-4">
        {{ t('map.noAccessTokenWarning') }}
      </v-alert>
      <div v-if="familyLocations && familyLocations.length > 0">
        <MultiMarkerMap
          v-if="mapboxAccessToken"
          :mapbox-access-token="mapboxAccessToken"
          :markers="mapMarkers"
        />
      </div>
      <v-alert v-else type="info" class="mt-4">
        {{ t('familyLocation.list.noData') }}
      </v-alert>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import MultiMarkerMap from '@/components/map/MultiMarkerMap.vue';
import { useGlobalSnackbar } from '@/composables';
import { getEnvVariable } from '@/utils/api.util';
import { useFamilyLocationsQuery } from '@/composables';
import type { FamilyLocation, ListOptions, FamilyLocationFilter } from '@/types';

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const mapboxAccessToken = ref(getEnvVariable('VITE_MAPBOX_ACCESS_TOKEN'));

const paginationOptions = ref<ListOptions>({
  page: 1,
  itemsPerPage: 100, // Load up to 100 locations
  sortBy: [],
});

const filters = ref<FamilyLocationFilter>({}); // No specific filters for all locations

const { data: familyLocationsData, isLoading: isLoadingFamilyLocations, error: familyLocationsError } = useFamilyLocationsQuery(paginationOptions, filters);

const familyLocations = computed<FamilyLocation[]>(() => {
  const locations = familyLocationsData.value?.items || [];
  return locations;
});

const mapMarkers = computed(() => {
  const markers = familyLocations.value
    .filter(loc => loc.latitude && loc.longitude)
    .map(loc => ({
      id: loc.id,
      lng: loc.longitude!,
      lat: loc.latitude!,
      title: loc.name,
      description: loc.description,
      locationType: loc.locationType, // Added locationType
    }));
  return markers;
});

watch(familyLocationsError, (newError) => {
  if (newError) {
    showSnackbar(newError.message || t('familyLocation.messages.loadError'), 'error');
  }
});
</script>

<style scoped>
.map-container {
  height: 600px; /* Adjust height as needed */
  width: 100%;
}
</style>