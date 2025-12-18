<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLocation.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoadingFamilyLocation || isDeletingFamilyLocation" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyLocationForm
        v-if="familyLocation"
        :initial-family-location-data="familyLocation"
        :family-id="familyLocation.familyId"
        :read-only="true"
      />
      <v-alert v-else-if="familyLocationError" type="error" class="mt-4">{{
        familyLocationError.message || t('familyLocation.detail.notFound')
      }}</v-alert>
      <v-alert v-else type="info" class="mt-4">{{ t('familyLocation.detail.notFound') }}</v-alert>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose" :disabled="isLoadingFamilyLocation || isDeletingFamilyLocation">{{
        t('common.close')
      }}</v-btn>
      <v-btn
        color="primary"
        @click="handleEdit"
        :disabled="!familyLocation || isLoadingFamilyLocation || isDeletingFamilyLocation"
        v-if="canEditOrDelete"
        >{{ t('common.edit') }}</v-btn
      >
      <v-btn
        color="error"
        @click="handleDelete"
        :disabled="!familyLocation || isLoadingFamilyLocation || isDeletingFamilyLocation"
        v-if="canEditOrDelete"
        >{{ t('common.delete') }}</v-btn
      >
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { computed, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyLocationForm } from '@/components/family-location';
import { useConfirmDialog, useAuth, useGlobalSnackbar } from '@/composables';
import { useFamilyLocationQuery, useDeleteFamilyLocationMutation } from '@/composables';

interface FamilyLocationDetailViewProps {
  familyLocationId: string;
}

const props = defineProps<FamilyLocationDetailViewProps>();
const emit = defineEmits(['close', 'family-location-deleted', 'edit-family-location']);

const { t } = useI18n();
const { showConfirmDialog } = useConfirmDialog();
const { isAdmin, isFamilyManager } = useAuth();
const { showSnackbar } = useGlobalSnackbar();

const familyLocationIdRef = toRef(props, 'familyLocationId');
const {
  data: familyLocation,
  isLoading: isLoadingFamilyLocation,
  error: familyLocationError,
} = useFamilyLocationQuery(familyLocationIdRef);
const { mutate: deleteFamilyLocation, isPending: isDeletingFamilyLocation } = useDeleteFamilyLocationMutation();

const canEditOrDelete = computed(() => {
  return isAdmin.value || isFamilyManager.value;
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
    message: t('familyLocation.list.confirmDelete', { name: familyLocation.value.name }),
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
