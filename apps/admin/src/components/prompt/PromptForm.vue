<template>
  <v-form ref="formRef" :disabled="isFormReadOnly" data-testid="prompt-form">
    <v-row>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.code" :label="t('prompt.form.code')" @blur="v$.code.$touch()"
          @input="v$.code.$touch()" :error-messages="v$.code.$errors.map(e => e.$message as string)"
          :readonly="isFormReadOnly" :disabled="isFormReadOnly" data-testid="prompt-code-input"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.title" :label="t('prompt.form.title')" @blur="v$.title.$touch()"
          @input="v$.title.$touch()" :error-messages="v$.title.$errors.map(e => e.$message as string)"
          :readonly="isFormReadOnly" :disabled="isFormReadOnly" data-testid="prompt-title-input"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-textarea v-model="formData.content" :label="t('prompt.form.content')" @blur="v$.content.$touch()"
          @input="v$.content.$touch()" :error-messages="v$.content.$errors.map(e => e.$message as string)"
          :readonly="isFormReadOnly" :disabled="isFormReadOnly" data-testid="prompt-content-input"></v-textarea>
      </v-col>
      <v-col cols="12">
        <v-textarea v-model="formData.description" :label="t('prompt.form.description')"
          @blur="v$.description.$touch()" @input="v$.description.$touch()"
          :error-messages="v$.description.$errors.map(e => e.$message as string)" :readonly="isFormReadOnly"
          :disabled="isFormReadOnly" data-testid="prompt-description-input"></v-textarea>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { reactive, toRefs, ref, toRef, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import type { Prompt } from '@/types';
import { useVuelidate } from '@vuelidate/core';
import { usePromptRules } from '@/validations/prompt.validation'; // This file needs to be created
import { useAuth } from '@/composables/useAuth';

const props = defineProps<{
  readOnly?: boolean;
  initialPromptData?: Prompt;
}>();

const { t } = useI18n();
const { isAdmin } = useAuth(); // Assuming only admin can manage prompts

const formRef = ref<HTMLFormElement | null>(null);

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

const state = reactive({
  code: toRef(formData, 'code'),
  title: toRef(formData, 'title'),
  content: toRef(formData, 'content'),
  description: toRef(formData, 'description'),
});

const rules = usePromptRules(toRefs(state));

const v$ = useVuelidate(rules, state);

const validate = async () => {
  const result = await v$.value.$validate();
  return result;
};

const getFormData = () => {
  return formData;
};

defineExpose({
  validate,
  getFormData,
  v$,
});
</script>
