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
        <v-card flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('faq.gettingStarted.title') }}</v-card-title>
          <v-card-text>
            <v-expansion-panels variant="accordion">
              <v-expansion-panel :title="$t('faq.gettingStarted.addMembers.title')">
                <v-expansion-panel-text>
                  {{ $t('faq.gettingStarted.addMembers.subtitle') }}
                </v-expansion-panel-text>
              </v-expansion-panel>
              <v-expansion-panel :title="$t('faq.gettingStarted.editRelationships.title')">
                <v-expansion-panel-text>
                  {{ $t('faq.gettingStarted.editRelationships.subtitle') }}
                </v-expansion-panel-text>
              </v-expansion-panel>
              <v-expansion-panel :title="$t('faq.gettingStarted.searchTree.title')">
                <v-expansion-panel-text>
                  {{ $t('faq.gettingStarted.searchTree.subtitle') }}
                </v-expansion-panel-text>
              </v-expansion-panel>
            </v-expansion-panels>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row class="mt-3">
      <v-col cols="12">
        <v-card flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('faq.generalQuestions.title') }}</v-card-title>
          <v-card-text>
            <v-expansion-panels variant="accordion">
              <v-expansion-panel :title="$t('faq.generalQuestions.createTree.title')">
                <v-expansion-panel-text>
                  {{ $t('faq.generalQuestions.createTree.text') }}
                </v-expansion-panel-text>
              </v-expansion-panel>
              <v-expansion-panel :title="$t('faq.generalQuestions.importData.title')">
                <v-expansion-panel-text>
                  {{ $t('faq.generalQuestions.importData.text') }}
                </v-expansion-panel-text>
              </v-expansion-panel>
              <v-expansion-panel :title="$t('faq.generalQuestions.shareTree.title')">
                <v-expansion-panel-text>
                  {{ $t('faq.generalQuestions.shareTree.text') }}
                </v-expansion-panel-text>
              </v-expansion-panel>
            </v-expansion-panels>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
</script>

<style scoped>
/* Add any specific styles for the FAQ page here */
</style>
