<template>
  <v-card :elevation="0">
    <v-card-title class="text-center text-uppercase">
      {{ t('map.title') }}
    </v-card-title>
    <v-card-text>
      <v-alert v-if="!mapboxAccessToken" type="warning" prominent class="mb-4">
        {{ t('map.noAccessTokenWarning') }}
      </v-alert>
      <v-text-field v-model="searchQuery" :label="t('map.searchLocation')" outlined clearable
        @keydown.enter="searchLocation" class="mb-4"></v-text-field>
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
      <v-btn color="primary" :disabled="!selectedCoordinates.latitude || !selectedCoordinates.longitude"
        @click="copyCoordinates">
        {{ t('map.copyCoordinates') }}
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

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const mapboxAccessToken = ref(getEnvVariable('VITE_MAPBOX_ACCESS_TOKEN'));
const searchQuery = ref('');
const searchResultCoordinates = ref<{ latitude: number; longitude: number }>({ latitude: 0, longitude: 0 });
const selectedCoordinates = ref({ latitude: 0, longitude: 0 });

const handleCoordinatesUpdate = (coords: { longitude: number; latitude: number }) => {
  selectedCoordinates.value = coords;
};

const searchLocation = async () => {
  if (!searchQuery.value) {
    showSnackbar(t('map.searchQueryEmpty'), 'warning');
    return;
  }
  if (!mapboxAccessToken.value) {
    showSnackbar(t('map.noAccessTokenWarning'), 'error');
    return;
  }

  try {
    const response = await fetch(
      `https://api.mapbox.com/geocoding/v5/mapbox.places/${encodeURIComponent(
        searchQuery.value
      )}.json?access_token=${mapboxAccessToken.value}`
    );
    const data = await response.json();

    if (data.features && data.features.length > 0) {
      const [longitude, latitude] = data.features[0].center;
      searchResultCoordinates.value = { latitude, longitude };
      selectedCoordinates.value = { latitude, longitude }; // Update selectedCoordinates as well
      showSnackbar(t('map.searchSuccess'), 'success');
    } else {
      showSnackbar(t('map.searchNoResults'), 'info');
    }
  } catch (error) {
    console.error('Error searching for location:', error);
    showSnackbar(t('map.searchError'), 'error');
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

</script>

<style scoped>
.map-container {
  border: 1px solid #ccc;
  border-radius: 4px;
}
</style>