<template>
  <v-form ref="securityForm" @submit.prevent="saveSecurity">
    <v-text-field
      v-model="securityForm.currentPassword"
      :label="t('userSettings.security.currentPassword')"
      :type="showCurrentPassword ? 'text' : 'password'"
      :append-inner-icon="showCurrentPassword ? 'mdi-eye-off' : 'mdi-eye'"
      @click:append-inner="showCurrentPassword = !showCurrentPassword"
      :rules="[rules.required]"
      class="mb-2"
    ></v-text-field>
    <v-text-field
      v-model="securityForm.newPassword"
      :label="t('userSettings.security.newPassword')"
      :type="showNewPassword ? 'text' : 'password'"
      :append-inner-icon="showNewPassword ? 'mdi-eye-off' : 'mdi-eye'"
      @click:append-inner="showNewPassword = !showNewPassword"
      :rules="[rules.required, rules.min(8)]"
      class="mb-2"
    ></v-text-field>
    <v-text-field
      v-model="securityForm.confirmPassword"
      :label="t('userSettings.security.confirmPassword')"
      :type="showConfirmPassword ? 'text' : 'password'"
      :append-inner-icon="showConfirmPassword ? 'mdi-eye-off' : 'mdi-eye'"
      @click:append-inner="showConfirmPassword = !showConfirmPassword"
      :rules="[rules.required, rules.passwordMatch]"
      class="mb-2"
    ></v-text-field>
    <v-row>
      <v-col cols="12" class="text-right">
        <v-btn color="primary" type="submit">{{ t('common.save') }}</v-btn>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useNotificationStore } from '@/stores/notification.store';

const { t } = useI18n();
const notificationStore = useNotificationStore();

const securityForm = ref({
  currentPassword: '',
  newPassword: '',
  confirmPassword: '',
});

const securityFormRef = ref<HTMLFormElement | null>(null);

const showCurrentPassword = ref(false);
const showNewPassword = ref(false);
const showConfirmPassword = ref(false);

const rules = {
  required: (value: string) => !!value || t('validation.required'),
  min: (length: number) => (value: string) =>
    value.length >= length ||
    t('userSettings.security.passwordMinLength', { length }),
  passwordMatch: (value: string) =>
    value === securityForm.value.newPassword ||
    t('userSettings.security.passwordMismatch'),
};

const saveSecurity = async () => {
  if (securityFormRef.value) {
    const { valid } = await securityFormRef.value.validate();
    if (valid) {
      // Simulate API call
      console.log('Saving security settings:', securityForm.value);
      notificationStore.showSnackbar(
        t('userSettings.security.saveSuccess'),
        'success',
      );
      // Clear form
      securityForm.value.currentPassword = '';
      securityForm.value.newPassword = '';
      securityForm.value.confirmPassword = '';
    } else {
      notificationStore.showSnackbar(
        t('userSettings.security.saveError'),
        'error',
      );
    }
  }
};
</script>
