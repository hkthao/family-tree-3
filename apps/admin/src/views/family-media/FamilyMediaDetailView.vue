<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyMedia.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoadingFamilyMedia || isDeleting" indeterminate color="primary"></v-progress-linear>
    <v-img v-if="familyMedia && familyMedia.mediaType === MediaType.Image && familyMedia.filePath"
           :src="familyMedia.filePath"
           :alt="familyMedia.fileName"
           class="my-4"
           contain
           height="200"
    ></v-img>
    <v-card-text class="pa-0">
      <v-list density="compact" v-if="familyMedia">
        <v-list-item>
          <v-list-item-title>{{ t('familyMedia.detail.fileName') }}:</v-list-item-title>
          <v-list-item-subtitle>{{ familyMedia.fileName }}</v-list-item-subtitle>
        </v-list-item>
        <v-list-item>
          <v-list-item-title>{{ t('familyMedia.detail.description') }}:</v-list-item-title>
          <v-list-item-subtitle>{{ familyMedia.description || t('common.na') }}</v-list-item-subtitle>
        </v-list-item>
        <v-list-item>
          <v-list-item-title>{{ t('familyMedia.detail.mediaType') }}:</v-list-item-title>
          <v-list-item-subtitle>{{ localizedMediaTypeTitle }}</v-list-item-subtitle>
        </v-list-item>
        <v-list-item>
          <v-list-item-title>{{ t('familyMedia.detail.fileSize') }}:</v-list-item-title>
          <v-list-item-subtitle>{{ formattedFileSize }}</v-list-item-subtitle>
        </v-list-item>
        <v-list-item>
          <v-list-item-title>{{ t('familyMedia.detail.createdDate') }}:</v-list-item-title>
          <v-list-item-subtitle>{{ formattedCreationTime }}</v-list-item-subtitle>
        </v-list-item>
      </v-list>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!familyMedia || isLoadingFamilyMedia || isDeleting">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { toRef, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyMediaQuery, useDeleteFamilyMediaMutation, useFamilyMediaDeletion } from '@/composables';
import { formatBytes, formatLocalizedDateTime } from '@/utils/format.utils';
import { MediaType } from '@/types/enums';
import { getMediaTypeOptions } from '@/composables/utils/mediaTypeOptions'; // Import getMediaTypeOptions

interface FamilyMediaDetailViewProps {
  familyId: string;
  familyMediaId: string;
}

const props = defineProps<FamilyMediaDetailViewProps>();
const emit = defineEmits(['close', 'media-deleted']);
const { t } = useI18n();

const familyIdRef = toRef(props, 'familyId');
const familyMediaIdRef = toRef(props, 'familyMediaId');

const { familyMedia, isLoading: isLoadingFamilyMedia } = useFamilyMediaQuery(familyMediaIdRef);
const { mutateAsync: deleteFamilyMediaMutation } = useDeleteFamilyMediaMutation();

const {
  state: { isDeleting },
  actions: { confirmAndDelete },
} = useFamilyMediaDeletion({
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

const mediaTypeOptions = computed(() => getMediaTypeOptions(t));
const localizedMediaTypeTitle = computed(() => {
  const mediaType = familyMedia.value?.mediaType;
  if (!mediaType) return t('common.na');
  const option = mediaTypeOptions.value.find(opt => opt.value === mediaType);
  return option ? option.title : t('common.na');
});

const formattedFileSize = computed(() => formatBytes(familyMedia.value?.fileSize || 0));
const formattedCreationTime = computed(() => formatLocalizedDateTime(familyMedia.value?.created));

const handleClose = () => {
  emit('close');
};

const handleDelete = async () => {
  if (!familyMedia.value) return;
  await confirmAndDelete(familyMedia.value.id, familyMedia.value.fileName);
};
</script>
