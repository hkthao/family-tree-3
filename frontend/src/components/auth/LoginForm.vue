<template>
  <v-form @submit.prevent="handleLogin">
    <v-text-field
      v-model="email"
      :label="t('login.email')"
      :placeholder="t('login.emailPlaceholder')"
      prepend-inner-icon="mdi-email-outline"
      type="email"
      :rules="[rules.required, rules.email]"
    ></v-text-field>
    <v-text-field
      class="mt-4"
      v-model="password"
      :label="t('login.password')"
      :type="showPassword ? 'text' : 'password'"
      prepend-inner-icon="mdi-lock-outline"
      :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
      @click:append-inner="showPassword = !showPassword"
      :rules="[rules.required]"
    ></v-text-field>

    <div class="d-flex justify-space-between align-center">
      <v-checkbox
        v-model="rememberMe"
        :label="t('login.rememberMe')"
        hide-details
      ></v-checkbox>
      <a href="#" class="text-primary">{{ t('login.forgotPassword') }}</a>
    </div>

    <v-btn type="submit" block color="primary" class="mt-4">{{
      t('login.login')
    }}</v-btn>
  </v-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { useAuthStore } from '@/stores/auth.store';

const { t } = useI18n();
const email = ref('');
const password = ref('');
const rememberMe = ref(false);
const showPassword = ref(false);
const router = useRouter();

const rules = {
  required: (value: string) => !!value || t('validation.required'),
  email: (value: string) => /.+@.+\..+/.test(value) || t('validation.email'),
};

import { useNotificationStore } from '@/stores/notification.store';

const notificationStore = useNotificationStore();

const handleLogin = async () => {
  const authStore = useAuthStore();
  await authStore.login({ email: email.value, password: password.value });

  if (authStore.isAuthenticated) {
    notificationStore.showSnackbar(t('login.success'), 'success');
    router.push('/dashboard');
  } else {
    notificationStore.showSnackbar(
      authStore.error || t('login.invalidCredentials'),
      'error',
    );
  }
};
</script>
