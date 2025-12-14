<template>
  <v-card :elevation="0" data-testid="family-media-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyMedia.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isSubmitting" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyMediaForm ref="familyMediaFormRef" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleSubmit" data-testid="save-family-media-button" :loading="isSubmitting">{{
        t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyMediaForm } from '@/components/family-media';
import { useAddFamilyMediaMutation } from '@/composables/family-media';
import { useFamilyMediaFormLogic } from '@/composables/family-media/useFamilyMediaFormLogic';

interface FamilyMediaAddViewProps {
  familyId: string;
}

const props = defineProps<FamilyMediaAddViewProps>();
const emit = defineEmits(['close', 'saved']);
const familyMediaFormRef = ref<InstanceType<typeof FamilyMediaForm> | null>(null);
const { t } = useI18n();

const familyIdRef = toRef(props, 'familyId');
const { mutateAsync: addFamilyMediaMutation } = useAddFamilyMediaMutation();

const { isSubmitting, handleSubmit } = useFamilyMediaFormLogic({
  familyId: familyIdRef,
  mutation: addFamilyMediaMutation,
  successMessageKey: 'familyMedia.messages.addSuccess',
  errorMessageKey: 'familyMedia.messages.saveError',
  formRef: familyMediaFormRef,
  onSuccess: () => {
    emit('saved');
  },
  transformData: (formData, familyId) => ({
    familyId,
    file: formData.file,
    description: formData.description,
  }),
});

const closeForm = () => {
  emit('close');
};
</script>
