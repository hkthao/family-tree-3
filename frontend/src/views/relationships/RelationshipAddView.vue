<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title class="d-flex align-center">
            {{ t('relationship.form.addTitle') }}
            <v-spacer></v-spacer>
            <v-btn icon="mdi-close" size="small" variant="text" @click="cancel"></v-btn>
          </v-card-title>
          <v-card-text>
            <RelationshipForm ref="relationshipForm" />
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" @click="save">{{ t('common.save') }}</v-btn>
            <v-btn @click="cancel">{{ t('common.cancel') }}</v-btn>
          </v-card-actions>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';

import { useRelationshipStore } from '@/stores/relationship.store';
import { useNotificationStore } from '@/stores/notification.store';

import type { Relationship } from '@/types';

import RelationshipForm from '@/components/relationships/RelationshipForm.vue';

const router = useRouter();
const { t } = useI18n();
const relationshipStore = useRelationshipStore();
const notificationStore = useNotificationStore();

const relationshipForm = ref<InstanceType<typeof RelationshipForm> | null>(null);

const save = async () => {
  const isValid = await relationshipForm.value?.validate();
  if (isValid) {
    const formData = relationshipForm.value?.getFormData();
    if (formData) {
      const result = await relationshipStore.addItem(formData as Omit<Relationship, 'id'>);
      if (result.ok) {
        notificationStore.showSnackbar(t('relationship.messages.addSuccess'), 'success');
        router.push({ name: 'RelationshipList' });
      } else {
        notificationStore.showSnackbar(result.error?.message || t('relationship.messages.saveError'), 'error');
      }
    }
  }
};

const cancel = () => {
  router.push({ name: 'RelationshipList' });
};
</script>
