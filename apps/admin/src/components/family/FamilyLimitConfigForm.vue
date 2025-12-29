<template>
  <v-form ref="formRef">
    <v-row>
      <v-col cols="6">
        <v-text-field v-model.number="internalFamilyLimitData.maxMembers" :label="t('family.form.maxMembers')"
          type="number" :rules="[rules.required, rules.positiveNumber]" data-testid="max-members-input"></v-text-field>
      </v-col>
      <v-col cols="6">
        <v-text-field v-model.number="internalFamilyLimitData.maxStorageMb" :label="t('family.form.maxStorageMb')"
          type="number" :rules="[rules.required, rules.positiveNumber]"
          data-testid="max-storage-mb-input"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model.number="internalFamilyLimitData.aiChatMonthlyLimit"
          :label="t('family.form.aiChatMonthlyLimit')" type="number" :rules="[rules.required, rules.positiveNumber]"
          data-testid="ai-chat-monthly-limit-input"></v-text-field>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, watch, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VForm } from 'vuetify/components';
import type { FamilyLimitConfiguration } from '@/types/family.d';
import { rules } from '@/utils/rules';

const { t } = useI18n();

const props = defineProps<{
  familyId: string;
  familyLimitData: FamilyLimitConfiguration;
  isSaving: boolean;
}>();

const formRef = ref<VForm | null>(null);

const internalFamilyLimitData = reactive<FamilyLimitConfiguration>({ ...props.familyLimitData });

watch(() => props.familyLimitData, (newVal) => {
  Object.assign(internalFamilyLimitData, newVal);
}, { deep: true });

const validate = async () => {
  const { valid } = await formRef.value!.validate();
  return valid;
};

defineExpose({
  validate,
  formData: internalFamilyLimitData, // Expose internalFamilyLimitData for parent access
});
</script>

<style scoped></style>
