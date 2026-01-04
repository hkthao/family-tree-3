<template>
  <v-card :elevation="0">
    <v-card-title class="headline text-center">{{ t('voiceProfile.generateVoiceDialog.title') }}</v-card-title>
    <v-overlay :model-value="isGeneratingVoice" class="align-center justify-center" contained scrim="#E0E0E0">
      <v-progress-circular color="primary" indeterminate size="64"></v-progress-circular>
    </v-overlay>
    <v-card-text>
      <v-textarea v-model="textToGenerate" :label="t('voiceProfile.generateVoiceDialog.textFieldLabel')" :rows="2"
        :max-rows="5" outlined clearable data-testid="text-to-generate-input"
        :rules="[v => !!v || t('common.fieldRequired')]"></v-textarea>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" text @click="emit('close')" data-testid="generate-voice-cancel-button">{{ t('common.cancel')
        }}</v-btn>
      <v-btn color="primary" @click="generate" :disabled="!textToGenerate.trim() || isGeneratingVoice"
        :loading="isGeneratingVoice" data-testid="generate-voice-confirm-button">{{ t('common.generate') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGenerateVoiceMutation } from '@/composables/voice-profile/useGenerateVoiceMutation';
import { useGlobalSnackbar } from '@/composables';

const props = defineProps<{
  voiceProfileId: string;
}>();

const emit = defineEmits(['close', 'generated']);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const textToGenerate = ref('');

const { mutate: generateVoice, isPending: isGeneratingVoice } = useGenerateVoiceMutation();

const generate = () => {
  if (!textToGenerate.value.trim()) {
    showSnackbar(t('voiceProfile.messages.textRequiredForGeneration'), 'warning');
    return;
  }

  generateVoice({ voiceProfileId: props.voiceProfileId, text: textToGenerate.value }, {
    onSuccess: () => {
      showSnackbar(t('voiceProfile.messages.generateVoiceSuccess'), 'success');
      emit('generated');
      textToGenerate.value = '';
    },
    onError: (error: Error) => {
      showSnackbar(error.message || t('voiceProfile.messages.generateVoiceError'), 'error');
    },
  });
};

watch(() => props.voiceProfileId, (newId) => {
  if (newId) {
    textToGenerate.value = ''; // Clear text when a new voice profile is selected
  }
});
</script>

<style scoped>
/* Add any specific styles for the dialog here */
</style>
