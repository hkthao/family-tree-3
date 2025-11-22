<template>
  <v-container fluid>
    <v-row>
      <v-col cols="12">
        <p class="text-body-1">
          {{ $t('faq.description') }}
        </p>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <FaqSection titleKey="faq.gettingStarted.title" :faq-items="gettingStartedFaqItems" />
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <FaqSection titleKey="faq.generalQuestions.title" :faq-items="generalQuestionsFaqItems" />
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { FaqSection } from '@/components/common'; // Import FaqSection

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
