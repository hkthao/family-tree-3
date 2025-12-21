<template>
  <div ref="mapContainer" class="map-container"></div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import mapboxgl from 'mapbox-gl';
import 'mapbox-gl/dist/mapbox-gl.css';
import { useMapbox } from '@/composables/map/useMapbox';

const props = defineProps<{
  mapboxAccessToken: string;
  initialCenter?: [number, number]; // [longitude, latitude]
  initialZoom?: number;
}>();

const emit = defineEmits(['update:coordinates']);

const mapContainer = ref<HTMLElement | null>(null);
const marker = ref<mapboxgl.Marker | null>(null);

const { state: { mapInstance } } = useMapbox({
  mapboxAccessToken: props.mapboxAccessToken,
  initialCenter: props.initialCenter,
  initialZoom: props.initialZoom,
  mapContainer: mapContainer,
});

onMounted(() => {
  watch(mapInstance, (newMapInstance) => {
    if (newMapInstance) {
      newMapInstance.on('click', (e: mapboxgl.MapMouseEvent) => {
        const { lng, lat } = e.lngLat;
        updateMarker([lng, lat]);
        emit('update:coordinates', { longitude: lng, latitude: lat });
      });

      if (props.initialCenter) {
        updateMarker(props.initialCenter);
      }
    }
  }, { immediate: true });
});

const updateMarker = (coordinates: [number, number]) => {
  if (!mapInstance.value) return;
  if (marker.value) {
    marker.value.setLngLat(coordinates);
  } else if (mapInstance.value) { // Add null check here
    marker.value = new mapboxgl.Marker().setLngLat(coordinates).addTo(mapInstance.value as any);
  }
};

watch(() => props.initialCenter, (newCenter) => {
  if (mapInstance.value && newCenter) {
    updateMarker(newCenter);
    mapInstance.value.flyTo({ center: newCenter, zoom: mapInstance.value.getZoom() });
  }
}, { deep: true });

</script>

<style scoped>
.map-container {
  width: 100%;
  height: 500px;
}
</style>
