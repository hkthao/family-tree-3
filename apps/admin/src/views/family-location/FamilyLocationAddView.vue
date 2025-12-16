<template>
  <v-card :elevation="0" data-testid="family-location-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLocation.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isAddingFamilyLocation" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyLocationForm ref="familyLocationFormRef" :family-id="props.familyId" />
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

    <!-- Map Picker Drawer -->
    <BaseCrudDrawer class="map-drawer" v-model="mapDrawer" :width="700" :hide-overlay="false" :location="'right'"
      @close="closeMapDrawer">
      <FamilyMapPicker v-if="mapDrawer"
        :initial-center="initialMapCoordinates.latitude && initialMapCoordinates.longitude ? [initialMapCoordinates.longitude, initialMapCoordinates.latitude] : undefined"
        @confirm-selection="handleMapCoordinatesSelected" @close="closeMapDrawer" />
    </BaseCrudDrawer>
  </v-card>

</template>
<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyLocationForm } from '@/components/family-location';
import type { FamilyLocation } from '@/types';
import { useGlobalSnackbar, useCrudDrawer } from '@/composables';
import { useAddFamilyLocationMutation } from '@/composables/family-location';
import FamilyMapPicker from './FamilyMapPicker.vue';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';

interface FamilyLocationAddViewProps {
  familyId: string;
}
const props = defineProps<FamilyLocationAddViewProps>();
const emit = defineEmits(['close', 'saved']);

const familyLocationFormRef = ref<InstanceType<typeof FamilyLocationForm> | null>(null);
const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const { mutate: addFamilyLocation, isPending: isAddingFamilyLocation } = useAddFamilyLocationMutation();

// Map Drawer related logic
const {
  addDrawer: mapDrawer, // Use alias for map drawer
  openAddDrawer: openMapDrawer,
  closeAllDrawers: closeMapDrawer,
} = useCrudDrawer<string>();

const initialMapCoordinates = ref<{ latitude?: number; longitude?: number }>({});

const handleAddFamilyLocation = async () => {
  if (!familyLocationFormRef.value) return;
  const isValid = await familyLocationFormRef.value.validate();
  if (!isValid) {
    return;
  }
  const familyLocationData = familyLocationFormRef.value.getFormData();
  familyLocationData.familyId = props.familyId;

  addFamilyLocation(familyLocationData as Omit<FamilyLocation, 'id'>, {
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

const handleOpenMapPicker = () => {
  const currentFormData = familyLocationFormRef.value?.getFormData();
  initialMapCoordinates.value = {
    latitude: currentFormData?.latitude,
    longitude: currentFormData?.longitude,
  };
  openMapDrawer();
};

const handleMapCoordinatesSelected = (payload: { coordinates: { latitude: number; longitude: number; }, location: string }) => {
  if (familyLocationFormRef.value) {
    familyLocationFormRef.value.setCoordinates(payload.coordinates.latitude, payload.coordinates.longitude);
    familyLocationFormRef.value.setAddress(payload.location);
  }
  closeMapDrawer();
};


</script>
<style>
.map-drawer {
  top: 0px !important;
  bottom: 0px !important;
  height: auto !important;
}
</style>