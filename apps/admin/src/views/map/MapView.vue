<template>
  <v-container fluid>
    <v-card>
      <v-card-title>
        <span class="text-h5">{{ t('map.title') }}</span>
      </v-card-title>
      <v-card-text>
        <v-alert v-if="!mapboxAccessToken" type="warning" prominent class="mb-4">
          {{ t('map.noAccessTokenWarning') }}
        </v-alert>
        <MapPicker v-if="mapboxAccessToken" :mapbox-access-token="mapboxAccessToken"
          :initial-center="selectedCoordinates.latitude && selectedCoordinates.longitude ? [selectedCoordinates.longitude, selectedCoordinates.latitude] : undefined"
          @update:coordinates="handleCoordinatesUpdate" />
        <v-text-field v-model="selectedCoordinates.latitude" :label="t('map.latitudeLabel')" readonly
          class="mt-4"></v-text-field>
        <v-text-field v-model="selectedCoordinates.longitude" :label="t('map.longitudeLabel')" readonly
          class="mt-2"></v-text-field>
      </v-card-text>
      <v-card-actions>
        <v-btn color="primary" :disabled="!selectedCoordinates.latitude || !selectedCoordinates.longitude"
          @click="copyCoordinates">
          {{ t('map.copyCoordinates') }}
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import MapPicker from '@/components/map/MapPicker.vue';
import { useGlobalSnackbar } from '@/composables';
import { getEnvVariable } from '@/utils/api.util';

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

// TODO: Replace with your actual Mapbox Access Token.
// You can get one from https://account.mapbox.com/access-tokens/
// For production, consider storing this securely (e.g., environment variable).
const mapboxAccessToken = ref(getEnvVariable('VITE_MAPBOX_ACCESS_TOKEN'));
const selectedCoordinates = ref({ latitude: 0, longitude: 0 });

const handleCoordinatesUpdate = (coords: { longitude: number; latitude: number }) => {
  selectedCoordinates.value = coords;
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