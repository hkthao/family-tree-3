<template>
  <v-card class="pa-0 picker-container" :elevation="0">
    <v-card-text>
      <v-alert v-if="!mapboxAccessToken" type="warning" prominent class="mb-4">
        {{ t('map.noAccessTokenWarning') }}
      </v-alert>
      <v-autocomplete
        v-model:search="searchQuery"
        v-model="selectedItem"
        :label="t('map.searchLocation')"
        :items="suggestions"
        :loading="loadingSuggestions"
        item-title="place_name"
        return-object
        clearable
        outlined
        hide-no-data
        hide-details
        :placeholder="t('map.search.placeholder')"
        @update:search="fetchSuggestions"
        @update:modelValue="selectSuggestion"
        class="mb-4"
      ></v-autocomplete>
      <MapPicker v-if="mapboxAccessToken" :mapbox-access-token="mapboxAccessToken"
        :initial-center="selectedCoordinates.latitude && selectedCoordinates.longitude ? [selectedCoordinates.longitude, selectedCoordinates.latitude] : undefined"
        @update:coordinates="handleCoordinatesUpdate" />

      <v-row class="mt-4">
        <v-col cols="6">
          <v-text-field v-model="selectedCoordinates.latitude" :label="t('map.latitudeLabel')" readonly></v-text-field>
        </v-col>
        <v-col cols="6">
          <v-text-field v-model="selectedCoordinates.longitude" :label="t('map.longitudeLabel')"
            readonly></v-text-field>
        </v-col>
      </v-row>

    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="success" :disabled="!selectedCoordinates.latitude || !selectedCoordinates.longitude"
        @click="confirmSelection">
        {{ t('common.confirm') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import MapPicker from '@/components/map/MapPicker.vue';
import { useGlobalSnackbar } from '@/composables';
import { getEnvVariable } from '@/utils/api.util';
import { useMobileWebViewMessenger } from '@/composables/utils/useMobileWebViewMessenger'; // Import the new composable

const emit = defineEmits(['confirm-selection']);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const mapboxAccessToken = ref(getEnvVariable('VITE_MAPBOX_ACCESS_TOKEN'));
const searchQuery = ref('');
const searchResultCoordinates = ref<{ latitude: number; longitude: number }>({ latitude: 0, longitude: 0 });
const selectedCoordinates = ref({ latitude: 0, longitude: 0 });
const suggestions = ref<any[]>([]); // To store autocomplete suggestions
const loadingSuggestions = ref(false); // Loading indicator for suggestions
const selectedItem = ref(null);

let abortController: AbortController | null = null; // To cancel previous fetch requests

const { postMapSelectionMessage } = useMobileWebViewMessenger(); // Use the new composable

const handleCoordinatesUpdate = (coords: { longitude: number; latitude: number }) => {
  selectedCoordinates.value = coords;
};

const confirmSelection = () => {
  const data = {
    coordinates: selectedCoordinates.value,
    location: selectedItem.value ? (selectedItem.value as any).place_name : ''
  };

  postMapSelectionMessage(data); // Use the composable function
  emit('confirm-selection', data);
};

const fetchSuggestions = async (query: string) => {
  if (abortController) {
    abortController.abort(); // Cancel previous request
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

const selectSuggestion = (selectedFeature: any) => {
  if (!selectedFeature) {
    selectedCoordinates.value = { latitude: 0, longitude: 0 };
    searchResultCoordinates.value = { latitude: 0, longitude: 0 };
    return;
  }
  if (selectedFeature && selectedFeature.center) {
    const [longitude, latitude] = selectedFeature.center;
    searchResultCoordinates.value = { latitude, longitude };
    selectedCoordinates.value = { latitude, longitude };
    showSnackbar(t('map.searchSuccess'), 'success');
  } else {
    console.warn('selectSuggestion received an invalid feature:', selectedFeature);
    showSnackbar(t('map.searchNoResults'), 'info'); // Or a more specific error
  }
};

</script>

<style scoped>
.map-container {
  border: 1px solid #ccc;
  border-radius: 4px;
  height: calc(100vh - 220px); /* Adjust height as needed */
}
.picker-container{
  height: 100vh;
}
</style>
