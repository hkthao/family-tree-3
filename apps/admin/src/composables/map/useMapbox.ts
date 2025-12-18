import { ref, onMounted, onUnmounted, watch, type Ref } from 'vue';
import mapboxgl from 'mapbox-gl';
import 'mapbox-gl/dist/mapbox-gl.css';

interface UseMapboxOptions {
  mapboxAccessToken: string;
  initialCenter?: [number, number]; // [longitude, latitude]
  initialZoom?: number;
  mapContainer: Ref<HTMLElement | null>;
  mapStyle?: string;
}

export function useMapbox(options: UseMapboxOptions) {
  const mapInstance = ref<mapboxgl.Map | null>(null);

  onMounted(() => {
    if (!options.mapboxAccessToken) {
      console.error('Mapbox access token is not provided.');
      return;
    }
    mapboxgl.accessToken = options.mapboxAccessToken;

    if (options.mapContainer.value) {
      mapInstance.value = new mapboxgl.Map({
        container: options.mapContainer.value,
        style: options.mapStyle || 'mapbox://styles/mapbox/streets-v11',
        center: options.initialCenter || [106.6297, 10.8231],
        zoom: options.initialZoom || 10,
      });
    }
  });

  onUnmounted(() => {
    mapInstance.value?.remove();
  });

  return {
    mapInstance,
  };
}
