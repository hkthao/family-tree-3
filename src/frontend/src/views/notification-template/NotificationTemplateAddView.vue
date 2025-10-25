<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useNotificationTemplateStore, useNotificationStore } from '@/stores';
import { NotificationTemplateForm, NLNotificationTemplateContentPopup } from '@/components/notification-template';
import type { NotificationTemplate } from '@/types';
import { storeToRefs } from 'pinia';

interface NotificationTemplateFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => Omit<NotificationTemplate, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>;
  setSubjectAndBody: (subject: string, body: string) => void;
}

const notificationTemplateFormRef = ref<NotificationTemplateFormExposed | null>(null);
const showAiContentPopup = ref(false);

const { t } = useI18n();
const router = useRouter();
const notificationTemplateStore = useNotificationTemplateStore();
const notificationStore = useNotificationStore();

const { loading } = storeToRefs(notificationTemplateStore);

const handleGenerateAiContent = async () => {
  showAiContentPopup.value = true;
};

const handleAiContentGenerated = (subject: string, body: string) => {
  if (notificationTemplateFormRef.value) {
    notificationTemplateFormRef.value.setSubjectAndBody(subject, body);
  }
};

const validateAndSave = async () => {
  if (!notificationTemplateFormRef.value) return;
  const isValid = await notificationTemplateFormRef.value.validate();
  if (isValid) {
    const itemData = notificationTemplateFormRef.value.getFormData();
    await handleAddItem(itemData);
  }
};

const handleAddItem = async (itemData: Omit<NotificationTemplate, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'>) => {
  try {
    await notificationTemplateStore.addItem(itemData);
    if (!notificationTemplateStore.error) {
      notificationStore.showSnackbar(
        t('notificationTemplate.messages.addSuccess'),
        'success',
      );
      closeForm();
    } else {
      notificationStore.showSnackbar(
        notificationTemplateStore.error || t('notificationTemplate.errors.add'),
        'error',
      );
    }
  } catch (err) {
    notificationStore.showSnackbar(
      t('notificationTemplate.errors.add'),
      'error',
    );
  }
};

const closeForm = () => {
  router.push({ name: 'NotificationTemplateList' });
};
</script>

<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('admin.notificationTemplates.form.addTitle')
      }}</span>
    </v-card-title>
    <v-card-text>
      <NotificationTemplateForm ref="notificationTemplateFormRef" @submit="handleAddItem" @cancel="closeForm" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{
        t('common.cancel')
      }}</v-btn>
      <v-btn color="blue-darken-1" data-testid="button-generate-ai" @click="handleGenerateAiContent" class="mr-2">
        {{ t('notificationTemplate.form.generateAiContent') }}
      </v-btn>
      <v-btn color="blue-darken-1" data-testid="button-save" @click="validateAndSave" :loading="loading">{{
        t('common.save') }}</v-btn>
    </v-card-actions>

    <NLNotificationTemplateContentPopup v-model="showAiContentPopup" @generated="handleAiContentGenerated" />
  </v-card>
</template>
