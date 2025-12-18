<template>
  <div ref="mapContainer" class="map-container"></div>
</template>
<script setup lang="ts">
import { ref, onUnmounted, watch, type Ref } from 'vue';
import 'mapbox-gl/dist/mapbox-gl.css';
import { useMapbox } from '@/composables/map/useMapbox';
import { useMultiMarkers, type MapMarker } from '@/composables/map/useMultiMarkers';

const props = defineProps<{
  mapboxAccessToken: string;
  initialCenter?: [number, number]; // [longitude, latitude]
  initialZoom?: number;
  markers?: MapMarker[];
}>();

const mapContainer = ref<HTMLElement | null>(null);

const { mapInstance } = useMapbox({
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
  mapInstance: mapInstance as Ref<mapboxgl.Map | null>,
  markers: markersRef,
  initialZoom: props.initialZoom,
});

onUnmounted(() => {
  // mapInstance is removed by useMapbox
  // activeMarkers are removed by useMultiMarkers
});
</script>
<style scoped>
.map-container {
  width: 100%;
  height: 500px;
  /* Adjust height as needed */
}
</style>
