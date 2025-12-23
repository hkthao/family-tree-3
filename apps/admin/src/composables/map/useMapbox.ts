import { ref, onMounted, onUnmounted, type Ref } from 'vue';
import { defaultMapboxMapAdapter, type IMapboxMapAdapter } from '@/composables/utils/mapbox.adapter';

interface UseMapboxOptions {
  mapboxAccessToken: string;
  initialCenter?: [number, number]; // [longitude, latitude]
  initialZoom?: number;
  mapContainer: Ref<HTMLElement | null>;
  mapStyle?: string;
  mapboxAdapter?: IMapboxMapAdapter;
}

export function useMapbox(options: UseMapboxOptions) {
  const { mapboxAdapter = defaultMapboxMapAdapter } = options;
  const mapInstance = ref<mapboxgl.Map | null>(null);

  const initializeMap = () => {
    if (!options.mapboxAccessToken) {
      console.error('Mapbox access token is not provided.');
      return;
    }

    if (options.mapContainer.value) {
      mapInstance.value = mapboxAdapter.createMap({
        container: options.mapContainer.value,
        style: options.mapStyle || 'mapbox://styles/mapbox/streets-v11',
        center: options.initialCenter || [106.6297, 10.8231],
        zoom: options.initialZoom || 10,
        accessToken: options.mapboxAccessToken,
      });
    }
  };

  onMounted(() => {
    initializeMap();
  });

  onUnmounted(() => {
    mapInstance.value && mapboxAdapter.removeMap(mapInstance.value as any);
  });

  return {
    state: {
      mapInstance,
    },
  };
}
