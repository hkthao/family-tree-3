<template>
  <v-container fill-height fluid>
    <v-row align="center" justify="center">
      <v-col cols="12" sm="8" md="6" lg="4">
        <v-card class="pa-5 text-center">
          <v-card-title class="headline justify-center">
            {{ t('logout.title') }}
          </v-card-title>
          <v-card-text>
            <v-progress-circular indeterminate color="primary" size="64" class="mb-4"></v-progress-circular>
            <p>{{ t('logout.message') }}</p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAuthStore } from '@/stores/auth.store';
import { useRouter } from 'vue-router';

const { t } = useI18n();
const authStore = useAuthStore();
const router = useRouter();

onMounted(async () => {
  // Perform frontend cleanup
  authStore.user = null;
  authStore.token = null;
  localStorage.clear(); // Clear any local storage items

  // Redirect to the Auth0 logout endpoint
  // Auth0 will then redirect back to window.location.origin
  await authStore.logout();
});
</script>

<style scoped>
/* Add any specific styles for LogoutView here if needed */
</style>
