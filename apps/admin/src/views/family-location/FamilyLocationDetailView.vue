<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLocation.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoadingFamilyLocation || isDeletingFamilyLocation" indeterminate
      color="primary"></v-progress-linear>
    <v-card-text>
      <div v-if="isLoadingFamilyLocation">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="familyLocationError">
        <v-alert type="error" :text="familyLocationError.message || t('familyLocation.detail.notFound')"></v-alert>
      </div>
      <div v-else-if="familyLocation">
        <PrivacyAlert :is-private="familyLocation.isPrivate" />
        <FamilyLocationForm :initial-family-location-data="transformedFamilyLocationData"
          :family-id="familyLocation.familyId" :read-only="true" />
      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose" :disabled="isLoadingFamilyLocation || isDeletingFamilyLocation">{{
        t('common.close')
      }}</v-btn>
      <v-btn color="primary" @click="handleEdit"
        :disabled="!familyLocation || isLoadingFamilyLocation || isDeletingFamilyLocation" v-if="canEditOrDelete">{{
          t('common.edit') }}</v-btn>
      <v-btn color="error" @click="handleDelete"
        :disabled="!familyLocation || isLoadingFamilyLocation || isDeletingFamilyLocation" v-if="canEditOrDelete">{{
          t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { computed, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyLocationForm } from '@/components/family-location';
import { useConfirmDialog, useAuth, useGlobalSnackbar } from '@/composables';
import { useFamilyLocationQuery, useDeleteFamilyLocationMutation } from '@/composables';
import PrivacyAlert from '@/components/common/PrivacyAlert.vue'; // Import PrivacyAlert
import type { AddFamilyLocationDto, UpdateFamilyLocationDto } from '@/types'; // Import DTOs

interface FamilyLocationDetailViewProps {
  familyLocationId: string;
}

const props = defineProps<FamilyLocationDetailViewProps>();
const emit = defineEmits(['close', 'family-location-deleted', 'edit-family-location']);

const { t } = useI18n();
const { showConfirmDialog } = useConfirmDialog();
const { state } = useAuth();
const { showSnackbar } = useGlobalSnackbar();

const familyLocationIdRef = toRef(props, 'familyLocationId');
const {
  data: familyLocation,
  isLoading: isLoadingFamilyLocation,
  error: familyLocationError,
} = useFamilyLocationQuery(familyLocationIdRef);
const { mutate: deleteFamilyLocation, isPending: isDeletingFamilyLocation } = useDeleteFamilyLocationMutation();

const canEditOrDelete = computed(() => {
  return state.isAdmin.value || state.isFamilyManager.value(familyLocation.value?.familyId || '');
});

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

const handleClose = () => {
  emit('close');
};

const handleEdit = () => {
  if (familyLocation.value) {
    emit('edit-family-location', familyLocation.value.id);
  }
};

const handleDelete = async () => {
  if (!familyLocation.value) return;

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('familyLocation.list.confirmDelete', { name: familyLocation.value.location.name }),
  });

  if (confirmed) {
    deleteFamilyLocation(familyLocation.value.id, {
      onSuccess: () => {
        showSnackbar(t('familyLocation.messages.deleteSuccess'), 'success');
        emit('family-location-deleted');
        emit('close');
      },
      onError: (error) => {
        showSnackbar(error.message || t('familyLocation.messages.deleteError'), 'error');
      },
    });
  }
};
</script>