<template>
  <v-card>
    <v-card-title>{{ t('chat.title') }}</v-card-title>
    <v-card-text>
      <N8nChatWidget />
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { N8nChatWidget } from '@/components/ai'; 

const { t } = useI18n();
</script>
