<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyMedia.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoadingFamilyMedia || isSubmitting" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyMediaForm ref="familyMediaFormRef" :initial-media="familyMedia || undefined" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleSubmit" data-testid="save-family-media-button" :loading="isSubmitting">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyMediaForm } from '@/components/family-media';
import { useFamilyMediaQuery, useUpdateFamilyMediaMutation } from '@/composables/family-media';
import { useFamilyMediaFormLogic } from '@/composables/family-media/useFamilyMediaFormLogic';

interface FamilyMediaEditViewProps {
  familyId: string;
  familyMediaId: string;
}

const props = defineProps<FamilyMediaEditViewProps>();
const emit = defineEmits(['close', 'saved']);
const familyMediaFormRef = ref<InstanceType<typeof FamilyMediaForm> | null>(null);
const { t } = useI18n();

const familyIdRef = toRef(props, 'familyId');
const familyMediaIdRef = toRef(props, 'familyMediaId');

const { familyMedia, isLoading: isLoadingFamilyMedia } = useFamilyMediaQuery(familyIdRef, familyMediaIdRef);
const { mutateAsync: updateFamilyMediaMutation } = useUpdateFamilyMediaMutation();

const { isSubmitting, handleSubmit } = useFamilyMediaFormLogic({
  familyId: familyIdRef,
  mutation: updateFamilyMediaMutation,
  successMessageKey: 'familyMedia.messages.updateSuccess',
  errorMessageKey: 'familyMedia.messages.saveError',
  formRef: familyMediaFormRef,
  onSuccess: () => {
    emit('saved');
  },
  transformData: (formData, familyId) => {
    // Ensure mediaId is included for update
    if (!familyMedia.value?.id) {
      throw new Error(t('familyMedia.messages.saveError'));
    }
    return {
      familyId,
      mediaId: familyMedia.value.id,
      description: formData.description,
    };
  },
  isUpdate: true,
});

const closeForm = () => {
  emit('close');
};
</script>
