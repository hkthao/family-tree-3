<template>
  <v-card>
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('family.form.editTitle')
      }}</span>
    </v-card-title>
    <v-card-text>
      <FamilyForm
        ref="familyFormRef"
        v-if="family"
        :initial-family-data="family"
        :read-only="false"
      />
      <v-progress-circular
        v-else
        indeterminate
        color="primary"
      ></v-progress-circular>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="blue-darken-1" @click="handleUpdateItem">{{
        t('common.save')
      }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useRoute, useRouter } from 'vue-router';
import { useFamilyStore } from '@/stores/family.store';
import { useNotificationStore } from '@/stores/notification.store';
import { FamilyForm } from '@/components/family';
import type { Family } from '@/types';

interface FamilyFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => Family | Omit<Family, 'id'>;
}

const family = ref<Family | undefined>(undefined);
const familyFormRef = ref<FamilyFormExposed | null>(null);

const { t } = useI18n();
const route = useRoute();
const router = useRouter();
const familyStore = useFamilyStore();
const notificationStore = useNotificationStore();

onMounted(async () => {
  const itemId = route.params.id as string;
  if (itemId) {
    family.value = await familyStore.getItemById(itemId);
  }
});

const handleUpdateItem = async () => {
  if (!familyFormRef.value) return;
  const isValid = await familyFormRef.value.validate();
  if (!isValid) return;

  const itemData = familyFormRef.value.getFormData() as Family;
  if (!itemData.id) {
    notificationStore.showSnackbar(
      t('family.management.messages.saveError'),
      'error',
    );
    return;
  }

  try {
    await familyStore.updateItem(itemData);
    notificationStore.showSnackbar(
      t('family.management.messages.updateSuccess'),
      'success',
    );
    closeForm();
  } catch (error) {
    notificationStore.showSnackbar(
      t('family.management.messages.saveError'),
      'error',
    );
  }
};

const closeForm = () => {
  router.push('/family');
};
</script>
