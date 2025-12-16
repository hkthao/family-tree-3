<template>
  <div ref="mapContainer" class="map-container"></div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue';
import mapboxgl from 'mapbox-gl';
import 'mapbox-gl/dist/mapbox-gl.css';

const props = defineProps<{
  mapboxAccessToken: string;
  initialCenter?: [number, number]; // [longitude, latitude]
  initialZoom?: number;
}>();

const emit = defineEmits(['update:coordinates']);

const mapContainer = ref<HTMLElement | null>(null);
let mapInstance: mapboxgl.Map | null = null; // Use a plain variable for the map instance
const marker = ref<mapboxgl.Marker | null>(null);

onMounted(() => {
  if (!props.mapboxAccessToken) {
    console.error('Mapbox access token is not provided.');
    return;
  }
  mapboxgl.accessToken = props.mapboxAccessToken;

  if (mapContainer.value) {
    mapInstance = new mapboxgl.Map({ // Assign to mapInstance
      container: mapContainer.value,
      style: 'mapbox://styles/mapbox/streets-v11',
      center: props.initialCenter || [106.6297, 10.8231],
      zoom: props.initialZoom || 10,
    });

    mapInstance.on('load', () => { // Use mapInstance directly
      mapInstance?.on('click', (e) => { // Use mapInstance directly
        const { lng, lat } = e.lngLat;
        updateMarker([lng, lat]);
        emit('update:coordinates', { longitude: lng, latitude: lat });
      });

      if (props.initialCenter) {
        updateMarker(props.initialCenter);
      }
    });
  }
});

onUnmounted(() => {
  mapInstance?.remove(); // Use mapInstance
});

const updateMarker = (coordinates: [number, number]) => {
  if (!mapInstance) return; // Ensure mapInstance is available
  if (marker.value) {
    marker.value.setLngLat(coordinates);
  } else {
    marker.value = new mapboxgl.Marker().setLngLat(coordinates).addTo(mapInstance); // Use mapInstance
  }
};

watch(() => props.initialCenter, (newCenter) => {
  if (mapInstance && newCenter) { // Use mapInstance
    updateMarker(newCenter);
    mapInstance.flyTo({ center: newCenter, zoom: mapInstance.getZoom() }); // Use mapInstance
  }
}, { deep: true });

</script>

<style scoped>
.map-container {
  width: 100%;
  height: 500px;
}
</style>