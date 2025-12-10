<template>
  <v-card :elevation="0" data-testid="family-media-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyMedia.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="familyMediaStore.isUploading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyMediaForm ref="familyMediaFormRef" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleAddFamilyMedia" data-testid="save-family-media-button" :loading="familyMediaStore.isUploading">{{
        t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyMediaStore } from '@/stores/familyMedia.store';
import { FamilyMediaForm } from '@/components/family-media'; // Assuming index.ts exports FamilyMediaForm
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

interface FamilyMediaAddViewProps {
  familyId: string;
}

const props = defineProps<FamilyMediaAddViewProps>();
const emit = defineEmits(['close', 'saved']);
const familyMediaFormRef = ref<InstanceType<typeof FamilyMediaForm> | null>(null);
const { t } = useI18n();
const familyMediaStore = useFamilyMediaStore();
const { showSnackbar } = useGlobalSnackbar();

const handleAddFamilyMedia = async () => {
  if (!familyMediaFormRef.value) return;
  const isValid = await familyMediaFormRef.value.validate();
  if (!isValid) {
    return;
  }

  const formData = familyMediaFormRef.value.getFormData();
  if (!formData.file) {
    showSnackbar(t('familyMedia.errors.noFileSelected'), 'error');
    return;
  }

  try {
    const result = await familyMediaStore.createFamilyMedia(props.familyId, formData.file, formData.description);
    if (result.ok) {
      showSnackbar(t('familyMedia.messages.addSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(result.error.message || t('familyMedia.messages.saveError'), 'error');
    }
  } catch (error) {
    showSnackbar(t('familyMedia.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  emit('close');
};
</script>
