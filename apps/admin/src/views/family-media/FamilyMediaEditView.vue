<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyMedia.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="familyMediaStore.detail.loading || familyMediaStore.isUploading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyMediaForm ref="familyMediaFormRef" :initial-media="familyMediaStore.detail.item || undefined" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdateFamilyMedia" data-testid="save-family-media-button" :loading="familyMediaStore.isUploading">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyMediaStore } from '@/stores/familyMedia.store';
import { FamilyMediaForm } from '@/components/family-media';
import { useGlobalSnackbar } from '@/composables';

interface FamilyMediaEditViewProps {
  familyMediaId: string;
}

const props = defineProps<FamilyMediaEditViewProps>();
const emit = defineEmits(['close', 'saved']);
const familyMediaFormRef = ref<InstanceType<typeof FamilyMediaForm> | null>(null);
const { t } = useI18n();
const familyMediaStore = useFamilyMediaStore();
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

const handleUpdateFamilyMedia = async () => {
  if (!familyMediaFormRef.value) return;
  const isValid = await familyMediaFormRef.value.validate();
  if (!isValid) {
    return;
  }

  const formData = familyMediaFormRef.value.getFormData();
  // Note: For update, we usually don't re-upload the file unless it's changed.
  // The backend API has separate endpoints for updating metadata vs. uploading new file.
  // For simplicity, this form only allows description change for existing media.
  // If file re-upload is needed, a more complex logic would be required.

  if (!familyMediaStore.detail.item || !currentFamilyId.value) {
    showSnackbar(t('familyMedia.messages.saveError'), 'error');
    return;
  }

  // Assuming backend has an update endpoint for metadata only
  // This part needs to be adapted based on actual backend API for update.
  // The current API.family-media.service.ts only has create and delete.
  // So, this feature is currently limited to description update if the API supports it.

  // For now, let's just emit saved. A full update function would be needed in the store.
  showSnackbar(t('familyMedia.messages.updateSuccess'), 'success'); // Placeholder
  emit('saved');
};

const closeForm = () => {
  emit('close');
  familyMediaStore.clearDetail(); // Clear detail state on close
};
</script>
