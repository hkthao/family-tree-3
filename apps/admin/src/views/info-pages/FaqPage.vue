<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <h1 class="text-h4 text-primary mb-4">{{ $t('faq.title') }}</h1>
        <p class="text-body-1">
          {{ $t('faq.description') }}
        </p>
      </v-col>
    </v-row>

    <v-row class="mt-3">
      <v-col cols="12">
        <FaqSection
          titleKey="faq.gettingStarted.title"
          :faq-items="gettingStartedFaqItems"
        />
      </v-col>
    </v-row>

    <v-row class="mt-3">
      <v-col cols="12">
        <FaqSection
          titleKey="faq.generalQuestions.title"
          :faq-items="generalQuestionsFaqItems"
        />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { FaqSection } from '@/components/common'; // Import FaqSection

const { t } = useI18n();

const gettingStartedFaqItems = [
  { titleKey: 'faq.gettingStarted.addMembers.title', textKey: 'faq.gettingStarted.addMembers.subtitle' },
  { titleKey: 'faq.gettingStarted.editRelationships.title', textKey: 'faq.gettingStarted.editRelationships.subtitle' },
  { titleKey: 'faq.gettingStarted.searchTree.title', textKey: 'faq.gettingStarted.searchTree.subtitle' },
];

const generalQuestionsFaqItems = [
  { titleKey: 'faq.generalQuestions.createTree.title', textKey: 'faq.generalQuestions.createTree.text' },
  { titleKey: 'faq.generalQuestions.importData.title', textKey: 'faq.generalQuestions.importData.text' },
  { titleKey: 'faq.generalQuestions.shareTree.title', textKey: 'faq.generalQuestions.shareTree.text' },
];
</script>

<style scoped>
/* Add any specific styles for the FAQ page here */
</style>
