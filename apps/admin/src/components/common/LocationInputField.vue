<template>
  <v-text-field
    :label="label"
    v-bind="$attrs"
    :model-value="modelValue"
    @update:model-value="updateAddress"
    append-inner-icon="mdi-selection-multiple-marker"
    @click:append-inner="openLocationPicker"
    :readonly="true"
    data-testid="location-input-field"
  ></v-text-field>
  <LocationDrawer cssClass="location-drawer" v-model="showDialog" :family-id="familyId"
    @selectLocation="handleLocationSelected" />
</template>

<script setup lang="ts">
import { ref } from 'vue'; // Import ref
import type { FamilyLocation } from '@/types'; // Import FamilyLocation type
import LocationDrawer from './LocationDrawer.vue'; // Import the new LocationDrawer

defineProps<{
  modelValue?: string; // The address string (address)
  locationId?: string | null; // The LocationId
  familyId?: string | null; // Optional familyId for filtering locations
  readOnly?: boolean; // To disable editing and the picker button, no longer affects opening the drawer
  label?: string; // Accept label prop
}>();

const emit = defineEmits(['update:modelValue', 'update:locationId']); // Added update:locationId

const showDialog = ref(false); // Controls the visibility of the LocationDrawer

const updateAddress = (value: string) => {
  emit('update:modelValue', value);
};

const openLocationPicker = () => {
  showDialog.value = true; // Open the drawer always
};

const handleLocationSelected = (location: FamilyLocation) => {
  if (location) {
    if (location.location.address) {
      updateAddress(location.location.address);
    }
    if (location.location.id) {
      emit('update:locationId', location.location.id);
    }
  }
};
</script>

<style>
.location-drawer {
  top: 0 !important;
  height: 100% !important;
}
</style>