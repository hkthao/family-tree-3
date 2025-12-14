<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyMedia.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoadingFamilyMedia || isDeleting" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyMediaForm v-if="familyMedia" :initial-media="familyMedia" :read-only="true" />
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!familyMedia || isLoadingFamilyMedia || isDeleting">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyMediaForm } from '@/components/family-media';
import { useFamilyMediaQuery, useDeleteFamilyMediaMutation } from '@/composables/family-media';
import { useFamilyMediaDeletion } from '@/composables/family-media/useFamilyMediaDeletion';

interface FamilyMediaDetailViewProps {
  familyId: string;
  familyMediaId: string;
}

const props = defineProps<FamilyMediaDetailViewProps>();
const emit = defineEmits(['close', 'media-deleted']);
const { t } = useI18n();

const familyIdRef = toRef(props, 'familyId');
const familyMediaIdRef = toRef(props, 'familyMediaId');

const { familyMedia, isLoading: isLoadingFamilyMedia } = useFamilyMediaQuery(familyIdRef, familyMediaIdRef);
const { mutateAsync: deleteFamilyMediaMutation } = useDeleteFamilyMediaMutation();

const { isDeleting, confirmAndDelete } = useFamilyMediaDeletion({
  familyId: familyIdRef,
  deleteMutation: deleteFamilyMediaMutation,
  successMessageKey: 'familyMedia.messages.deleteSuccess',
  errorMessageKey: 'familyMedia.messages.deleteError',
  confirmationTitleKey: 'confirmDelete.title',
  confirmationMessageKey: 'familyMedia.list.confirmDelete',
  onSuccess: () => {
    emit('media-deleted');
    emit('close');
  },
});

const handleClose = () => {
  emit('close');
};

const handleDelete = async () => {
  if (!familyMedia.value) return;
  await confirmAndDelete(familyMedia.value.id, familyMedia.value.fileName);
};
</script>
