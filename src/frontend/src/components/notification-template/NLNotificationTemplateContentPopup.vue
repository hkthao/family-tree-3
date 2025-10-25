<template>
  <v-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" max-width="800px">
    <v-card>
      <v-card-title class="headline">{{ t('aiInput.title') }}</v-card-title>
      <v-card-text>
        <v-form ref="form">
          <v-textarea v-model="prompt" :label="t('notificationTemplate.form.aiPromptLabel')" rows="3" outlined clearable
            counter :auto-grow="true" :rules="[rules.required, rules.length(1000)]"></v-textarea>
        </v-form>

        <div v-if="generatedData">
          <v-alert type="info" class="mb-4">{{ t('aiInput.previewMessage') }}</v-alert>
          <div class="mb-6 pa-4 border rounded">
            <v-alert v-if="generatedData.validationErrors && generatedData.validationErrors.length" type="warning"
              class="mb-2">
              <p>{{ t('aiInput.validationErrorsFound') }}</p>
              <ul>
                <li v-for="(error, errorIndex) in generatedData.validationErrors" :key="errorIndex">{{ error }}</li>
              </ul>
            </v-alert>
            <v-divider class="mb-2"></v-divider>
            <p class="text-body-2">
              <strong>{{ t('notificationTemplate.form.subject') }}:</strong>
              {{ generatedData.subject || t('common.unknown') }}
            </p>
            <p class="text-body-2">
              <strong>{{ t('notificationTemplate.form.body') }}:</strong>
              {{ generatedData.body || t('common.unknown') }}
            </p>
          </div>
        </div>
      </v-card-text>
      <v-card-actions>
        <v-btn color="primary" :loading="loading" :disabled="loading" @click="generateData" class="mr-2">
          {{ t('aiInput.generateButton') }}
        </v-btn>
        <v-btn color="info" @click="fillSamplePrompt" class="mr-2">
          {{ t('aiInput.fillSampleButton') }}
        </v-btn>
        <v-spacer></v-spacer>
        <v-btn color="grey-darken-1" @click="cancel" :disabled="loading">{{ t('aiInput.cancelButton')
          }}</v-btn>
        <v-btn color="primary" :disabled="!generatedData || loading || hasValidationErrors" @click="save">{{
          t('aiInput.saveButton') }}</v-btn>
      </v-card-actions>
      <v-progress-linear v-if="loading" indeterminate color="primary" height="4" class="mb-0"></v-progress-linear>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useNotificationStore, useNotificationTemplateStore } from '@/stores';
import { NotificationType, NotificationChannel, TemplateFormat } from '@/types';

const props = defineProps({
  modelValue: Boolean,
  eventType: {
    type: String,
    default: null,
  },
  channel: {
    type: String,
    default: null,
  },
  format: {
    type: String,
    default: null,
  },
  language: {
    type: String,
    default: null,
  },
});

const emit = defineEmits([
  'update:modelValue',
  'generated',
]);

const { t } = useI18n();
const notificationTemplateStore = useNotificationTemplateStore();
const notificationStore = useNotificationStore();

const prompt = ref('');
const generatedData = ref<{ subject: string; body: string; validationErrors?: string[] } | null>(null);
const loading = ref(false);
const form = ref<HTMLFormElement | null>(null);

const getNotificationTypeText = computed(() => {
  if (props.eventType === null) return null;
  const enumKey = NotificationType[parseInt(props.eventType, 10)];
  return enumKey ? t(`notificationType.${enumKey}`) : null;
});

const getNotificationChannelText = computed(() => {
  if (props.channel === null) return null;
  const enumKey = NotificationChannel[parseInt(props.channel, 10)];
  return enumKey ? t(`notificationChannel.${enumKey}`) : null;
});

const getTemplateFormatText = computed(() => {
  if (props.format === null) return null;
  const enumKey = TemplateFormat[parseInt(props.format, 10)];
  return enumKey ? t(`templateFormat.${enumKey}`) : null;
});

const hasValidationErrors = computed(() => {
  return generatedData.value?.validationErrors && generatedData.value.validationErrors.length > 0 || false;
});

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
  notificationTemplateStore.error = null; // Clear previous errors
  try {
    let fullPrompt = prompt.value;
    if (getNotificationTypeText.value) {
      fullPrompt += `\nLoại sự kiện: ${getNotificationTypeText.value}`;
    }
    if (getNotificationChannelText.value) {
      fullPrompt += `\nKênh thông báo: ${getNotificationChannelText.value}`;
    }
    if (getTemplateFormatText.value) {
      fullPrompt += `\nĐịnh dạng: ${getTemplateFormatText.value}`;
    }
    if (props.language) {
      fullPrompt += `\nNgôn ngữ: ${props.language}`;
    }

    const result = await notificationTemplateStore.generateAiContent(
      fullPrompt,
    );
    if (result.ok) {
      generatedData.value = result.value;
    } else {
      generatedData.value = null;
      if (notificationTemplateStore.error) {
        notificationStore.showSnackbar(notificationTemplateStore.error, 'error');
      }
    }
  } catch (error) {
    console.error('Error generating data:', error);
    notificationStore.showSnackbar(
      t('notificationTemplate.errors.aiGenerationFailed'),
      'error',
    );
  } finally {
    loading.value = false;
  }
};

const save = () => {
  if (generatedData.value) {
    emit('generated', generatedData.value.subject, generatedData.value.body);
    emit('update:modelValue', false);
  }
};

const cancel = () => {
  notificationTemplateStore.error = null; // Clear error on cancel
  emit('update:modelValue', false);
};

const fillSamplePrompt = () => {
  let sample = t('notificationTemplate.form.aiPromptSample');
  if (getNotificationTypeText.value) {
    sample += `\nLoại sự kiện: ${getNotificationTypeText.value}`;
  }
  if (getNotificationChannelText.value) {
    sample += `\nKênh thông báo: ${getNotificationChannelText.value}`;
  }
  if (getTemplateFormatText.value) {
    sample += `\nĐịnh dạng: ${getTemplateFormatText.value}`;
  }
  if (props.language) {
    sample += `\nNgôn ngữ: ${props.language}`;
  }
  prompt.value = sample;
};
</script>
