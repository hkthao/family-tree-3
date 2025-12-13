<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('prompt.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="detail.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <PromptForm v-if="prompt" :initial-prompt-data="prompt" :read-only="true" />
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="handleClose">{{ t('common.close') }}</v-btn>
      <v-btn color="primary" @click="handleEdit" :disabled="!prompt || detail.loading" v-if="canEditOrDelete">{{ t('common.edit') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!prompt || detail.loading" v-if="canEditOrDelete">{{ t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { usePromptStore } from '@/stores/prompt.store';
import { PromptForm } from '@/components/prompt';
import type { Prompt } from '@/types';
import { useConfirmDialog, useAuth, useGlobalSnackbar } from '@/composables';
import { storeToRefs } from 'pinia';

interface PromptDetailViewProps {
  promptId: string;
}

const props = defineProps<PromptDetailViewProps>();
const emit = defineEmits(['close', 'prompt-deleted', 'edit-prompt']);

const { t } = useI18n();
const promptStore = usePromptStore();
const { showConfirmDialog } = useConfirmDialog();
const { isAdmin } = useAuth();
const { showSnackbar } = useGlobalSnackbar();

const { detail } = storeToRefs(promptStore);

const prompt = ref<Prompt | undefined>(undefined);

const canEditOrDelete = computed(() => {
  return isAdmin.value;
});

const loadPrompt = async (id: string) => {
  await promptStore.getById(id);
  if (promptStore.detail.item) {
    prompt.value = promptStore.detail.item;
  } else {
    prompt.value = undefined;
  }
};

onMounted(async () => {
  if (props.promptId) {
    await loadPrompt(props.promptId);
  }
});

watch(
  () => props.promptId,
  async (newId) => {
    if (newId) {
      await loadPrompt(newId);
    }
  },
);

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
      await promptStore.deleteItem(prompt.value.id);
      if (!promptStore.error) {
        showSnackbar(t('prompt.messages.deleteSuccess'), 'success');
        emit('prompt-deleted');
        emit('close');
      } else {
        showSnackbar(promptStore.error || t('prompt.messages.deleteError'), 'error');
      }
    } catch (error) {
      showSnackbar(t('prompt.messages.deleteError'), 'error');
    }
  }
};
</script>
