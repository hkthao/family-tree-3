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
import { ref, onMounted } from 'vue';
import { VListSubheader } from 'vuetify/components';
import { useI18n } from 'vue-i18n';
import { useTheme } from 'vuetify';
import { useNotificationStore } from '@/stores/notification.store';

const { t } = useI18n();
const theme = useTheme();
const notificationStore = useNotificationStore();

const preferencesForm = ref({
  theme: 'light',
  notifications: {
    email: true,
    sms: false,
    inApp: true,
  },
});

onMounted(() => {
  preferencesForm.value.theme = theme.global.name.value;
});

const savePreferences = () => {
  theme.global.name.value = preferencesForm.value.theme;
  console.log('Saving preferences:', preferencesForm.value);
  notificationStore.showSnackbar(t('userSettings.preferences.saveSuccess'), 'success');
};
</script>
