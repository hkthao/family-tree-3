<template>
  <v-card>
    <v-card-title>{{ t('family.form.editLimitTitle') }}</v-card-title>
    <v-card-text>
      <v-form ref="formRef">
        <v-text-field v-model.number="formData.maxMembers" :label="t('family.form.maxMembers')" type="number"
          :rules="[rules.required, rules.positiveNumber]" data-testid="max-members-input"></v-text-field>
        <v-text-field v-model.number="formData.maxStorageMb" :label="t('family.form.maxStorageMb')" type="number"
          :rules="[rules.required, rules.positiveNumber]" data-testid="max-storage-mb-input"></v-text-field>
        <v-text-field v-model.number="formData.aiChatMonthlyLimit" :label="t('family.form.aiChatMonthlyLimit')"
          type="number" :rules="[rules.required, rules.positiveNumber]"
          data-testid="ai-chat-monthly-limit-input"></v-text-field>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="gray" @click="emit('close')">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="saveLimits">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VForm } from 'vuetify/components';
import { useServices } from '@/plugins/services.plugin';
import type { FamilyLimitConfiguration } from '@/types/family.d';
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar';
import { rules } from '@/utils/rules';

const { t } = useI18n();
const { family: familyService } = useServices();
const { showSnackbar } = useGlobalSnackbar();

const props = defineProps<{
  familyId: string;
}>();

const emit = defineEmits(['close', 'updated']);

const formRef = ref<VForm | null>(null);
const formData = ref<FamilyLimitConfiguration>({
  id: '',
  familyId: props.familyId,
  maxMembers: 0,
  maxStorageMb: 0,
  aiChatMonthlyLimit: 0,
  aiChatMonthlyUsage: 0, // This is usage, not editable here
});

const isLoading = ref(false);

const loadFamilyLimits = async () => {
  isLoading.value = true;
  const result = await familyService.getFamilyLimitConfiguration(props.familyId);
  if (result.ok && result.value) {
    formData.value = { ...result.value };
  } else {
    showSnackbar(t('family.form.errorLoadLimits'), 'error');
  }
  isLoading.value = false;
};

const saveLimits = async () => {
  const { valid } = await formRef.value!.validate();
  if (!valid) return;

  isLoading.value = true;
  const result = await familyService.updateFamilyLimitConfiguration(props.familyId, {
    maxMembers: formData.value.maxMembers,
    maxStorageMb: formData.value.maxStorageMb,
    aiChatMonthlyLimit: formData.value.aiChatMonthlyLimit,
  });

  if (result.ok) {
    showSnackbar(t('family.form.saveLimitSuccess'), 'success');
    emit('updated');
    emit('close');
  } else {
    showSnackbar(result.error.message || t('family.form.errorSaveLimits'), 'error');
  }
  isLoading.value = false;
};

onMounted(() => {
  if (props.familyId) {
    loadFamilyLimits();
  }
});

watch(() => props.familyId, (newFamilyId) => {
  if (newFamilyId) {
    loadFamilyLimits();
  }
});
</script>
