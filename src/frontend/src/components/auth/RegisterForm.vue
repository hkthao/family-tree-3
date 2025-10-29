<template>
  <v-form @submit.prevent="handleRegister">
    <v-text-field
      v-model="username"
      label="Username"
      :rules="[rules.required]"
    ></v-text-field>

    <v-text-field
      v-model="email"
      label="Email"
      type="email"
      :rules="[rules.required, rules.email]"
    ></v-text-field>

    <v-text-field
      v-model="password"
      label="Password"
      :type="showPassword ? 'text' : 'password'"
      :append-inner-icon="showPassword ? 'mdi-eye-off' : 'mdi-eye'"
      @click:append-inner="showPassword = !showPassword"
      :rules="[rules.required]"
    ></v-text-field>

    <v-checkbox v-model="agreeToTerms" :rules="[rules.required]">
      <template #label>
        <div>
          I agree to
          <a href="#" @click.prevent>privacy policy & terms</a>
        </div>
      </template>
    </v-checkbox>

    <v-btn type="submit" block color="primary" class="mt-4">Sign Up</v-btn>
  </v-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore, useNotificationStore } from '@/stores';
import { useI18n } from 'vue-i18n';

const username = ref('');
const email = ref('');
const password = ref('');
const agreeToTerms = ref(false);
const showPassword = ref(false);
const router = useRouter();

const rules = {
  required: (value: string | null | undefined) => !!value || t('validation.required'),
  email: (value: string) => /.+@.+\..+/.test(value) || t('validation.email'),
};

const { t } = useI18n();

const notificationStore = useNotificationStore();

const handleRegister = async () => {
  const authStore = useAuthStore();
  await authStore.register({
    name: username.value,
    email: email.value,
    password: password.value,
    // Assuming agreeToTerms is handled by the backend or not needed for basic registration
  });

  if (authStore.isAuthenticated) {
    notificationStore.showSnackbar(t('register.success'), 'success');
    router.push('/dashboard');
  } else {
    notificationStore.showSnackbar(authStore.error || t('register.fillAllFields'), 'error');
  }
};
</script>