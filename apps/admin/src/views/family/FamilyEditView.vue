<template>
  <v-card :elevation="0" data-testid="family-edit-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('family.form.editTitle')
        }}</span>
    </v-card-title>
    <v-card-text>
      <FamilyForm ref="familyFormRef" v-if="props.initialFamily" :initial-family-data="props.initialFamily" :read-only="false" />
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
import { FamilyForm } from '@/components/family';
import type { Family } from '@/types';
import { useGlobalSnackbar } from '@/composables';

interface FamilyFormExposed {
  validate: () => Promise<boolean>;
  getFormData: () => Family | Omit<Family, 'id'>;
}

interface FamilyEditViewProps {
  initialFamily?: Family; // Make optional for better flexibility
  initialFamilyId?: string; // New prop to fetch family if not provided
}

const props = defineProps<FamilyEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const familyFormRef = ref<FamilyFormExposed | null>(null);

const { t } = useI18n();
const familyStore = useFamilyStore();
const { showSnackbar } = useGlobalSnackbar();

const family = ref<Family | undefined>(undefined); // Internal ref for family data

// Fetch family if initialFamilyId is provided or if initialFamily is not
const loadFamily = async (id: string) => {
  await familyStore.getById(id);
  if (!familyStore.error) {
    family.value = familyStore.detail.item as Family;
  } else {
    family.value = undefined; // Clear family on error
  }
};

onMounted(() => {
  if (props.initialFamily) {
    family.value = props.initialFamily;
  } else if (props.initialFamilyId) {
    loadFamily(props.initialFamilyId);
  }
});

watch(
  () => props.initialFamilyId,
  (newId) => {
    if (newId) {
      loadFamily(newId);
    }
  },
);

watch(
  () => props.initialFamily,
  (newFamily) => {
    if (newFamily) {
      family.value = newFamily;
    }
  },
);

const handleUpdateItem = async () => {
  if (!familyFormRef.value) return;
  const isValid = await familyFormRef.value.validate();
  if (!isValid) return;

  const itemData = familyFormRef.value.getFormData() as Family;
  if (!itemData.id) {
    showSnackbar(
      t('family.management.messages.saveError'),
      'error',
    );
    return;
  }

  try {
    await familyStore.updateItem(itemData);
    if (!familyStore.error) {
      showSnackbar(
        t('family.management.messages.updateSuccess'),
        'success',
      );
      emit('saved'); // Emit saved event
    } else {
      showSnackbar(
        familyStore.error || t('family.management.messages.saveError'),
        'error',
      );
    }
  } catch (error) {
    showSnackbar(
      t('family.management.messages.saveError'),
      'error',
    );
  }
};

const closeForm = () => {
  emit('close'); // Emit close event
};
</script>
