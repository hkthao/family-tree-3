<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('prompt.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoading || isDeletingPrompt" indeterminate color="primary"></v-progress-linear>
    <v-alert v-else-if="isError" type="error" dismissible class="mt-4">
      {{ error?.message || t('prompt.detail.errorLoading') }}
    </v-alert>
    <v-card-text>
      <PromptForm v-if="prompt" :initial-prompt-data="prompt" :read-only="true" />
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>
      <v-btn color="primary" @click="handleEdit" :disabled="!prompt || isLoading || isDeletingPrompt" v-if="canEditOrDelete">{{ t('common.edit') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!prompt || isLoading || isDeletingPrompt" v-if="canEditOrDelete">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { toRef, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { PromptForm } from '@/components/prompt';
import type { Prompt } from '@/types';
import { useConfirmDialog, useAuth, useGlobalSnackbar } from '@/composables';
import { usePromptQuery, useDeletePromptMutation } from '@/composables/prompt';

interface PromptDetailViewProps {
  promptId: string;
}

const props = defineProps<PromptDetailViewProps>();
const emit = defineEmits(['close', 'prompt-deleted', 'edit-prompt']);

const { t } = useI18n();
const { showConfirmDialog } = useConfirmDialog();
const { isAdmin } = useAuth();
const { showSnackbar } = useGlobalSnackbar();

const { data: prompt, isLoading, isError, error } = usePromptQuery(toRef(props, 'promptId'));
const { mutateAsync: deletePrompt, isPending: isDeletingPrompt } = useDeletePromptMutation();

const canEditOrDelete = computed(() => {
  return isAdmin.value;
});



const handleClose = () => {
  emit('close');
};

const handleEdit = () => {
  if (prompt.value) {
    emit('edit-prompt', prompt.value.id);
  }
};

const handleDelete = async () => {
  if (!prompt.value) return;

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('prompt.list.confirmDelete', { title: prompt.value.title }),
  });

  if (confirmed) {
    try {
      await deletePrompt(prompt.value.id);
      showSnackbar(t('prompt.messages.deleteSuccess'), 'success');
      emit('prompt-deleted');
      emit('close');
    } catch (error) {
      showSnackbar((error as Error).message || t('prompt.messages.deleteError'), 'error');
    }
  }
};
</script>
