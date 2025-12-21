<template>
  <div ref="mapContainer" class="map-container"></div>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useMapbox } from '@/composables/map/useMapbox';
import { defaultMapboxMarkerAdapter, defaultMapboxMapAdapter, type IMapboxMarkerAdapter } from '@/composables/utils/mapbox.adapter';

const props = defineProps<{
  mapboxAccessToken: string;
  initialCenter?: [number, number]; // [longitude, latitude]
  initialZoom?: number;
}>();

const emit = defineEmits(['update:coordinates']);

const mapContainer = ref<HTMLElement | null>(null);
const marker = ref<any | null>(null);

const { state: { mapInstance } } = useMapbox({
  mapboxAccessToken: props.mapboxAccessToken,
  initialCenter: props.initialCenter,
  initialZoom: props.initialZoom,
  mapContainer: mapContainer,
});

const updateMarker = (coordinates: [number, number]) => {
  if (!mapInstance.value) return;
  if (marker.value) {
    defaultMapboxMarkerAdapter.setMarkerLngLat(marker.value as any, coordinates);
  } else {
    marker.value = defaultMapboxMarkerAdapter.createMarker();
    defaultMapboxMarkerAdapter.setMarkerLngLat(marker.value as any, coordinates);
    defaultMapboxMarkerAdapter.addMarkerToMap(marker.value as any, mapInstance.value as any);
  }
};

onMounted(() => {
  watch(mapInstance, (newMapInstance) => {
    if (newMapInstance) {
      // Use onMapLoad from the adapter to ensure map is fully loaded before adding click listener
      defaultMapboxMapAdapter.onMapLoad(newMapInstance as any, () => {
        newMapInstance.on('click', (e: any) => {
          const { lng, lat } = e.lngLat;
          updateMarker([lng, lat]);
          emit('update:coordinates', { longitude: lng, latitude: lat });
        });

        if (props.initialCenter) {
          updateMarker(props.initialCenter);
          emit('update:coordinates', { longitude: props.initialCenter[0], latitude: props.initialCenter[1] });
        }
      });
    }
  }, { immediate: true });
});

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
