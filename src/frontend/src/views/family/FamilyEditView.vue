<template>
  <v-card data-testid="family-edit-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('family.form.editTitle')
        }}</span>
    </v-card-title>
    <v-card-text>
      <FamilyForm ref="familyFormRef" v-if="family" :initial-family-data="family" :read-only="false" />
      <v-progress-circular v-else indeterminate color="primary"></v-progress-circular>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn
        color="primary"
        data-testid="button-save"
        @click="handleUpdateItem"
        >{{
        t('common.save')
        }}</v-btn
      >
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyStore } from '@/stores/family.store';
import { useNotificationStore } from '@/stores/notification.store';
import { FamilyForm } from '@/components/family';
import type { Family } from '@/types';

interface FamilyFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => Family | Omit<Family, 'id'>;
}

interface FamilyEditViewProps {
  familyId: string;
}

const props = defineProps<FamilyEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const family = ref<Family | undefined>(undefined);
const familyFormRef = ref<FamilyFormExposed | null>(null);

const { t } = useI18n();
const familyStore = useFamilyStore();
const notificationStore = useNotificationStore();

const loadFamily = async (id: string) => {
  await familyStore.getById(id);
  if (!familyStore.error) {
    family.value = familyStore.currentItem as Family;
  }
};

onMounted(async () => {
  if (props.familyId) {
    await loadFamily(props.familyId);
  }
});

watch(
  () => props.familyId,
  async (newId) => {
    if (newId) {
      await loadFamily(newId);
    }
  },
);

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
    if (!familyStore.error) {
      notificationStore.showSnackbar(
        t('family.management.messages.updateSuccess'),
        'success',
      );
      emit('saved'); // Emit saved event
    } else {
      notificationStore.showSnackbar(
        familyStore.error || t('family.management.messages.saveError'),
        'error',
      );
    }
  } catch (error) {
    notificationStore.showSnackbar(
      t('family.management.messages.saveError'),
      'error',
    );
  }
};

const closeForm = () => {
  emit('close'); // Emit close event
};
</script>
