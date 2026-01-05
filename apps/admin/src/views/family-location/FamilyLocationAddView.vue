<template>
  <v-card :elevation="0" data-testid="family-location-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLocation.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isAddingFamilyLocation" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyLocationForm
        ref="familyLocationFormRef"
        :family-id="props.familyId"
        :initial-family-location-data="transformedInitialLocationData"
      />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="info" data-testid="button-select-from-map" @click="handleOpenMapPicker"
        :disabled="isAddingFamilyLocation">
        {{ t('familyLocation.form.chooseFromMap') }}
      </v-btn>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm" :disabled="isAddingFamilyLocation">{{
        t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleAddFamilyLocation" data-testid="save-family-location-button"
        :loading="isAddingFamilyLocation" :disabled="isAddingFamilyLocation">{{
          t('common.save') }}</v-btn>
    </v-card-actions>
</v-card>

</template>
<script setup lang="ts">
import { ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyLocationForm } from '@/components/family-location';
import type { FamilyLocation, AddFamilyLocationDto, UpdateFamilyLocationDto } from '@/types'; // Import FamilyLocation and DTOs
import { useGlobalSnackbar } from '@/composables';
import { useAddFamilyLocationMutation } from '@/composables';
import { useMapLocationDrawerStore } from '@/stores/mapLocationDrawer.store';

interface FamilyLocationAddViewProps {
  familyId: string;
  initialLocationData?: FamilyLocation | null; // Add initialLocationData prop
}
const props = defineProps<FamilyLocationAddViewProps>();
const emit = defineEmits(['close', 'saved']);

const familyLocationFormRef = ref<InstanceType<typeof FamilyLocationForm> | null>(null);
const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const mapDrawerStore = useMapLocationDrawerStore();

const { mutate: addFamilyLocation, isPending: isAddingFamilyLocation } = useAddFamilyLocationMutation();

// Transform initialLocationData to the format expected by FamilyLocationForm
const transformedInitialLocationData = computed<
  (AddFamilyLocationDto & Partial<UpdateFamilyLocationDto> & { id?: string }) | null
>(() => {
  if (!props.initialLocationData) return null;
  return {
    id: props.initialLocationData.id,
    familyId: props.initialLocationData.familyId,
    locationId: props.initialLocationData.locationId,
    locationName: props.initialLocationData.location.name,
    locationDescription: props.initialLocationData.location.description,
    locationLatitude: props.initialLocationData.location.latitude,
    locationLongitude: props.initialLocationData.location.longitude,
    locationAddress: props.initialLocationData.location.address,
    locationType: props.initialLocationData.location.locationType,
    locationAccuracy: props.initialLocationData.location.accuracy,
    locationSource: props.initialLocationData.location.source,
  };
});

const handleAddFamilyLocation = async () => {
  if (!familyLocationFormRef.value) return;
  const isValid = await familyLocationFormRef.value.validate();
  if (!isValid) {
    return;
  }
  const familyLocationData = familyLocationFormRef.value.getFormData() as AddFamilyLocationDto; // Ensure correct type for mutation
  familyLocationData.familyId = props.familyId;

  addFamilyLocation(familyLocationData, {
    onSuccess: () => {
      showSnackbar(t('familyLocation.messages.addSuccess'), 'success');
      emit('saved');
    },
    onError: (error) => {
      showSnackbar(error.message || t('familyLocation.messages.saveError'), 'error');
    },
  });
};

const closeForm = () => {
  emit('close');
};

const handleOpenMapPicker = async () => {
  try {
    const result = await mapDrawerStore.openDrawer(); // Open global map picker
    if (result.coordinates && familyLocationFormRef.value) {
      familyLocationFormRef.value.setCoordinates(result.coordinates.latitude, result.coordinates.longitude);
      familyLocationFormRef.value.setAddress(result.location);
    }
  } catch (error) {
    console.error('Map location selection cancelled or failed:', error);
  }
};


</script>