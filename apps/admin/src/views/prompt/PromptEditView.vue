<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('prompt.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isQueryLoading || isUpdatingPrompt" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <PromptForm ref="promptFormRef" v-if="prompt" :initial-prompt-data="prompt" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm" :disabled="isQueryLoading || isUpdatingPrompt">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdatePrompt" data-testid="save-prompt-button" :loading="isUpdatingPrompt">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { PromptForm } from '@/components/prompt';
import type { Prompt } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { usePromptQuery, useUpdatePromptMutation } from '@/composables/prompt';

interface PromptEditViewProps {
  promptId: string;
}

const props = defineProps<PromptEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const promptFormRef = ref<InstanceType<typeof PromptForm> | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const { data: prompt, isLoading: isQueryLoading, isError: isQueryError, error: queryError } = usePromptQuery(toRef(props, 'promptId'));
const { mutate: updatePrompt, isPending: isUpdatingPrompt, error: mutationError } = useUpdatePromptMutation();



const handleUpdatePrompt = async () => {
  if (!promptFormRef.value) return;
  const isValid = await promptFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const promptData = promptFormRef.value.getFormData() as Prompt;
  if (!promptData.id) {
    showSnackbar(t('prompt.messages.saveError'), 'error');
    return;
  }

  updatePrompt(promptData, {
    onSuccess: () => {
      showSnackbar(t('prompt.messages.updateSuccess'), 'success');
      emit('saved');
    },
    onError: (error) => {
      showSnackbar(error.message || t('prompt.messages.saveError'), 'error');
    },
  });
};

const closeForm = () => {
  emit('close');
};
</script>
