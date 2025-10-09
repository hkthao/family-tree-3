<template>
  <v-form @submit.prevent="savePreferences">
    <v-row>
      <v-col cols="12">
        <VListSubheader>{{
          t('userSettings.preferences.theme')
          }}</VListSubheader>
        <v-radio-group v-model="preferencesForm.theme" inline hide-details>
          <v-radio
            v-for="option in themeOptions"
            :key="option.value"
            :label="option.text"
            :value="option.value"
          ></v-radio>
        </v-radio-group>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <VListSubheader>{{
          t('userSettings.preferences.language')
          }}</VListSubheader>
        <v-select v-model="preferencesForm.language" :items="languageOptions"
          :label="t('userSettings.preferences.language')" item-title="text" item-value="value" hide-details></v-select>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <VListSubheader>{{
          t('userSettings.preferences.notifications')
          }}</VListSubheader>
        <v-checkbox
          v-model="preferencesForm.emailNotificationsEnabled"
          :label="t('userSettings.preferences.notificationsEmail')"
          hide-details
        ></v-checkbox>
        <v-checkbox
          v-model="preferencesForm.smsNotificationsEnabled"
          :label="t('userSettings.preferences.notificationsSMS')"
          hide-details
        ></v-checkbox>
        <v-checkbox
          v-model="preferencesForm.inAppNotificationsEnabled"
          :label="t('userSettings.preferences.notificationsInApp')"
          hide-details
        ></v-checkbox>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" class="text-right">
        <v-btn color="primary" type="submit">{{ t('common.save') }}</v-btn>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import { VListSubheader } from 'vuetify/components';
import { useI18n } from 'vue-i18n';
import { useTheme } from 'vuetify';
import { useUserSettingsStore } from '@/stores/userSettings.store';
import { getThemeOptions } from '@/constants/theme.constants';
import { getLanguageOptions } from '@/constants/language.constants';
import { useNotificationStore } from '@/stores/notification.store';

const { t } = useI18n();
const theme = useTheme();
const userSettingsStore = useUserSettingsStore();
const notificationStore = useNotificationStore();

const preferencesForm = ref({
  theme: userSettingsStore.preferences.theme,
  emailNotificationsEnabled: userSettingsStore.preferences.emailNotificationsEnabled,
  smsNotificationsEnabled: userSettingsStore.preferences.smsNotificationsEnabled,
  inAppNotificationsEnabled: userSettingsStore.preferences.inAppNotificationsEnabled,
  language: userSettingsStore.preferences.language,
});

const languageOptions = computed(() => getLanguageOptions(t));
const themeOptions = computed(() => getThemeOptions(t));

watch(
  () => userSettingsStore.preferences.theme,
  (newTheme) => {
    const option = themeOptions.value.find((option) => option.value === newTheme);
    if (option) {
         theme.change(option.code)
    }
  },
  { immediate: true },
); // Immediate to set theme on initial load

const savePreferences = async () => {
  // Update store state
  userSettingsStore.setTheme(preferencesForm.value.theme);
  userSettingsStore.preferences.emailNotificationsEnabled =
    preferencesForm.value.emailNotificationsEnabled;
  userSettingsStore.preferences.smsNotificationsEnabled =
    preferencesForm.value.smsNotificationsEnabled;
  userSettingsStore.preferences.inAppNotificationsEnabled =
    preferencesForm.value.inAppNotificationsEnabled;
  userSettingsStore.setLanguage(preferencesForm.value.language);

  // Save settings via store action
  try {
    await userSettingsStore.saveUserSettings();
    notificationStore.showSnackbar(t('userSettings.preferences.saveSuccess'), 'success');
  } catch (error) {
    notificationStore.showSnackbar(t('userSettings.preferences.saveError'), 'error');
  }
};
</script>
