<template>
  <v-card :elevation="0" data-testid="prompt-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('prompt.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isAddingPrompt" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <PromptForm ref="promptFormRef" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleAddPrompt" data-testid="save-prompt-button" :loading="isAddingPrompt">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { PromptForm } from '@/components/prompt';
import type { Prompt } from '@/types';
import { useGlobalSnackbar } from '@/composables';
import { useCreatePromptMutation } from '@/composables/prompt';

const emit = defineEmits(['close', 'saved']);
const promptFormRef = ref<InstanceType<typeof PromptForm> | null>(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const { mutate: createPrompt, isPending: isAddingPrompt } = useCreatePromptMutation();

const handleAddPrompt = async () => {
  if (!promptFormRef.value) return;
  const isValid = await promptFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const promptData = promptFormRef.value.getFormData();

  const newPrompt: Omit<Prompt, 'id'> = {
    ...promptData,
  };

  createPrompt(newPrompt, {
    onSuccess: () => {
      showSnackbar(t('prompt.messages.addSuccess'), 'success');
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
