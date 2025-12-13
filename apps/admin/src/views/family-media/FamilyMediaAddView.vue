<template>
  <v-card :elevation="0" data-testid="family-media-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyMedia.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isAddingFamilyMedia" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyMediaForm ref="familyMediaFormRef" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleAddFamilyMedia" data-testid="save-family-media-button" :loading="isAddingFamilyMedia">{{
        t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyMediaForm } from '@/components/family-media';
import { useGlobalSnackbar } from '@/composables';
import { useAddFamilyMediaMutation } from '@/composables/family-media';

interface FamilyMediaAddViewProps {
  familyId: string;
}

const props = defineProps<FamilyMediaAddViewProps>();
const emit = defineEmits(['close', 'saved']);
const familyMediaFormRef = ref<InstanceType<typeof FamilyMediaForm> | null>(null);
const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const { mutate: addFamilyMedia, isPending: isAddingFamilyMedia } = useAddFamilyMediaMutation();

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

  addFamilyMedia({ familyId: props.familyId, file: formData.file, description: formData.description }, {
    onSuccess: () => {
      showSnackbar(t('familyMedia.messages.addSuccess'), 'success');
      emit('saved');
    },
    onError: (error) => {
      showSnackbar(error.message || t('familyMedia.messages.saveError'), 'error');
    },
  });
};

const closeForm = () => {
  emit('close');
};
</script>
