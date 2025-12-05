<template>
  <v-card :elevation="0" data-testid="family-link-request-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('familyLinkRequest.form.addTitle') }}</span>
    </v-card-title>
    <v-progress-linear v-if="add.loading" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <!-- This form will only handle requestingFamilyId and targetFamilyId -->
      <v-form ref="formRef">
        <v-row>
          <v-col cols="12" md="6">
            <FamilyAutocomplete v-model="requestingFamilyId" :label="t('familyLinkRequest.form.requestingFamily')"
              :rules="[(v: string | null | undefined) => !!v || t('familyLinkRequest.form.rules.requestingFamilyRequired')]"
              data-testid="requesting-family-field" />
          </v-col>
          <v-col cols="12" md="6">
            <FamilyAutocomplete v-model="targetFamilyId" :label="t('familyLinkRequest.form.targetFamily')"
              :rules="[(v: string | null | undefined) => !!v || t('familyLinkRequest.form.rules.targetFamilyRequired')]"
              data-testid="target-family-field" />
          </v-col>
          <v-col cols="12">
            <v-textarea v-model="requestMessage" :label="t('familyLinkRequest.form.requestMessage')" rows="3" clearable
              counter maxlength="500" data-testid="request-message-field"></v-textarea>
          </v-col>
        </v-row>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleAddFamilyLinkRequest" data-testid="save-family-link-request-button"
        :loading="add.loading">{{
          t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useFamilyLinkRequestStore } from '@/stores/familyLinkRequest.store';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';
import { storeToRefs } from 'pinia';
import { useGlobalSnackbar } from '@/composables/useGlobalSnackbar';

const emit = defineEmits(['close', 'saved']);

const formRef = ref<HTMLFormElement | null>(null);
const requestingFamilyId = ref<string | null>(null);
const targetFamilyId = ref<string | null>(null);
const requestMessage = ref<string | null>(null);

const { t } = useI18n();
const familyLinkRequestStore = useFamilyLinkRequestStore();
const { showSnackbar } = useGlobalSnackbar();

const { add } = storeToRefs(familyLinkRequestStore);

onMounted(async () => {
  // No initial data to process
});

const handleAddFamilyLinkRequest = async () => {
  if (!formRef.value) return;
  const { valid } = await formRef.value.validate();

  if (!valid) {
    return;
  }

  if (!requestingFamilyId.value || !targetFamilyId.value) {
    showSnackbar(t('familyLinkRequest.form.rules.allFieldsRequired'), 'error');
    return;
  }

  try {
    const result = await familyLinkRequestStore.createRequest(requestingFamilyId.value, targetFamilyId.value, requestMessage.value || undefined);
    if (result.ok) {
      showSnackbar(t('familyLinkRequest.requests.messages.sendSuccess'), 'success');
      emit('saved');
    } else {
      showSnackbar(result.error?.message || t('familyLinkRequest.requests.messages.sendError'), 'error');
    }
  } catch (error) {
    showSnackbar(t('familyLinkRequest.requests.messages.sendError'), 'error');
  }
};

const closeForm = () => {
  emit('close');
};
</script>