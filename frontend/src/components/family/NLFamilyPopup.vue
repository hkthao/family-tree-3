<template>
  <v-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" max-width="800px">
    <v-card>
      <v-card-title class="headline">{{ t('aiInput.title') }}</v-card-title>
      <v-card-text>
        <v-form ref="form">
          <v-textarea v-model="prompt" :label="t('aiInput.promptLabel')" rows="3" outlined clearable
            :rules="[rules.required, rules.length(500)]"></v-textarea>
        </v-form>
        <v-btn color="primary" :loading="loading" :disabled="loading" @click="generateData" class="mb-4">
          {{ t('aiInput.generateButton') }}
        </v-btn>

        <div v-if="generatedData && generatedData.length">
          <v-alert type="info" class="mb-4">{{ t('aiInput.previewMessage') }}</v-alert>
          <v-data-table :headers="headers" :items="generatedData" item-value="name" class="elevation-1"
            density="compact">
            <template #item.data="{ item }">
              <v-list density="compact">
                <v-list-item v-for="(value, key) in item" :key="key">
                  <v-list-item-title>{{ key }}: {{ value }}</v-list-item-title>
                </v-list-item>
              </v-list>
            </template>
          </v-data-table>
        </div>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey-darken-1" @click="cancel" :disabled="loading">{{ t('aiInput.cancelButton')
        }}</v-btn>
        <v-btn color="primary" :disabled="!generatedData || !generatedData.length || loading" @click="save">{{
          t('aiInput.saveButton') }}</v-btn>
      </v-card-actions>
      <v-overlay :model-value="loading" class="align-center justify-center" contained>
        <v-progress-circular indeterminate color="primary" size="64"></v-progress-circular>
      </v-overlay>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useNaturalLanguageInputStore } from '@/stores/naturalLanguageInput.store';
import { useFamilyStore } from '@/stores/family.store';
import { useNotificationStore } from '@/stores/notification.store';
import type { Family } from '@/types';

const props = defineProps({
  modelValue: Boolean,
});

const emit = defineEmits([
  'update:modelValue',
  'saved',
]);

const { t } = useI18n();
const naturalLanguageInputStore = useNaturalLanguageInputStore();
const familyStore = useFamilyStore();
const notificationStore = useNotificationStore();

const prompt = ref('');
const generatedData = ref<Family[] | null>(null);
const loading = ref(false);
const form = ref<HTMLFormElement | null>(null);

const headers = computed(() => [
  { title: t('common.key'), key: 'key', sortable: false },
  { title: t('common.value'), key: 'value', sortable: false },
]);

const rules = {
  required: (value: string) => !!value || t('aiInput.promptRequired'),
  length: (len: number) => (value: string) =>
    (value || '').length <= len ||
    t('aiInput.promptLength', { length: len }),
};

watch(() => props.modelValue, (newValue) => {
  if (!newValue) {
    // Reset state when dialog is closed
    prompt.value = '';
    generatedData.value = null;
  }
});

const generateData = async () => {
  if (!form.value) return;
  const { valid } = await form.value.validate();
  if (!valid) return;

  loading.value = true;
  try {
    const result = await naturalLanguageInputStore.generateFamilyData(prompt.value);
    if (result) {
      generatedData.value = result;
    } else {
      generatedData.value = null;
    }
  } catch (error) {
    console.error('Error generating data:', error);
    notificationStore.showSnackbar(
      t('aiInput.generateError', { entity: t('aiInput.families') }),
      'error',
    );
  } finally {
    loading.value = false;
  }
};

const save = async () => {
  if (!generatedData.value || !generatedData.value.length) return;

  try {
    for (const family of generatedData.value) {
      await familyStore.addItem(family);
    }
    notificationStore.showSnackbar(
      t('aiInput.saveSuccess'),
      'success',
    );
    emit('saved');
    emit('update:modelValue', false);
  } catch (error) {
    console.error('Error saving generated data:', error);
    notificationStore.showSnackbar(
      t('aiInput.saveError'),
      'error',
    );
  }
};

const cancel = () => {
  emit('update:modelValue', false);
};
</script>