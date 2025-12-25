<template>
  <v-card :elevation="0">
    <v-card-title class="text-center text-uppercase">
      {{ t('map.title') }}
    </v-card-title>
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
      <v-btn color="primary" :disabled="!isConfirmEnabled"
        @click="copyCoordinates">
        {{ t('map.copyCoordinates') }}
      </v-btn>
      <v-btn color="success" :disabled="!isConfirmEnabled"
        @click="confirmSelection">
        {{ t('common.confirm') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { useMapPicker } from '@/composables/map/useMapPicker';
import MapPicker from '@/components/map/MapPicker.vue';
import { useI18n } from 'vue-i18n';

const emit = defineEmits(['confirm-selection']);

const { t } = useI18n();
const {
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
} = useMapPicker();

const confirmSelection = () => {
  emit('confirm-selection', {
    coordinates: selectedCoordinates.value,
    location: selectedItem.value ? (selectedItem.value as any).place_name : ''
  });
};
</script>

<style scoped>
.map-container {
  border: 1px solid #ccc;
  border-radius: 4px;
}
</style>