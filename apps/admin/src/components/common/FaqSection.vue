<template>
  <v-card flat>
    <v-card-title class="text-h6 text-secondary">{{ $t(titleKey) }}</v-card-title>
    <v-card-text>
      <v-expansion-panels variant="accordion">
        <v-expansion-panel
          v-for="(item, index) in faqItems"
          :key="index"
          :title="$t(item.titleKey)"
        >
          <v-expansion-panel-text>
            {{ $t(item.textKey) }}
          </v-expansion-panel-text>
        </v-expansion-panel>
      </v-expansion-panels>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

interface FaqItem {
  titleKey: string;
  textKey: string;
}

defineProps<{
  titleKey: string;
  faqItems: FaqItem[];
}>();

const { t } = useI18n();
</script>

<style scoped>
/* Add any specific styles for the FAQ section here */
</style>