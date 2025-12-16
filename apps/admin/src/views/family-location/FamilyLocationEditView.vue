<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLocation.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoadingFamilyLocation || isUpdatingFamilyLocation" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyLocationForm
        ref="familyLocationFormRef"
        v-if="familyLocation"
        :initial-family-location-data="familyLocation"
        :family-id="familyLocation.familyId"
      />
      <v-alert v-else-if="familyLocationError" type="error" class="mt-4">{{
        familyLocationError.message || t('familyLocation.messages.notFound')
      }}</v-alert>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn
        color="info"
        data-testid="button-select-from-map"
        @click="handleOpenMapPicker"
        :disabled="isLoadingFamilyLocation || isUpdatingFamilyLocation"
      >
        {{ t('familyLocation.form.chooseFromMap') }}
      </v-btn>
      <v-btn color="grey" @click="closeForm" :disabled="isLoadingFamilyLocation || isUpdatingFamilyLocation">{{
        t('common.cancel')
      }}</v-btn>
      <v-btn
        color="primary"
        @click="handleUpdateFamilyLocation"
        data-testid="save-family-location-button"
        :loading="isUpdatingFamilyLocation"
        :disabled="isLoadingFamilyLocation || isUpdatingFamilyLocation"
        >{{ t('common.save') }}</v-btn
      >
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyLocationForm } from '@/components/family-location'; // Assuming index.ts re-exports it
import type { FamilyLocation } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useFamilyLocationQuery, useUpdateFamilyLocationMutation } from '@/composables/family-location';

interface FamilyLocationEditViewProps {
  familyLocationId: string;
}

const props = defineProps<FamilyLocationEditViewProps>();
const emit = defineEmits(['close', 'saved', 'open-map-picker']); // Add open-map-picker emit

const familyLocationFormRef = ref<InstanceType<typeof FamilyLocationForm> | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const familyLocationIdRef = toRef(props, 'familyLocationId');
const {
  data: familyLocation,
  isLoading: isLoadingFamilyLocation,
  error: familyLocationError,
} = useFamilyLocationQuery(familyLocationIdRef);
const { mutate: updateFamilyLocation, isPending: isUpdatingFamilyLocation } = useUpdateFamilyLocationMutation();

const handleUpdateFamilyLocation = async () => {
  if (!familyLocationFormRef.value) return;
  const isValid = await familyLocationFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const familyLocationData = familyLocationFormRef.value.getFormData() as FamilyLocation;
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

const handleOpenMapPicker = () => {
  // Get current coordinates from the form to pre-fill the map picker
  const currentFormData = familyLocationFormRef.value?.getFormData();
  emit('open-map-picker', {
    latitude: currentFormData?.latitude,
    longitude: currentFormData?.longitude,
  });
};

// Function to set coordinates received from the map picker
const setCoordinates = (coords: { latitude: number; longitude: number }) => {
  if (familyLocationFormRef.value) {
    familyLocationFormRef.value.setCoordinates(coords.latitude, coords.longitude);
  }
};

defineExpose({
  setCoordinates, // Expose setCoordinates so parent can call it
});
</script>
