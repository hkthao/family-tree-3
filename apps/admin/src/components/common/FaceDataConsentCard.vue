<template>
  <v-card variant="outlined" class="mx-auto" max-width="500">
    <v-card-title>{{ t('faceDataConsent.title') }}</v-card-title>
    <v-card-text>
      <p v-html="t('faceDataConsent.content')"></p>
      <p class="mt-4">
        <a href="https://donghoviet.thaohk.com/privacy-policy" target="_blank" class="text-primary">
          {{ t('faceDataConsent.privacyPolicyLink') }}
        </a>
      </p>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="primary" @click="handleGrantConsent">{{ t('faceDataConsent.grantConsent') }}</v-btn>
      <v-btn color="error" @click="handleDenyConsent">{{ t('faceDataConsent.denyConsent') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

interface FaceDataConsentCardProps {
  handleGrantConsent: () => void;
  handleDenyConsent: () => void;
}

defineProps<FaceDataConsentCardProps>();
const { t } = useI18n();
</script>