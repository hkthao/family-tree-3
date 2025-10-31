<template>
  <v-card data-testid="relationship-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('relationship.form.addTitle')
      }}</span>
    </v-card-title>
    <v-card-text>
      <RelationshipForm ref="relationshipFormRef" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey"  @click="closeForm" data-testid="relationship-add-cancel-button">{{
        t('common.cancel')
      }}</v-btn>
      <v-btn color="blue-darken-1"  @click="handleAddItem" data-testid="relationship-add-save-button">{{
        t('common.save')
      }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useRelationshipStore } from '@/stores/relationship.store';
import { useNotificationStore } from '@/stores/notification.store';
import { RelationshipForm } from '@/components/relationship';
import type { Relationship } from '@/types';

const relationshipFormRef = ref<InstanceType<typeof RelationshipForm> | null>(null);

const { t } = useI18n();
const router = useRouter();
const relationshipStore = useRelationshipStore();
const notificationStore = useNotificationStore();

const handleAddItem = async () => {
  if (!relationshipFormRef.value) return;
  const isValid = await relationshipFormRef.value.validate();
  if (!isValid) return;
  const itemData = relationshipFormRef.value.getFormData();
  try {
    await relationshipStore.addItem(itemData as Omit<Relationship, 'id'>);
    if (!relationshipStore.error) {
      notificationStore.showSnackbar(
        t('relationship.messages.addSuccess'),
        'success',
      );
      closeForm();
    } else {
      notificationStore.showSnackbar(
        relationshipStore.error || t('relationship.messages.saveError'),
        'error',
      );
    }
  } catch (error) {
    notificationStore.showSnackbar(
      t('relationship.messages.saveError'),
      'error',
    );
  }
};

const closeForm = () => {
  router.push('/relationship');
};
</script>
