<template>
  <div class="map-wrapper">
    <div ref="mapContainer" class="map-container"></div>
    <v-autocomplete v-model="selectedMarker" :items="markerItems" item-title="title" item-value="value"
      placeholder="Gõ để tìm địa điểm..." clearable hide-details variant="solo" class="marker-search-autocomplete">
      <template v-slot:item="{ props, item }">
        <v-list-item v-bind="props" :title="item.raw.title" :subtitle="item.raw.subtitle"></v-list-item>
      </template>
    </v-autocomplete>
  </div>
</template>
<script setup lang="ts">
import { ref, onUnmounted, watch, computed } from 'vue';
import { useMapbox } from '@/composables/map/useMapbox';
import { useMultiMarkers, type MapMarker } from '@/composables/map/useMultiMarkers';
import { VAutocomplete } from 'vuetify/components'; // Import VAutocomplete

const props = defineProps<{
  mapboxAccessToken: string;
  initialCenter?: [number, number]; // [longitude, latitude]
  initialZoom?: number;
  markers?: MapMarker[];
}>();

const mapContainer = ref<HTMLElement | null>(null);
const selectedMarker = ref<MapMarker | null>(null);

const { state: { mapInstance } } = useMapbox({
  mapboxAccessToken: props.mapboxAccessToken,
  initialCenter: props.initialCenter,
  initialZoom: props.initialZoom,
  mapContainer: mapContainer,
  mapStyle: 'mapbox://styles/mapbox/streets-v9', // Using v9 for now, will revisit
});

const markersRef = ref(props.markers);
watch(() => props.markers, (newMarkers) => {
  markersRef.value = newMarkers;
}, { deep: true });

useMultiMarkers({
  mapInstance: mapInstance as any,
  markers: markersRef,
  initialZoom: props.initialZoom,
});

// Computed property to transform markers for v-autocomplete
const markerItems = computed(() => {
  return (props.markers || []).map(marker => ({
    title: marker.title || 'Địa điểm không tên',
    value: marker,
    subtitle: marker.address || marker.description, // Use address or description as subtitle
  }));
});

// Watch for selected marker changes and move map
watch(selectedMarker, (newValue) => {
  if (newValue && mapInstance.value) {
    mapInstance.value.flyTo({
      center: [newValue.lng, newValue.lat],
      zoom: props.initialZoom || 15, // Zoom in a bit when selecting a marker
      essential: true, // This animation is considered essential
    });
  }
});

onUnmounted(() => {
  // mapInstance is removed by useMapbox
  // activeMarkers are removed by useMultiMarkers
});
</script>
<style scoped>
.map-wrapper {
  position: relative;
  /* Establish positioning context for absolute children */
  width: 100%;
  height: 100%;

  /* Or adjust based on parent, e.g., inherit */
}

.map-container {
  width: 100%;
  min-height: 500px;
  height: 100%;
  /* Take full height of wrapper */
}

.marker-search-autocomplete {
  position: absolute;
  top: 10px;
  right: 10px;
  min-width: 320px;
  z-index: 1000;
  border-radius: 4px;
}
</style>
