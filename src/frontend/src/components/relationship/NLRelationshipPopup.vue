<template>
  <v-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" max-width="800px"
    data-testid="nl-relationship-popup">
    <v-card>
      <v-card-title class="headline">{{ t('aiInput.title') }}</v-card-title>
      <v-card-text>
        <v-form>
          <v-textarea v-model="state.prompt" :label="t('aiInput.promptLabelRelationship')" rows="3" outlined clearable
            counter :auto-grow="true" @blur="v$.prompt.$touch()" @input="v$.prompt.$touch()"
            :error-messages="v$.prompt.$errors.map(e => e.$message as string)"
            data-testid="nl-relationship-prompt-input"></v-textarea>
        </v-form>
        <v-btn color="primary" :loading="loading" :disabled="loading || v$.$invalid" @click="generateData" class="mb-4"
          data-testid="nl-relationship-generate-button">
          {{ t('aiInput.generateButton') }}
        </v-btn>
        <v-btn color="info" @click="fillSamplePrompt" class="mb-4 ml-2"
          data-testid="nl-relationship-fill-sample-button">
          {{ t('aiInput.fillSampleButton') }}
        </v-btn>

        <div v-if="generatedData && generatedData.length">
          <v-alert type="info" class="mb-4">{{ t('aiInput.previewMessage') }}</v-alert>
          <div v-for="(relationship, relationshipIndex) in generatedData" :key="relationshipIndex"
            class="mb-6 pa-4 border rounded">
            <h4 class="text-h6 mb-2">{{ t('aiInput.relationship') }} #{{ relationshipIndex + 1 }}</h4>
            <v-alert v-if="relationship.validationErrors && relationship.validationErrors.length" type="warning"
              class="mb-2">
              <p>{{ t('aiInput.validationErrorsFound') }}</p>
              <ul>
                <li v-for="(error, errorIndex) in relationship.validationErrors" :key="errorIndex">{{ error }}</li>
              </ul>
            </v-alert>
            <v-divider class="mb-2"></v-divider>
            <p class="text-body-2">
              {{ relationship.sourceMemberFullName }} {{ t('relationship.isThe') }} {{ 
                getRelationshipTypeTitle(relationship.type) }} {{ t('relationship.of') }} {{ 
                relationship.targetMemberFullName }}
            </p>
            <p v-if="relationship.startDate" class="text-body-2">
              <strong>{{ t('relationship.startDate') }}:</strong> {{ relationship.startDate }}
            </p>
            <p v-if="relationship.endDate" class="text-body-2">
              <strong>{{ t('relationship.endDate') }}:</strong> {{ relationship.endDate }}
            </p>
            <p v-if="relationship.description" class="text-body-2">
              <strong>{{ t('relationship.description') }}:</strong> {{ relationship.description }}
            </p>
          </div>
        </div>
      </v-card-text>
      <v-card-actions>
        <v-spacer></v-spacer>
        <v-btn color="grey-darken-1" @click="cancel" :disabled="loading" data-testid="nl-relationship-cancel-button">{{ 
          t('aiInput.cancelButton')
          }}</v-btn>
        <v-btn color="primary" :disabled="!generatedData || !generatedData.length || loading || hasValidationErrors"
          @click="save" data-testid="nl-relationship-save-button">{{ 
            t('aiInput.saveButton') }}
        </v-btn>
      </v-card-actions>
      <v-progress-linear v-if="loading" indeterminate color="primary" height="4" class="mb-0"></v-progress-linear>
    </v-card>
  </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch, computed, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import { useVuelidate } from '@vuelidate/core';
import { useNLRelationshipRules } from '@/validations/nl-relationship.validation';
import { useNaturalLanguageInputStore } from '@/stores';
import { useRelationshipStore } from '@/stores/relationship.store';
import { useNotificationStore } from '@/stores/notification.store';
import type { Relationship } from '@/types';
import { getRelationshipTypeTitle } from '@/constants/relationshipTypes';

const props = defineProps({
  modelValue: Boolean,
});

const emit = defineEmits([
  'update:modelValue',
  'saved',
]);

const { t } = useI18n();
const naturalLanguageInputStore = useNaturalLanguageInputStore();
const relationshipStore = useRelationshipStore();
const notificationStore = useNotificationStore();

const state = reactive({
  prompt: '',
});

const rules = useNLRelationshipRules();

const v$ = useVuelidate(rules, state);

const generatedData = ref<Relationship[] | null>(null);
const loading = ref(false);

const hasValidationErrors = computed(() => {
  return generatedData.value?.some(relationship => relationship.validationErrors && relationship.validationErrors.length > 0) || false;
});

watch(() => props.modelValue, (newValue) => {
  if (newValue) {
    v$.value.$reset();
    state.prompt = '';
    generatedData.value = null;
  }
});

const generateData = async () => {
  const result = await v$.value.$validate();
  if (!result) {
    return;
  }

  loading.value = true;
  naturalLanguageInputStore.error = null;
  try {
    const response = await naturalLanguageInputStore.generateRelationshipData(state.prompt);
    if (response) {
      generatedData.value = response;
    } else {
      generatedData.value = null;
      if (naturalLanguageInputStore.error) {
        notificationStore.showSnackbar(naturalLanguageInputStore.error, 'error');
      }
    }
  } catch (error) {
    console.error('Error generating data:', error);
    notificationStore.showSnackbar(
      t('aiInput.generateError', { entity: t('aiInput.relationships') }),
      'error',
    );
  } finally {
    loading.value = false;
  }
};

const save = async () => {
  if (!generatedData.value || !generatedData.value.length) return;

  try {
    await relationshipStore.addItems(generatedData.value);
    notificationStore.showSnackbar(
      t('aiInput.saveSuccess'),
      'success',
    );
    emit('saved');
    emit('update:modelValue', false);
  } catch (error) {
    console.error('Error saving generated data:', error);
    notificationStore.showSnackbar(
      t('aiInput.saveError'),
      'error',
    );
  }
};

const cancel = () => {
  naturalLanguageInputStore.error = null; // Clear error on cancel
  emit('update:modelValue', false);
};

const fillSamplePrompt = () => {
  state.prompt = "Tạo mối quan hệ giữa Nguyễn Văn A và Trần Thị B. Nguyễn Văn A là chồng của Trần Thị B, kết hôn vào ngày 2000-01-15. Mối quan hệ này được mô tả là một cặp vợ chồng hạnh phúc.\n\nTạo mối quan hệ giữa Nguyễn Văn A và Nguyễn Văn C. Nguyễn Văn A là cha của Nguyễn Văn C, sinh năm 2005. Nguyễn Văn C là con thứ nhất.";
};
</script>
