<template>
  <v-app>
    <v-main class="d-flex align-center justify-center">
      <v-container fluid>
        <v-row justify="center">
          <v-col cols="12" sm="8" md="6" lg="4">
            <v-card class="elevation-12">
              <v-card-title class="text-center text-h5 py-4">
                <span class="font-weight-bold">FamilyTree</span>
              </v-card-title>
              <v-card-subtitle class="text-center">
                {{ t('register.title') }} 🚀
              </v-card-subtitle>
              <v-card-text class="px-6 py-8">
                <p class="text-center mb-4">{{ t('register.subtitle') }}</p>
                <RegisterForm @register-success="onRegisterSuccess" @register-fail="onRegisterFail" />

                <v-divider class="my-4">{{ t('common.or') }}</v-divider>

                <SocialLogin />
              </v-card-text>
              <v-card-actions class="justify-center pb-4">
                {{ t('register.alreadyHaveAccount') }}
                <router-link to="/login" class="text-primary ml-1">{{ t('register.signInInstead') }}</router-link>
              </v-card-actions>
            </v-card>
          </v-col>
        </v-row>
      </v-container>
    </v-main>
    <v-snackbar v-model="notificationStore.snackbar.show" :color="notificationStore.snackbar.color" timeout="3000">
      {{ notificationStore.snackbar.message }}
    </v-snackbar>
  </v-app>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { RegisterForm, SocialLogin } from '@/components/auth';
import { useNotificationStore } from '@/stores/notification.store';
import { useRouter } from 'vue-router';

const { t } = useI18n();
const notificationStore = useNotificationStore();
const router = useRouter();

const onRegisterSuccess = () => {
  notificationStore.showSnackbar(t('register.success'), 'success');
  router.push('/login'); // Redirect to login after successful registration
};

const onRegisterFail = (message: string) => {
  notificationStore.showSnackbar(message, 'error');
};
</script>