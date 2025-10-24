<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('admin.notificationTemplates.form.editTitle')
      }}</span>
    </v-card-title>
    <v-card-text>
      <NotificationTemplateForm
        ref="notificationTemplateFormRef"
        v-if="currentItem"
        :initial-template-data="currentItem"
        @cancel="closeForm"
        @save="handleUpdateItem"
      />
      <v-progress-circular v-else indeterminate color="primary"></v-progress-circular>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{
        t('common.cancel')
      }}</v-btn>
      <v-btn
        color="blue-darken-1"
        data-testid="button-save"
        @click="validateAndSave"
        :loading="loading"
        >{{ t('common.save') }}</v-btn
      >
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useNotificationTemplateStore } from '@/stores/notificationTemplate.store';
import { useNotificationStore } from '@/stores/notification.store';
import { NotificationTemplateForm } from '@/views/notificationTemplate';
import type { NotificationTemplate } from '@/types';
import { storeToRefs } from 'pinia';

interface NotificationTemplateFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => NotificationTemplate;
}

const notificationTemplateFormRef = ref<NotificationTemplateFormExposed | null>(null);

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const notificationTemplateStore = useNotificationTemplateStore();
const notificationStore = useNotificationStore();

const { currentItem, loading, error } = storeToRefs(notificationTemplateStore);

onMounted(async () => {
  const templateId = route.params.id as string;
  if (templateId) {
    await notificationTemplateStore.getById(templateId);
  }
});

const validateAndSave = async () => {
  if (!notificationTemplateFormRef.value) return;
  const isValid = await notificationTemplateFormRef.value.validate();
  if (isValid) {
    const itemData = notificationTemplateFormRef.value.getFormData();
    await handleUpdateItem(itemData);
  }
};

const handleUpdateItem = async (itemData: NotificationTemplate) => {
  if (!itemData.id) {
    notificationStore.showSnackbar(
      t('notificationTemplate.errors.update'),
      'error',
    );
    return;
  }

  try {
    await notificationTemplateStore.updateItem(itemData);
    if (!notificationTemplateStore.error) {
      notificationStore.showSnackbar(
        t('notificationTemplate.messages.updateSuccess'),
        'success',
      );
      closeForm();
    } else {
      notificationStore.showSnackbar(
        notificationTemplateStore.error || t('notificationTemplate.errors.update'),
        'error',
      );
    }
  } catch (err) {
    notificationStore.showSnackbar(
      t('notificationTemplate.errors.update'),
      'error',
    );
  }
};

const closeForm = () => {
  router.push({ name: 'NotificationTemplateList' });
};
</script>
