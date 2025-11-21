<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyDict.form.editTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="detail.loading || update.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <FamilyDictForm ref="familyDictFormRef" v-if="familyDict" :initial-family-dict-data="familyDict" @close="closeForm" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleUpdateFamilyDict" data-testid="save-family-dict-button" :loading="update.loading">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyDictStore } from '@/stores/family-dict.store';
import { FamilyDictForm } from '@/components/family-dict';
import type { FamilyDict } from '@/types';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar'; // Import useGlobalSnackbar

interface FamilyDictEditViewProps {
  familyDictId: string;
}

const props = defineProps<FamilyDictEditViewProps>();
const emit = defineEmits(['close', 'saved']);

const familyDictFormRef = ref<InstanceType<typeof FamilyDictForm> | null>(null);

const { t } = useI18n();
const familyDictStore = useFamilyDictStore();
const { showSnackbar } = useGlobalSnackbar(); // Khởi tạo useGlobalSnackbar

const { detail, update } = storeToRefs(familyDictStore);

const familyDict = ref<FamilyDict | undefined>(undefined);

const loadFamilyDict = async (id: string) => {
  await familyDictStore.getById(id);
  if (familyDictStore.detail.item)
    familyDict.value = familyDictStore.detail.item;
};

onMounted(async () => {
  if (props.familyDictId) {
    await loadFamilyDict(props.familyDictId);
  }
});

watch(
  () => props.familyDictId,
  async (newId) => {
    if (newId) {
      await loadFamilyDict(newId);
    }
  },
);

const handleUpdateFamilyDict = async () => {
  if (!familyDictFormRef.value) return;
  const isValid = await familyDictFormRef.value.validate();

  if (!isValid) {
    return;
  }

  const familyDictData = familyDictFormRef.value.getFormData() as FamilyDict;
  if (!familyDictData.id) {
    showSnackbar(t('familyDict.messages.saveError'), 'error');
    return;
  }

  try {
    await familyDictStore.updateItem(familyDictData as FamilyDict);
    if (!familyDictStore.error) {
      showSnackbar(t('familyDict.messages.updateSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(familyDictStore.error || t('familyDict.messages.saveError'), 'error');
    }
  } catch (error) {
    showSnackbar(t('familyDict.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  emit('close');
};
</script>
