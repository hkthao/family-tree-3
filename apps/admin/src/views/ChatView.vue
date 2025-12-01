<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <v-card>
          <v-card-title>{{ t('chat.title') }}</v-card-title>
          <v-card-text>
            <N8nChatWidget />
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { N8nChatWidget } from '@/components/ai'; // Import the N8nChatWidget

const { t } = useI18n();
</script>