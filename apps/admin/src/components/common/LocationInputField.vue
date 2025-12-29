<template>
  <v-text-field
    :model-value="modelValue"
    @update:model-value="updateAddress"
    :label="$t('family.form.address')"
    :append-inner-icon="readOnly ? '' : 'mdi-map-marker'"
    @click:append-inner="openLocationPicker"
    :readonly="readOnly"
    data-testid="location-input-field"
  ></v-text-field>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { useLocationDrawerStore } from '@/stores/locationDrawer.store';
import type { FamilyLocation } from '@/types'; // Import FamilyLocation type

const props = defineProps<{
  modelValue?: string; // The address string
  familyId?: string | null; // Optional familyId for filtering locations
  readOnly?: boolean; // To disable editing and the picker button
}>();

const emit = defineEmits(['update:modelValue']);

const { t } = useI18n();
const locationDrawerStore = useLocationDrawerStore();

const updateAddress = (value: string) => {
  emit('update:modelValue', value);
};

const openLocationPicker = async () => {
  if (props.readOnly) return;
  try {
    const selectedLocation: FamilyLocation = await locationDrawerStore.openDrawer(props.familyId);
    if (selectedLocation && selectedLocation.address) {
      updateAddress(selectedLocation.address);
    }
  } catch (error) {
    console.error('Location selection cancelled or failed:', error);
  }
};
</script>