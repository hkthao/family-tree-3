<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { NotificationChannel, NotificationType, TemplateFormat } from '@/types';
import type { NotificationTemplate } from '@/types';
import Editor from '@/components/common/Editor.vue';

interface Props {
  initialTemplateData?: NotificationTemplate;
  readOnly?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  readOnly: false,
});

const emit = defineEmits<{ (e: 'submit', payload: NotificationTemplate | Omit<NotificationTemplate, 'id'>): void; (e: 'cancel'): void }>();

const { t } = useI18n();

const formRef = ref<HTMLFormElement | null>(null);

const form = ref<Omit<NotificationTemplate, 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy' | 'placeholders'>>({
  eventType: NotificationType.General,
  channel: NotificationChannel.InApp,
  subject: '',
  body: '',
  format: TemplateFormat.PlainText,
  languageCode: 'vi', // Default to Vietnamese
  isActive: true,
});

watch(
  () => props.initialTemplateData,
  (newVal) => {
    if (newVal) {
      form.value = { ...newVal };
      // If the format is HTML, parse the body string into an object for the editor
      if (form.value.format === TemplateFormat.Html && typeof form.value.body === 'string') {
        try {
          form.value.body = JSON.parse(form.value.body);
        } catch (e) {
          console.error("Error parsing initial HTML body data:", e);
          form.value.body = { blocks: [] };
        }
      }
    }
  },
  { immediate: true, deep: true },
);

const notificationTypes = Object.keys(NotificationType)
  .filter(key => isNaN(Number(key)))
  .map(key => ({
    title: t(`notificationType.${key}`),
    value: NotificationType[key as keyof typeof NotificationType],
  }));

const notificationChannels = Object.keys(NotificationChannel)
  .filter(key => isNaN(Number(key)))
  .map(key => ({
    title: t(`notificationChannel.${key}`),
    value: NotificationChannel[key as keyof typeof NotificationChannel],
  }));

const templateFormats = Object.keys(TemplateFormat)
  .filter(key => isNaN(Number(key)))
  .map(key => ({
    title: t(`templateFormat.${key}`),
    value: TemplateFormat[key as keyof typeof TemplateFormat],
  }));

const languageOptions = computed(() => [
  { title: t('common.language.en'), value: 'en' },
  { title: t('common.language.vi'), value: 'vi' },
]);

const rules = {
  eventType: [(v: NotificationType) => v !== undefined && v !== null || t('notificationTemplate.form.validation.eventTypeRequired')],
  channel: [(v: NotificationChannel) => v !== undefined && v !== null || t('notificationTemplate.form.validation.channelRequired')],
  subject: [(v: string) => !!v || t('notificationTemplate.form.validation.subjectRequired')],
  body: [(v: string | object) => !!v || t('notificationTemplate.form.validation.bodyRequired')], // Adjusted rule for object type
  format: [(v: TemplateFormat) => v !== undefined && v !== null || t('notificationTemplate.form.validation.formatRequired')],
  languageCode: [(v: string) => !!v || t('notificationTemplate.form.validation.languageCodeRequired')],
};

const validate = async () => {
  if (!formRef.value) return false;
  const { valid } = await formRef.value.validate();
  return valid;
};

const getFormData = () => {
  const formData = { ...form.value };
  // If the format is HTML, stringify the body object before returning
  if (formData.format === TemplateFormat.Html && typeof formData.body === 'object') {
    formData.body = JSON.stringify(formData.body);
  }
  return formData;
};

const submitForm = async () => {
  const { valid } = await formRef.value!.validate();
  if (valid) {
    emit('submit', getFormData()); // Emit processed data
  }
};

defineExpose({
  validate,
  getFormData,
});
</script>

<template>
  <v-form ref="formRef" @submit.prevent="submitForm" :disabled="props.readOnly">
    <v-row>
      <v-col cols="12" md="6">
        <v-select
          v-model="form.eventType"
          :items="notificationTypes"
          item-title="title"
          item-value="value"
          :label="t('notificationTemplate.form.eventType')"
          :rules="rules.eventType"
          :readonly="readOnly"
          required
        ></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <v-select
          v-model="form.channel"
          :items="notificationChannels"
          item-title="title"
          item-value="value"
          :label="t('notificationTemplate.form.channel')"
          :rules="rules.channel"
          :readonly="readOnly"
          required
        ></v-select>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12" md="6">
        <v-select
          v-model="form.format"
          :items="templateFormats"
          item-title="title"
          item-value="value"
          :label="t('notificationTemplate.form.format')"
          :rules="rules.format"
          :readonly="readOnly"
          required
        ></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <v-select
          v-model="form.languageCode"
          :items="languageOptions"
          item-title="title"
          item-value="value"
          :label="t('notificationTemplate.form.languageCode')"
          :rules="rules.languageCode"
          :readonly="readOnly"
          required
        ></v-select>
      </v-col>
    </v-row>

    <v-text-field
      v-model="form.subject"
      :label="t('notificationTemplate.form.subject')"
      :rules="rules.subject"
      :readonly="readOnly"
      required
    ></v-text-field>

    <Editor
      v-if="form.format === TemplateFormat.Html"
      v-model="form.body"
      :label="t('notificationTemplate.form.body')"
      :read-only="readOnly"
    />
    <v-textarea
      v-else
      v-model="form.body"
      :label="t('notificationTemplate.form.body')"
      :rules="rules.body"
      :readonly="readOnly"
      required
    ></v-textarea>

    <v-checkbox
      v-model="form.isActive"
      :label="t('notificationTemplate.form.isActive')"
      :readonly="readOnly"
    ></v-checkbox>
  </v-form>
</template>