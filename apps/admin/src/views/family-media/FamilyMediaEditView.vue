<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyMedia.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoadingFamilyMedia || isUpdatingFamilyMedia" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyMediaForm ref="familyMediaFormRef" :initial-media="familyMedia || undefined" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdateFamilyMedia" data-testid="save-family-media-button" :loading="isUpdatingFamilyMedia">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyMediaForm } from '@/components/family-media';
import { useGlobalSnackbar } from '@/composables';
import { useFamilyMediaQuery, useUpdateFamilyMediaMutation } from '@/composables/family-media';

interface FamilyMediaEditViewProps {
  familyId: string;
  familyMediaId: string;
}

const props = defineProps<FamilyMediaEditViewProps>();
const emit = defineEmits(['close', 'saved']);
const familyMediaFormRef = ref<InstanceType<typeof FamilyMediaForm> | null>(null);
const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const familyIdRef = toRef(props, 'familyId');
const familyMediaIdRef = toRef(props, 'familyMediaId');

const { familyMedia, isLoading: isLoadingFamilyMedia } = useFamilyMediaQuery(familyIdRef, familyMediaIdRef);
const { mutate: updateFamilyMedia, isPending: isUpdatingFamilyMedia } = useUpdateFamilyMediaMutation();

const handleUpdateFamilyMedia = async () => {
  if (!familyMediaFormRef.value) return;
  const isValid = await familyMediaFormRef.value.validate();
  if (!isValid) {
    return;
  }

  const formData = familyMediaFormRef.value.getFormData();

  if (!familyMedia.value || !props.familyId) {
    showSnackbar(t('familyMedia.messages.saveError'), 'error');
    return;
  }

  updateFamilyMedia({
    familyId: props.familyId,
    mediaId: familyMedia.value.id,
    description: formData.description,
  }, {
    onSuccess: () => {
      showSnackbar(t('familyMedia.messages.updateSuccess'), 'success');
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
