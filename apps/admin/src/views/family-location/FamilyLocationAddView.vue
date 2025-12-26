<template>
  <v-card :elevation="0" data-testid="family-location-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLocation.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isAddingFamilyLocation" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyLocationForm ref="familyLocationFormRef" :family-id="props.familyId" :initial-family-location-data="props.initialLocationData" />
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
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyLocationForm } from '@/components/family-location';
import type { FamilyLocation } from '@/types';
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