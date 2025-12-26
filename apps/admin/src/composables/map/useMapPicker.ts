import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { getEnvVariable } from '@/utils/api.util';

interface Coordinates {
  latitude: number;
  longitude: number;
}

interface MapboxFeature {
  place_name: string;
  center: [number, number]; // [longitude, latitude]
  // Add other properties if needed
}

interface MapPickerOptions {
  initialLocation?: { latitude: number; longitude: number; address?: string };
}

export function useMapPicker(options?: MapPickerOptions) {
  const { t } = useI18n();
  const { showSnackbar } = useGlobalSnackbar();

  const mapboxAccessToken = ref(getEnvVariable('VITE_MAPBOX_ACCESS_TOKEN'));
  const searchQuery = ref('');
  const selectedCoordinates = ref<Coordinates>(
    options?.initialLocation
      ? { latitude: options.initialLocation.latitude, longitude: options.initialLocation.longitude }
      : { latitude: 0, longitude: 0 },
  );
  const suggestions = ref<MapboxFeature[]>([]);
  const loadingSuggestions = ref(false);
  const selectedItem = ref<MapboxFeature | null>(
    options?.initialLocation?.address
      ? { place_name: options.initialLocation.address, center: [options.initialLocation.longitude, options.initialLocation.latitude] } as MapboxFeature
      : null,
  );

  let abortController: AbortController | null = null;

  const isConfirmEnabled = computed(() => selectedCoordinates.value.latitude !== 0 || selectedCoordinates.value.longitude !== 0);

  const handleCoordinatesUpdate = (coords: Coordinates) => {
    selectedCoordinates.value = coords;
  };

  const fetchSuggestions = async (query: string) => {
    if (abortController) {
      abortController.abort();
    }
    if (!query) {
      suggestions.value = [];
      return;
    }

    loadingSuggestions.value = true;
    abortController = new AbortController();
    const signal = abortController.signal;

    try {
      const response = await fetch(
        `https://api.mapbox.com/geocoding/v5/mapbox.places/${encodeURIComponent(
          query
        )}.json?access_token=${mapboxAccessToken.value}&autocomplete=true`, { signal }
      );
      const data = await response.json();

      if (data.features) {
        suggestions.value = data.features;
      }
    } catch (error: any) {
      if (error.name !== 'AbortError') {
        console.error('Error fetching suggestions:', error);
        showSnackbar(t('map.suggestionFetchError'), 'error');
      }
    } finally {
      loadingSuggestions.value = false;
    }
  };

  const selectSuggestion = (selectedFeature: MapboxFeature | null) => {
    if (!selectedFeature) {
      selectedCoordinates.value = { latitude: 0, longitude: 0 };
      return;
    }
    if (selectedFeature && selectedFeature.center) {
      const [longitude, latitude] = selectedFeature.center;
      selectedCoordinates.value = { latitude, longitude };
      showSnackbar(t('map.searchSuccess'), 'success');
    } else {
      console.warn('selectSuggestion received an invalid feature:', selectedFeature);
      showSnackbar(t('map.searchNoResults'), 'info');
    }
  };

  const copyCoordinates = async () => {
    const coordsText = `${selectedCoordinates.value.latitude}, ${selectedCoordinates.value.longitude}`;
    try {
      await navigator.clipboard.writeText(coordsText);
      showSnackbar(t('map.coordinatesCopied'), 'success');
    } catch (err) {
      console.error('Failed to copy coordinates: ', err);
      showSnackbar(t('map.copyFailed'), 'error');
    }
  };

  return {
    mapboxAccessToken,
    searchQuery,
    selectedCoordinates,
    suggestions,
    loadingSuggestions,
    selectedItem,
    isConfirmEnabled,
    handleCoordinatesUpdate,
    fetchSuggestions,
    selectSuggestion,
    copyCoordinates,
  };
}
