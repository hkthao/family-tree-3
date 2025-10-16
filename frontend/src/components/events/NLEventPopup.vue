<template>
  <v-dialog elevation="10" :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" max-width="800px">
    <v-card >
      <v-card-title class="headline">{{ t('aiInput.title') }}</v-card-title>
      <v-card-text>
        <v-form ref="form">
          <v-textarea v-model="prompt" :label="t('aiInput.promptLabelEvent')" rows="3" outlined clearable counter
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
          <div v-for="(event, eventIndex) in generatedData" :key="eventIndex" class="mb-6 pa-4 border rounded">
            <h4 class="text-h6 mb-2">{{ t('aiInput.event') }} #{{ eventIndex + 1 }}</h4>
            <v-alert v-if="event.validationErrors && event.validationErrors.length" type="warning" class="mb-2">
              <p>{{ t('aiInput.validationErrorsFound') }}</p>
              <ul>
                <li v-for="(error, errorIndex) in event.validationErrors" :key="errorIndex">{{ error }}</li>
              </ul>
            </v-alert>
            <v-divider class="mb-2"></v-divider>
            <div v-for="key in displayKeys" :key="key">
              <p class="text-body-2">
                <strong>{{ t(`event.${key}`) || key }}:</strong>
                {{ formatValue(event[key as keyof Event], key) }}
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
import { useEventStore } from '@/stores/event.store'; // Assuming an event store exists
import { useNotificationStore } from '@/stores/notification.store';
import type { Event } from '@/types';

const props = defineProps({
  modelValue: Boolean,
});

const emit = defineEmits([
  'update:modelValue',
  'saved',
]);

const { t } = useI18n();
const naturalLanguageInputStore = useNaturalLanguageInputStore();
const eventStore = useEventStore(); // Use the event store
const notificationStore = useNotificationStore();

const prompt = ref('');
const generatedData = ref<Event[] | null>(null);
const loading = ref(false);
const form = ref<HTMLFormElement | null>(null);

const displayKeys = [
  'name',
  'type',
  'startDate',
  'endDate',
  'location',
  'description',
  'familyName',
  'relatedMembers',
];

const hasValidationErrors = computed(() => {
  return generatedData.value?.some((event: Event) => event.validationErrors && event.validationErrors.length > 0) || false;
});

const formatValue = (value: any, key: string) => {
  if (value === null || value === '') {
    return t('common.unknown');
  }
  if (key === 'type') {
    return t(`event.type.${value.toLowerCase()}`);
  }
  if (key === 'startDate' || key === 'endDate') {
    return value ? new Date(value).toLocaleDateString() : t('common.unknown');
  }
  if (key === 'relatedMembers') {
    return value && value.length > 0 ? value.join(', ') : t('common.na');
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
    const result = await naturalLanguageInputStore.generateEventData(prompt.value);
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
      t('aiInput.generateError', { entity: t('aiInput.events') }),
      'error',
    );
  } finally {
    loading.value = false;
  }
};

const save = async () => {
  if (!generatedData.value || !generatedData.value.length) return;

  try {
    // Assuming eventStore has an addItems method for multiple events
    await eventStore.addItems(generatedData.value);
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
  prompt.value = `Thêm sự kiện tên "Lễ kỷ niệm 50 năm thành lập gia đình Nguyễn", loại "Other", bắt đầu vào ngày "2025-10-26" và kết thúc vào ngày "2025-10-27", tại "Trung tâm hội nghị Quốc gia, Hà Nội". Mô tả: "Sự kiện trọng đại kỷ niệm nửa thế kỷ hình thành và phát triển của gia đình Nguyễn, với sự tham gia của đông đảo thành viên và khách quý.". Thuộc gia đình "Nguyễn". Thành viên liên quan: "Trần Văn A", "Nguyễn Thị B".`;
};
</script>
