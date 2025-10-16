<template>
  <v-dialog :model-value="modelValue" @update:modelValue="$emit('update:modelValue', $event)" max-width="800px">
    <v-card>
      <v-card-title class="headline">{{ t('aiInput.title') }}</v-card-title>
      <v-card-text>
        <v-form ref="form">
          <v-textarea v-model="prompt" :label="t('aiInput.promptLabelRelationship')" rows="3" outlined clearable counter
            :auto-grow="true" :rules="[rules.required, rules.length(1000)]"></v-textarea>
        </v-form>
        <v-btn color="primary" :loading="loading" :disabled="loading" @click="generateData" class="mb-4">
          {{ t('aiInput.generateButton') }}
        </v-btn>
        <v-btn color="info" @click="fillSamplePrompt" class="mb-4 ml-2">
          {{ t('aiInput.fillSampleButton') }}
        </v-btn>

        <div v-if="generatedData && generatedData.length">
          <v-alert type="info" class="mb-4">{{ t('aiInput.previewMessage') }}</v-alert>
          <div v-for="(relationship, relationshipIndex) in generatedData" :key="relationshipIndex" class="mb-6 pa-4 border rounded">
            <h4 class="text-h6 mb-2">{{ t('aiInput.relationship') }} #{{ relationshipIndex + 1 }}</h4>
            <v-alert v-if="relationship.validationErrors && relationship.validationErrors.length" type="warning" class="mb-2">
              <p>{{ t('aiInput.validationErrorsFound') }}</p>
              <ul>
                <li v-for="(error, errorIndex) in relationship.validationErrors" :key="errorIndex">{{ error }}</li>
              </ul>
            </v-alert>
            <v-divider class="mb-2"></v-divider>
            <div v-for="key in displayKeys" :key="key">
              <p class="text-body-2">
                <strong>{{ t(`relationship.list.headers.${key}`) || key }}:</strong>
                {{ formatValue(relationship[key as keyof Relationship], key) }}
              </p>
            </div>
          </div>
        </div>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey-darken-1" @click="cancel" :disabled="loading">{{ t('aiInput.cancelButton')
          }}</v-btn>
        <v-btn color="primary" :disabled="!generatedData || !generatedData.length || loading || hasValidationErrors"
          @click="save">{{
            t('aiInput.saveButton') }}</v-btn>
      </v-card-actions>
      <v-progress-linear v-if="loading" indeterminate color="primary" height="4" class="mb-0"></v-progress-linear>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useNaturalLanguageInputStore } from '@/stores/naturalLanguageInput.store';
import { useRelationshipStore } from '@/stores/relationship.store';
import { useNotificationStore } from '@/stores/notification.store';
import type { Relationship } from '@/types';
import { getRelationshipTypeTitle } from '@/constants/relationshipTypes';

const props = defineProps({
  modelValue: Boolean,
});

const emit = defineEmits([
  'update:modelValue',
  'saved',
]);

const { t } = useI18n();
const naturalLanguageInputStore = useNaturalLanguageInputStore();
const relationshipStore = useRelationshipStore();
const notificationStore = useNotificationStore();

const prompt = ref('');
const generatedData = ref<Relationship[] | null>(null);
const loading = ref(false);
const form = ref<HTMLFormElement | null>(null);

const displayKeys = [
  'sourceMemberFullName',
  'targetMemberFullName',
  'type',
];

const hasValidationErrors = computed(() => {
  return generatedData.value?.some(relationship => relationship.validationErrors && relationship.validationErrors.length > 0) || false;
});

const formatValue = (value: any, key: string) => {
  if (value === null || value === '') {
    return t('common.unknown');
  }
  if (key === 'type') {
    return getRelationshipTypeTitle(value);
  }
  return value;
};

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
  naturalLanguageInputStore.error = null; // Clear previous errors
  try {
    const result = await naturalLanguageInputStore.generateRelationshipData(prompt.value);
    if (result) {
      generatedData.value = result;
    } else {
      generatedData.value = null;
      if (naturalLanguageInputStore.error) {
        notificationStore.showSnackbar(naturalLanguageInputStore.error, 'error');
      }
    }
  } catch (error) {
    console.error('Error generating data:', error);
    notificationStore.showSnackbar(
      t('aiInput.generateError', { entity: t('aiInput.relationships') }),
      'error',
    );
  } finally {
    loading.value = false;
  }
};

const save = async () => {
  if (!generatedData.value || !generatedData.value.length) return;

  try {
    await relationshipStore.addItems(generatedData.value);
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
  naturalLanguageInputStore.error = null; // Clear error on cancel
  emit('update:modelValue', false);
};

const fillSamplePrompt = () => {
  prompt.value = `Tạo mối quan hệ giữa Nguyễn Văn A và Trần Thị B. Nguyễn Văn A là chồng của Trần Thị B, kết hôn vào ngày 2000-01-15. Mối quan hệ này được mô tả là một cặp vợ chồng hạnh phúc.`;
};
</script>
