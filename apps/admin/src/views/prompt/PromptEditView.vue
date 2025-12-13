<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('prompt.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="detail.loading || update.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <PromptForm ref="promptFormRef" v-if="prompt" :initial-prompt-data="prompt" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdatePrompt" data-testid="save-prompt-button" :loading="update.loading">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { usePromptStore } from '@/stores/prompt.store';
import { PromptForm } from '@/components/prompt';
import type { Prompt } from '@/types';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables';

interface PromptEditViewProps {
  promptId: string;
}

const props = defineProps<PromptEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const promptFormRef = ref<InstanceType<typeof PromptForm> | null>(null);

const { t } = useI18n();
const promptStore = usePromptStore();
const { showSnackbar } = useGlobalSnackbar();

const { detail, update } = storeToRefs(promptStore);

const prompt = ref<Prompt | undefined>(undefined);

const loadPrompt = async (id: string) => {
  await promptStore.getById(id);
  if (promptStore.detail.item)
    prompt.value = promptStore.detail.item;
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

  try {
    await promptStore.updateItem(promptData as Prompt);
    if (!promptStore.error) {
      showSnackbar(t('prompt.messages.updateSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(promptStore.error || t('prompt.messages.saveError'), 'error');
    }
  } catch (error) {
    showSnackbar(t('prompt.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  emit('close');
};
</script>
