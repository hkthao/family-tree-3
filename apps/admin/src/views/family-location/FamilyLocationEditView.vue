<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLocation.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoadingFamilyLocation || isUpdatingFamilyLocation" indeterminate
      color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyLocationForm ref="familyLocationFormRef" v-if="familyLocation"
        :initial-family-location-data="transformedFamilyLocationData" :family-id="familyLocation.familyId" />
      <v-alert v-else-if="familyLocationError" type="error" class="mt-4">{{
        familyLocationError.message || t('familyLocation.messages.notFound')
      }}</v-alert>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="info" data-testid="button-select-from-map" @click="handleOpenMapPicker"
        :disabled="isLoadingFamilyLocation || isUpdatingFamilyLocation">
        {{ t('familyLocation.form.chooseFromMap') }}
      </v-btn>
      <v-btn color="grey" @click="closeForm" :disabled="isLoadingFamilyLocation || isUpdatingFamilyLocation">{{
        t('common.cancel')
      }}</v-btn>
      <v-btn color="primary" @click="handleUpdateFamilyLocation" data-testid="save-family-location-button"
        :loading="isUpdatingFamilyLocation" :disabled="isLoadingFamilyLocation || isUpdatingFamilyLocation">{{
          t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>


</template>

<script setup lang="ts">
import { ref, toRef, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyLocationForm } from '@/components/family-location';
import type { AddFamilyLocationDto, UpdateFamilyLocationDto } from '@/types'; // Import FamilyLocation and DTOs
import { useGlobalSnackbar } from '@/composables';
import { useFamilyLocationQuery, useUpdateFamilyLocationMutation } from '@/composables';
import { useMapLocationDrawerStore } from '@/stores/mapLocationDrawer.store';

interface FamilyLocationEditViewProps {
  familyLocationId: string;
}

const props = defineProps<FamilyLocationEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const familyLocationFormRef = ref<InstanceType<typeof FamilyLocationForm> | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const mapDrawerStore = useMapLocationDrawerStore();

const familyLocationIdRef = toRef(props, 'familyLocationId');
const {
  data: familyLocation,
  isLoading: isLoadingFamilyLocation,
  error: familyLocationError,
} = useFamilyLocationQuery(familyLocationIdRef);
const { mutate: updateFamilyLocation, isPending: isUpdatingFamilyLocation } = useUpdateFamilyLocationMutation();

// Transform FamilyLocation data to the format expected by FamilyLocationForm
const transformedFamilyLocationData = computed<
  (AddFamilyLocationDto & Partial<UpdateFamilyLocationDto> & { id?: string }) | null
>(() => {
  if (!familyLocation.value) return null;
  return {
    id: familyLocation.value.id,
    familyId: familyLocation.value.familyId,
    locationId: familyLocation.value.locationId,
    locationName: familyLocation.value.location.name,
    locationDescription: familyLocation.value.location.description,
    locationLatitude: familyLocation.value.location.latitude,
    locationLongitude: familyLocation.value.location.longitude,
    locationAddress: familyLocation.value.location.address,
    locationType: familyLocation.value.location.locationType,
    locationAccuracy: familyLocation.value.location.accuracy,
    locationSource: familyLocation.value.location.source,
  };
});

const handleUpdateFamilyLocation = async () => {
  if (!familyLocationFormRef.value) return;
  const isValid = await familyLocationFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const familyLocationData = familyLocationFormRef.value.getFormData() as UpdateFamilyLocationDto; // Ensure correct type for mutation
  if (!familyLocationData.id) {
    showSnackbar(t('familyLocation.messages.saveError'), 'error');
    return;
  }

  updateFamilyLocation(familyLocationData, {
    onSuccess: () => {
      showSnackbar(t('familyLocation.messages.updateSuccess'), 'success');
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