<template>
  <v-card flat>
    <v-tabs v-model="tab" color="primary" align-tabs="center">
      <v-tab :value="0">{{ t('familyMedia.addUnified.uploadFileTabTitle') }}</v-tab>
      <v-tab :value="1">{{ t('familyMedia.addUnified.fromUrlTabTitle') }}</v-tab>
    </v-tabs>

    <v-window v-model="tab">
      <v-window-item :value="0">
        <!-- Content for Upload File tab will go here -->
        <v-card :elevation="0">
          <v-progress-linear v-if="isSubmittingFile" indeterminate color="primary"></v-progress-linear>
          <v-card-text>
            <FamilyMediaForm ref="familyMediaFormRef" />
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" @click="handleSubmitFile" :loading="isSubmittingFile">{{
              t('common.save') }}</v-btn>
          </v-card-actions>
        </v-card>
      </v-window-item>

      <v-window-item :value="1">
        <FamilyMediaAddLink :family-id="props.familyId" @saved="handleMediaSaved" @close="handleMediaClose" />
      </v-window-item>
    </v-window>
  </v-card>
</template>

<script setup lang="ts">
import { ref, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyMediaForm } from '@/components/family-media'; // Import FamilyMediaForm
import FamilyMediaAddLink from '@/components/family-media/FamilyMediaAddLink.vue'; // Import new component
import { useAddFamilyMediaMutation, useFamilyMediaFormLogic, useGlobalSnackbar } from '@/composables'; // Import for file upload logic

const props = defineProps<{
  familyId: string;
}>();

const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const tab = ref(0);
const { showSnackbar } = useGlobalSnackbar();

// Logic for File Upload tab (from FamilyMediaAddView.vue)
const familyMediaFormRef = ref<InstanceType<typeof FamilyMediaForm> | null>(null);
const familyIdRef = toRef(props, 'familyId');
const { mutateAsync: addFamilyMediaMutation } = useAddFamilyMediaMutation();

const {
  state: { isSubmitting: isSubmittingFile },
  actions: { handleSubmit: handleSubmitFile },
} = useFamilyMediaFormLogic({
  familyId: familyIdRef,
  mutation: addFamilyMediaMutation,
  successMessageKey: 'familyMedia.messages.addSuccess',
  errorMessageKey: 'familyMedia.messages.saveError',
  formRef: familyMediaFormRef,
  onSuccess: () => {
    showSnackbar(t('familyMedia.messages.addSuccess'), 'success'); // Adding snackbar for consistency
    emit('saved');
  },
  transformData: (data, familyId) => ({
    familyId,
    file: data.file,
    description: data.description,
  }),
});

const handleMediaSaved = () => {
  emit('saved');
};

const handleMediaClose = () => {
  emit('close');
};
</script>

<style scoped>
/* Add any specific styles for FamilyMediaAddUnifiedView here */
</style>

<style scoped>
/* Add any specific styles for FamilyMediaAddUnifiedView here */
</style>
