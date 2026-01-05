<template>
  <v-text-field
    :label="label"
    v-bind="$attrs"
    :model-value="modelValue"
    @update:model-value="updateAddress"
    :append-inner-icon="readOnly ? '' : 'mdi-selection-multiple-marker'"
    @click:append-inner="openLocationPicker"
    :readonly="readOnly"
    data-testid="location-input-field"
  ></v-text-field>
  <LocationDialog v-model="showDialog" :family-id="familyId" @selectLocation="handleLocationSelected" />
</template>

<script setup lang="ts">
import { ref } from 'vue'; // Import ref
import type { FamilyLocation } from '@/types'; // Import FamilyLocation type
import LocationDialog from './LocationDialog.vue'; // Import the new LocationDialog

const props = defineProps<{
  modelValue?: string; // The address string
  familyId?: string | null; // Optional familyId for filtering locations
  readOnly?: boolean; // To disable editing and the picker button
  label?: string; // Accept label prop
}>();

const emit = defineEmits(['update:modelValue']);

const showDialog = ref(false); // Controls the visibility of the LocationDialog

const updateAddress = (value: string) => {
  emit('update:modelValue', value);
};

const openLocationPicker = () => {
  if (props.readOnly) return;
  showDialog.value = true; // Open the dialog
};

const handleLocationSelected = (location: FamilyLocation) => {
  if (location && location.location.address) {
    updateAddress(location.location.address);
  }
};
</script>