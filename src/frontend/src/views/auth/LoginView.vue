<template>
  <v-app>
    <v-main class="d-flex align-center justify-center">
      <v-container fluid>
        <v-row justify="center">
          <v-col cols="12" sm="8" md="6" lg="4">
            <v-card class="elevation-12">
              <v-card-title class="text-center text-h5 py-4">
                <AppNameDisplay />
              </v-card-title>
              <v-card-subtitle class="text-center">
                {{ t('login.welcomeMessage') }}
              </v-card-subtitle>
              <v-card-text class="px-6 py-8">
                <LoginForm @login-success="onLoginSuccess" @login-fail="onLoginFail" />

                <v-divider class="my-4">{{ t('common.or') }}</v-divider>

                <SocialLogin />
              </v-card-text>
              <v-card-actions class="justify-center pb-4">
                {{ t('login.newOnPlatform') }}
                <router-link to="/register" class="text-primary ml-1">{{ t('login.createAccount') }}</router-link>
              </v-card-actions>
            </v-card>
          </v-col>
        </v-row>
      </v-container>
    </v-main>

  </v-app>
</template>

<script setup lang="ts">
import { LoginForm, SocialLogin } from '@/components/auth';
import { AppNameDisplay } from '@/components/common';
import { useNotificationStore } from '@/stores/notification.store';
import { useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();

const notificationStore = useNotificationStore();
const router = useRouter();

const onLoginSuccess = () => {
  notificationStore.showSnackbar(t('login.success'), 'success');
  router.push('/'); // Redirect to home or dashboard
};

const onLoginFail = (message: string) => {
  notificationStore.showSnackbar(message, 'error');
};
</script>