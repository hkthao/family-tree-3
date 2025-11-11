<template>
  <v-container>
    <v-row>
      <v-col cols="12">
        <h1 class="text-h4 text-primary mb-4">{{ $t('help.title') }}</h1>
        <p class="text-body-1">
          {{ $t('help.description') }}
        </p>
      </v-col>
    </v-row>

    <v-row class="mt-3">
      <v-col cols="12">
        <v-card flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('help.gettingStarted.title') }}</v-card-title>
          <v-card-text>
            <v-list density="compact">
              <v-list-item prepend-icon="mdi-account-plus-outline">
                <v-list-item-title>{{ $t('help.gettingStarted.addMembers.title') }}</v-list-item-title>
                <v-list-item-subtitle>
                  {{ $t('help.gettingStarted.addMembers.subtitle') }}
                </v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-link-variant">
                <v-list-item-title>{{ $t('help.gettingStarted.editRelationships.title') }}</v-list-item-title>
                <v-list-item-subtitle>
                  {{ $t('help.gettingStarted.editRelationships.subtitle') }}
                </v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-magnify">
                <v-list-item-title>{{ $t('help.gettingStarted.searchTree.title') }}</v-list-item-title>
                <v-list-item-subtitle>
                  {{ $t('help.gettingStarted.searchTree.subtitle') }}
                </v-list-item-subtitle>
              </v-list-item>
              <v-list-item prepend-icon="mdi-robot-outline">
                <v-list-item-title>{{ $t('help.gettingStarted.useAIAssistant.title') }}</v-list-item-title>
                <v-list-item-subtitle>
                  {{ $t('help.gettingStarted.useAIAssistant.subtitle') }}
                </v-list-item-subtitle>
              </v-list-item>
            </v-list>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row class="mt-3">
      <v-col cols="12">
        <v-card flat>
          <v-card-title class="text-h6 text-secondary">{{ $t('help.faq.title') }}</v-card-title>
          <v-card-text>
            <v-expansion-panels variant="accordion">
              <v-expansion-panel :title="$t('help.faq.createTree.title')">
                <v-expansion-panel-text>
                  {{ $t('help.faq.createTree.text') }}
                </v-expansion-panel-text>
              </v-expansion-panel>
              <v-expansion-panel :title="$t('help.faq.importData.title')">
                <v-expansion-panel-text>
                  {{ $t('help.faq.importData.text') }}
                </v-expansion-panel-text>
              </v-expansion-panel>
              <v-expansion-panel :title="$t('help.faq.dataPrivacy.title')">
                <v-expansion-panel-text>
                  {{ $t('help.faq.dataPrivacy.text') }}
                </v-expansion-panel-text>
              </v-expansion-panel>
              <v-expansion-panel :title="$t('help.faq.shareTree.title')">
                <v-expansion-panel-text>
                  {{ $t('help.faq.shareTree.text') }}
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
          <v-card-title class="text-h6 text-secondary">{{ $t('help.furtherAssistance.title') }}</v-card-title>
          <v-card-text>
            <p class="text-body-1">
              {{ $t('help.furtherAssistance.description') }}
              <a href="#" target="_blank" class="text-primary">{{ $t('help.furtherAssistance.documentationLink') }}</a> {{ $t('common.or') }}
              <a href="#" target="_blank" class="text-primary">{{ $t('help.furtherAssistance.contactSupport') }}</a>.
            </p>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>
  </v-container>
</template>

<script setup lang="ts">
// No specific script logic needed for this page yet.
</script>

<style scoped>
/* Add any specific styles for the Help page here */
</style>
