<template>
  <div ref="mapContainer" class="map-container"></div>
</template>
<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue';
import mapboxgl from 'mapbox-gl';
import 'mapbox-gl/dist/mapbox-gl.css';
import { LocationType } from '@/types/familyLocation.d';
interface MapMarker {
  id: string;
  lng: number;
  lat: number;
  title?: string;
  description?: string;
  locationType: LocationType; // Added locationType
}
const props = defineProps<{
  mapboxAccessToken: string;
  initialCenter?: [number, number]; // [longitude, latitude]
  initialZoom?: number;
  markers?: MapMarker[];
}>();
const mapContainer = ref<HTMLElement | null>(null);
let mapInstance: mapboxgl.Map | null = null;
const currentMarkers = ref<MapMarker[]>(props.markers || []);
const activeMarkers = ref<mapboxgl.Marker[]>([]);
const fitMapToMarkers = (markers: MapMarker[]) => {
  if (!mapInstance || markers.length === 0) {
    return;
  }
  const bounds = new mapboxgl.LngLatBounds();
  markers.forEach(marker => {
    bounds.extend([marker.lng, marker.lat]);
  });
  mapInstance.fitBounds(bounds, {
    padding: 50,
    maxZoom: props.initialZoom || 10,
    duration: 0, // No animation for initial fit
  });
};
const addMarkersToMap = (markers: MapMarker[]) => {
  if (!mapInstance) return;

  activeMarkers.value = markers.map(loc => {
    // Add defensive check for valid numbers
    if (isNaN(loc.lng!) || isNaN(loc.lat!)) {
      console.warn(`Skipping marker with invalid coordinates: ${loc.id}, Lng: ${loc.lng}, Lat: ${loc.lat}`);
      return null; // Return null for invalid markers
    }

    let markerColor = 'gray'; // Default color for unknown types and LocationType.Other

    switch (loc.locationType) {
      case LocationType.Grave:
        markerColor = '#4CAF50'; // Green
        break;
      case LocationType.Homeland:
        markerColor = '#2196F3'; // Blue
        break;
      case LocationType.AncestralHall:
        markerColor = '#FFC107'; // Amber
        break;
      case LocationType.Cemetery:
        markerColor = '#9C27B0'; // Purple
        break;
      case LocationType.EventLocation:
        markerColor = '#FF5722'; // Deep Orange
        break;
      case LocationType.Other:
        markerColor = '#607D8B'; // Blue Gray
        break;
    }

    const marker = new mapboxgl.Marker({
      color: markerColor, // Set marker color based on LocationType
      anchor: 'bottom'
    })
      .setLngLat([loc.lng!, loc.lat!])
      .addTo(mapInstance!);
    
    // Add popup functionality
    if (loc.title || loc.description) {
      const descriptionHtml = `<h3 style="color: #333333;">${loc.title || ''}</h3><p style="color: #333333;">${loc.description || ''}</p>`;
      marker.setPopup(new mapboxgl.Popup().setHTML(descriptionHtml));
    }

    return marker;
  }).filter(Boolean) as mapboxgl.Marker[]; // Filter out any nulls
};
onMounted(() => {
  if (!props.mapboxAccessToken) {
    return;
  }
  mapboxgl.accessToken = props.mapboxAccessToken;
  if (mapContainer.value) {
    mapInstance = new mapboxgl.Map({
      container: mapContainer.value,
      style: 'mapbox://styles/mapbox/streets-v9', // Using v9 for now, will revisit
      center: props.initialCenter || [106.6297, 10.8231], // Default to Ho Chi Minh City
      zoom: props.initialZoom || 10,
    }) as mapboxgl.Map;
    mapInstance.on('load', () => {
      addMarkersToMap(currentMarkers.value);
      if (currentMarkers.value.length > 0) {
        fitMapToMarkers(currentMarkers.value);
      }
    });
  }
});
onUnmounted(() => {
  mapInstance?.remove();
  activeMarkers.value.forEach(marker => marker.remove());
});
watch(() => props.markers, (newMarkers) => {
  currentMarkers.value = newMarkers || [];
  // Remove existing markers
  activeMarkers.value.forEach(marker => marker.remove());
  activeMarkers.value = [];
  // Add new markers
  addMarkersToMap(currentMarkers.value);
  // Fit map to new markers
  if (currentMarkers.value.length > 0) {
    if (mapInstance && mapInstance.isStyleLoaded()) {
      fitMapToMarkers(currentMarkers.value);
    } else {
      // If map is not loaded yet, fit after load
      mapInstance?.on('load', () => {
        fitMapToMarkers(currentMarkers.value);
      });
    }
  }
}, { deep: true, immediate: true });</script>
<style scoped>
.map-container {
  width: 100%;
  height: 500px;
  /* Adjust height as needed */
}
</style>