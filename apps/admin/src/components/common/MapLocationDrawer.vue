<template>
  <BaseCrudDrawer v-model="mapDrawerStore.drawer"  @close="mapDrawerStore.closeDrawer">
    <FamilyMapPicker v-if="mapDrawerStore.drawer" :initial-location="mapDrawerStore.initialLocation" @confirm-selection="handleConfirmSelection" />
  </BaseCrudDrawer>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import FamilyMapPicker from '@/views/family-location/FamilyMapPicker.vue';
import { useMapLocationDrawerStore } from '@/stores/mapLocationDrawer.store';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';

useI18n();
const mapDrawerStore = useMapLocationDrawerStore();

const handleConfirmSelection = (payload: { coordinates: { latitude: number, longitude: number }, location: string }) => {
  mapDrawerStore.confirmSelection(payload.coordinates, payload.location);
};
</script>