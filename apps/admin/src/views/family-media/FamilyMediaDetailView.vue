<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyMedia.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoadingFamilyMedia || isDeletingFamilyMedia" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyMediaForm v-if="familyMedia" :initial-media="familyMedia" :read-only="true" />
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!familyMedia || isLoadingFamilyMedia || isDeletingFamilyMedia">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { computed, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyMediaForm } from '@/components/family-media';
import { useConfirmDialog, useGlobalSnackbar } from '@/composables';
import { MediaType } from '@/types/enums';
import { useFamilyMediaQuery, useDeleteFamilyMediaMutation } from '@/composables/family-media';

interface FamilyMediaDetailViewProps {
  familyId: string;
  familyMediaId: string;
}

const props = defineProps<FamilyMediaDetailViewProps>();
const emit = defineEmits(['close', 'media-deleted']);
const { t } = useI18n();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const familyIdRef = toRef(props, 'familyId');
const familyMediaIdRef = toRef(props, 'familyMediaId');

const { familyMedia, isLoading: isLoadingFamilyMedia, error: familyMediaError } = useFamilyMediaQuery(familyIdRef, familyMediaIdRef);
const { mutate: deleteFamilyMedia, isPending: isDeletingFamilyMedia } = useDeleteFamilyMediaMutation();

const handleClose = () => {
  emit('close');
};

const handleDelete = async () => {
  if (!familyMedia.value) return;

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('familyMedia.list.confirmDelete', { fileName: familyMedia.value.fileName }),
  });

  if (confirmed) {
    deleteFamilyMedia({ familyId: props.familyId, id: familyMedia.value.id }, {
      onSuccess: () => {
        showSnackbar(t('familyMedia.messages.deleteSuccess'), 'success');
        emit('media-deleted');
        emit('close');
      },
      onError: (error) => {
        showSnackbar(error.message || t('familyMedia.messages.deleteError'), 'error');
      },
    });
  }
};
</script>
