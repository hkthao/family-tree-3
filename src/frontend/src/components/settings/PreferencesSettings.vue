<template>
  <v-form @submit.prevent="savePreferences">
    <v-row>
      <v-col cols="12">
        <VListSubheader>{{
          t('userSettings.preferences.theme')
          }}</VListSubheader>
        <v-radio-group v-model="preferencesForm.theme" inline >
          <v-radio v-for="option in themeOptions" :key="option.value" :label="option.text"
            :value="option.value"></v-radio>
        </v-radio-group>
      </v-col>
    </v-row>

    <v-row>
      <v-col cols="12">
        <VListSubheader>{{
          t('userSettings.preferences.language')
          }}</VListSubheader>
        <v-select v-model="preferencesForm.language" :items="languageOptions" 
          :label="t('userSettings.preferences.language')" item-title="text" item-value="value"></v-select>
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
import { ref, computed, onMounted } from 'vue';
import { VListSubheader } from 'vuetify/components';
import { useI18n } from 'vue-i18n';
import { useUserSettingsStore } from '@/stores';
import { getThemeOptions } from '@/constants/theme.constants';
import { getLanguageOptions } from '@/constants/language.constants';
import { useNotificationStore } from '@/stores/notification.store';

const { t } = useI18n();
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

const savePreferences = async () => {
  // Update store state
  userSettingsStore.preferences.emailNotificationsEnabled =
    preferencesForm.value.emailNotificationsEnabled;
  userSettingsStore.preferences.smsNotificationsEnabled =
    preferencesForm.value.smsNotificationsEnabled;
  userSettingsStore.preferences.inAppNotificationsEnabled =
    preferencesForm.value.inAppNotificationsEnabled;
  userSettingsStore.setLanguage(preferencesForm.value.language);
  userSettingsStore.setTheme(preferencesForm.value.theme);
  const success = await userSettingsStore.saveUserSettings();
  if (success) {
    notificationStore.showSnackbar(t('userSettings.preferences.saveSuccess'), 'success');
  } else {
    notificationStore.showSnackbar(userSettingsStore.error || t('userSettings.preferences.saveError'), 'error');
  }
};

onMounted(async () => {
  await userSettingsStore.fetchUserSettings();
  preferencesForm.value = {
    theme: userSettingsStore.preferences.theme,
    emailNotificationsEnabled: userSettingsStore.preferences.emailNotificationsEnabled,
    smsNotificationsEnabled: userSettingsStore.preferences.smsNotificationsEnabled,
    inAppNotificationsEnabled: userSettingsStore.preferences.inAppNotificationsEnabled,
    language: userSettingsStore.preferences.language,
  };
});
</script>
