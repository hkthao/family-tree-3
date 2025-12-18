<template>
  <div class="mobile-map-wrapper">
    <MultiMarkerMap
      :mapbox-access-token="mapboxAccessToken"
      :markers="mapMarkers"
      :initial-center="[0, 0]"
      :initial-zoom="1"
      map-style="mapbox://styles/mapbox/streets-v11"
    />
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { getEnvVariable } from '@/utils/api.util';
import MultiMarkerMap from '@/components/map/MultiMarkerMap.vue';
import type { MapMarker } from '@/composables/map/useMultiMarkers';
import { LocationType } from '@/types/familyLocation.d';

const mapboxAccessToken = getEnvVariable('VITE_MAPBOX_ACCESS_TOKEN') as string;
const mapMarkers = ref<MapMarker[]>([]);

interface FamilyLocationData {
  id: string;
  latitude: number;
  longitude: number;
  name: string;
  description?: string;
  locationType?: LocationType; // Add locationType
}

onMounted(() => {
  // Check if data is available on the window object
  if ((window as any).familyLocationsData) {
    const locations: FamilyLocationData[] = (window as any).familyLocationsData;
    mapMarkers.value = locations.map(loc => ({
      id: loc.id,
      lng: loc.longitude,
      lat: loc.latitude,
      title: loc.name,
      description: loc.description,
      locationType: loc.locationType || LocationType.Other,
    }));
  } else {
    console.warn('No familyLocationsData found on window object.');
  }
});
</script>

<style scoped>
.mobile-map-wrapper {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  overflow: hidden; /* Ensure no scrollbars */
}
/* MultiMarkerMap's container has a fixed height, override it */
.mobile-map-wrapper :deep(.map-container) {
  width: 100%;
  height: 100%;
}
</style>