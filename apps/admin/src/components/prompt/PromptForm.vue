<template>
  <v-form ref="formRef" :disabled="isFormReadOnly" data-testid="prompt-form">
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.code" :label="t('prompt.form.code')" :rules="rules.code"
          :readonly="isFormReadOnly" :disabled="isFormReadOnly" data-testid="prompt-code-input"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model="formData.title" :label="t('prompt.form.title')" :rules="rules.title"
          :readonly="isFormReadOnly" :disabled="isFormReadOnly" data-testid="prompt-title-input"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-textarea v-model="formData.content" :label="t('prompt.form.content')" :auto-grow="true" :rules="rules.content"
          :readonly="isFormReadOnly" :disabled="isFormReadOnly" data-testid="prompt-content-input"></v-textarea>
      </v-col>
      <v-col cols="12">
        <v-textarea 
        :rows="2"
        v-model="formData.description" :label="t('prompt.form.description')" :rules="rules.description"
          :readonly="isFormReadOnly" :disabled="isFormReadOnly" data-testid="prompt-description-input"></v-textarea>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { reactive, ref, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Prompt } from '@/types';
import { usePromptRules } from '@/validations/prompt.validation';
import { useAuth } from '@/composables';

const props = defineProps<{
  readOnly?: boolean;
  initialPromptData?: Prompt;
}>();
const { t } = useI18n();
const { isAdmin } = useAuth(); // Assuming only admin can manage prompts

const formRef = ref();

const isFormReadOnly = computed(() => {
  return props.readOnly || !isAdmin.value;
});

const formData = reactive<Omit<Prompt, 'id'> | Prompt>(
  props.initialPromptData
    ? {
      ...props.initialPromptData,
    }
    : {
      code: '',
      title: '',
      content: '',
      description: '',
    },
);

const rules = usePromptRules();

const validate = async () => {
  const { valid } = await formRef.value.validate();
  return valid;
};

const getFormData = () => {
  return formData;
};

defineExpose({
  validate,
  getFormData,
});
</script>
