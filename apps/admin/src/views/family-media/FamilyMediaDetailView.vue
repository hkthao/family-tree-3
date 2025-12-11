<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyMedia.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="familyMediaStore.detail.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyMediaForm v-if="familyMediaStore.detail.item" :initial-media="familyMediaStore.detail.item" :read-only="true" />
      <v-alert v-else type="info" class="mt-4">{{ t('familyMedia.detail.notFound') }}</v-alert>

      <div v-if="familyMediaStore.detail.item && familyMediaStore.detail.item.filePath" class="mt-4 text-center">
        <v-img v-if="familyMediaStore.detail.item.mediaType === MediaType.Image" :src="familyMediaStore.detail.item.filePath" max-width="400" class="mx-auto"></v-img>
        <video v-else-if="familyMediaStore.detail.item.mediaType === MediaType.Video" controls :src="familyMediaStore.detail.item.filePath" max-width="400" class="mx-auto"></video>
        <audio v-else-if="familyMediaStore.detail.item.mediaType === MediaType.Audio" controls :src="familyMediaStore.detail.item.filePath" class="mx-auto"></audio>
        <p v-else-if="familyMediaStore.detail.item.mediaType === MediaType.Document">
          <a :href="familyMediaStore.detail.item.filePath" target="_blank">{{ t('familyMedia.viewer.viewDocument') }}</a>
        </p>
        <p v-else>{{ t('familyMedia.viewer.unsupportedMediaType') }}</p>
      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>
      <v-btn color="primary" @click="handleEdit" :disabled="!familyMediaStore.detail.item || familyMediaStore.detail.loading">{{ t('common.edit') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!familyMediaStore.detail.item || familyMediaStore.detail.loading">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyMediaStore } from '@/stores/familyMedia.store';
import { FamilyMediaForm } from '@/components/family-media';
import { useConfirmDialog, useGlobalSnackbar } from '@/composables';
import { MediaType } from '@/types/enums';

interface FamilyMediaDetailViewProps {
  familyMediaId: string;
}

const props = defineProps<FamilyMediaDetailViewProps>();
const emit = defineEmits(['close', 'media-deleted', 'edit-media']);
const { t } = useI18n();
const familyMediaStore = useFamilyMediaStore();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const currentFamilyId = ref(''); // TODO: Get current family ID from context/store

const loadFamilyMedia = async (id: string) => {
  if (currentFamilyId.value) { // Ensure familyId is available
    await familyMediaStore.getById(currentFamilyId.value, id);
  } else {
    showSnackbar(t('familyMedia.errors.familyIdRequired'), 'error');
  }
};

onMounted(async () => {
  // TODO: Get current family ID before loading media
  // For now, using a placeholder
  currentFamilyId.value = 'YOUR_FAMILY_ID_HERE'; // Replace with actual logic
  if (props.familyMediaId) {
    await loadFamilyMedia(props.familyMediaId);
  }
});

watch(
  () => props.familyMediaId,
  async (newId) => {
    if (newId) {
      await loadFamilyMedia(newId);
    }
  },
);

const handleClose = () => {
  emit('close');
  familyMediaStore.clearDetail();
};

const handleEdit = () => {
  if (familyMediaStore.detail.item) {
    emit('edit-media', familyMediaStore.detail.item.id);
  }
};

const handleDelete = async () => {
  if (!familyMediaStore.detail.item) return;

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('familyMedia.list.confirmDelete', { fileName: familyMediaStore.detail.item.fileName }),
  });

  if (confirmed && currentFamilyId.value) {
    try {
      await familyMediaStore.deleteFamilyMedia(currentFamilyId.value, familyMediaStore.detail.item.id);
      if (!familyMediaStore.detail.error) {
        showSnackbar(t('familyMedia.messages.deleteSuccess'), 'success');
        emit('media-deleted');
        emit('close');
      } else {
        showSnackbar(familyMediaStore.detail.error || t('familyMedia.messages.deleteError'), 'error');
      }
    } catch (error) {
      showSnackbar(t('familyMedia.messages.deleteError'), 'error');
    }
  }
};
</script>
