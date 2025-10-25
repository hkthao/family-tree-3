<script setup lang="ts">
import { ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { NotificationChannel, NotificationType } from '@/types';
import type { NotificationTemplate } from '@/types';

interface Props {
  initialTemplateData?: NotificationTemplate;
  formTitle: string;
  readOnly?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  readOnly: false,
});

const emit = defineEmits<{ (e: 'save', payload: NotificationTemplate | Omit<NotificationTemplate, 'id'>): void; (e: 'cancel'): void }>();

const { t } = useI18n();

const formRef = ref<HTMLFormElement | null>(null);

const form = ref<Omit<NotificationTemplate, 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>>({
  eventType: NotificationType.General,
  channel: NotificationChannel.InApp,
  subject: '',
  body: '',
  isActive: true,
});

watch(
  () => props.initialTemplateData,
  (newVal) => {
    if (newVal) {
      form.value = { ...newVal };
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

const rules = {
  eventType: [(v: NotificationType) => v !== undefined && v !== null || t('notificationTemplate.form.validation.eventTypeRequired')],
  channel: [(v: NotificationChannel) => v !== undefined && v !== null || t('notificationTemplate.form.validation.channelRequired')],
  subject: [(v: string) => !!v || t('notificationTemplate.form.validation.subjectRequired')],
  body: [(v: string) => !!v || t('notificationTemplate.form.validation.bodyRequired')],
};

const validate = async () => {
  if (!formRef.value) return false;
  const { valid } = await formRef.value.validate();
  return valid;
};

const getFormData = () => {
  return form.value;
};

const submitForm = () => {
  emit('save', form.value);
};

const cancelForm = () => {
  emit('cancel');
};

defineExpose({
  validate,
  getFormData,
});
</script>

<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title>{{ formTitle }}</v-card-title>
          <v-card-text>
            <v-form ref="formRef" @submit.prevent="submitForm">
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

              <v-text-field
                v-model="form.subject"
                :label="t('notificationTemplate.form.subject')"
                :rules="rules.subject"
                :readonly="readOnly"
                required
              ></v-text-field>

              <v-textarea
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

              <v-card-actions v-if="!readOnly">
                <v-spacer></v-spacer>
                <v-btn color="blue-grey" @click="cancelForm">
                  {{ t('common.cancel') }}
                </v-btn>
                <v-btn color="primary" type="submit">
                  {{ t('common.save') }}
                </v-btn>
              </v-card-actions>
            </v-form>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>