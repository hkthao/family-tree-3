<template>
  <v-container fluid>
    <v-card>
      <v-card-title class="text-h5">{{ t('userSettings.title') }}</v-card-title>
              <v-card-text>
                <v-row>
                  <v-col cols="12" md="3">
                    <v-tabs v-model="tab" color="primary" direction="vertical">
                      <v-tab value="profile">{{ t('userSettings.tab.profile') }}</v-tab>
                      <v-tab value="preferences">{{ t('userSettings.tab.preferences') }}</v-tab>
                      <v-tab value="security">{{ t('userSettings.tab.security') }}</v-tab>
                    </v-tabs>
                  </v-col>
                  <v-col cols="12" md="9">
                    <v-window v-model="tab">
                      <v-window-item value="profile">
                        <ProfileSettings />
                      </v-window-item>
                      <v-window-item value="preferences">
                        <PreferencesSettings />
                      </v-window-item>
                      <v-window-item value="security">
                        <SecuritySettings />
                      </v-window-item>
                    </v-window>
                  </v-col>
                </v-row>
              </v-card-text>    </v-card>
  </v-container>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { ProfileSettings, PreferencesSettings, SecuritySettings } from '@/components/settings';

const { t } = useI18n();
const tab = ref('profile');
</script>
