<template>
  <v-card flat>
    <v-card-text>
      <v-form @submit.prevent="savePreferences">
        <v-row>
          <v-col cols="12">
            <VListSubheader>{{ t('userSettings.preferences.theme') }}</VListSubheader>
            <v-radio-group v-model="preferencesForm.theme" inline>
              <v-radio :label="t('userSettings.preferences.themeLight')" value="light"></v-radio>
              <v-radio :label="t('userSettings.preferences.themeDark')" value="dark"></v-radio>
            </v-radio-group>
          </v-col>
        </v-row>

        <v-row>
          <v-col cols="12">
            <VListSubheader>{{ t('userSettings.preferences.language') }}</VListSubheader>
            <v-select
              v-model="preferencesForm.language"
              :items="languageOptions"
              :label="t('userSettings.preferences.language')"
              item-title="text"
              item-value="value"
              hide-details
            ></v-select>
          </v-col>
        </v-row>

        <v-row>
          <v-col cols="12">
            <VListSubheader>{{ t('userSettings.preferences.notifications') }}</VListSubheader>
            <v-checkbox
              v-model="preferencesForm.notifications.email"
              :label="t('userSettings.preferences.notificationsEmail')"
              hide-details
            ></v-checkbox>
            <v-checkbox
              v-model="preferencesForm.notifications.sms"
              :label="t('userSettings.preferences.notificationsSMS')"
              hide-details
            ></v-checkbox>
            <v-checkbox
              v-model="preferencesForm.notifications.inApp"
              :label="t('userSettings.preferences.notificationsInApp')"
              hide-details
            ></v-checkbox>
          </v-col>
        </v-row>

        <v-card-actions class="justify-end">
          <v-btn color="primary" type="submit">{{ t('common.save') }}</v-btn>
        </v-card-actions>
      </v-form>
    </v-card-text>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { VListSubheader } from 'vuetify/components';
import { useI18n } from 'vue-i18n';
import { useTheme } from 'vuetify';
import { useNotificationStore } from '@/stores/notification.store';
import { useUserSettingsStore } from '@/stores/userSettings.store';

const { t } = useI18n();
const theme = useTheme();
const notificationStore = useNotificationStore();
const userSettingsStore = useUserSettingsStore();

const preferencesForm = ref({
  theme: userSettingsStore.theme,
  notifications: {
    email: userSettingsStore.notifications.email,
    sms: userSettingsStore.notifications.sms,
    inApp: userSettingsStore.notifications.inApp,
  },
  language: userSettingsStore.language,
});

const languageOptions = computed(() => [
  { text: t('userSettings.preferences.languageEnglish'), value: 'en' },
  { text: t('userSettings.preferences.languageVietnamese'), value: 'vi' },
]);

onMounted(() => {
  // Ensure theme is synced with Vuetify's current theme
  userSettingsStore.setTheme(theme.global.name.value as 'light' | 'dark');
  preferencesForm.value.theme = userSettingsStore.theme;
  preferencesForm.value.language = userSettingsStore.language;
});

const savePreferences = async () => {
  // Update store state
  userSettingsStore.setTheme(preferencesForm.value.theme);
  userSettingsStore.notifications.email = preferencesForm.value.notifications.email;
  userSettingsStore.notifications.sms = preferencesForm.value.notifications.sms;
  userSettingsStore.notifications.inApp = preferencesForm.value.notifications.inApp;
  userSettingsStore.setLanguage(preferencesForm.value.language);

  // Save settings via store action
  try {
    await userSettingsStore.saveSettings();
    // Snackbar is handled by the store action
  } catch (error) {
    // Error snackbar is handled by the store action
  }
};
</script>
