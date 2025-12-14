<template>
  <v-form @submit.prevent="savePreferences">
    <v-row>
      <v-col cols="12">
        <VListSubheader>{{
          t('userSettings.preferences.theme')
        }}</VListSubheader>
        <v-radio-group v-model="preferencesForm.theme" inline>
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
        <v-btn color="primary" type="submit" :loading="isSaving">{{ t('common.save') }}</v-btn>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'; // Add watch here
import { VListSubheader } from 'vuetify/components';
import { useI18n } from 'vue-i18n';
import { getThemeOptions } from '@/constants/theme.constants';
import { getLanguageOptions } from '@/constants/language.constants';
import { useGlobalSnackbar } from '@/composables';
import { useUserPreferences } from '@/composables/data/useUserPreferences'; // Import useUserPreferences
import { type UserPreference, Theme, Language } from '@/types'; // Import UserPreference, Theme, Language

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();
const { preferences, savePreferences: mutateSavePreferences, isSaving, saveError } = useUserPreferences();

const preferencesForm = ref<UserPreference>({
  ...(preferences.value || {} as UserPreference), // Spread existing preferences or an empty object cast to UserPreference
  theme: preferences.value?.theme || Theme.Dark, // Default to Dark enum
  language: preferences.value?.language || Language.English, // Default to English enum
});

// Watch for preferences to be loaded and update the form
watch(preferences, (newPreferences) => {
  if (newPreferences) {
    preferencesForm.value = { ...newPreferences };
  }
}, { immediate: true });


const languageOptions = computed(() => getLanguageOptions(t));
const themeOptions = computed(() => getThemeOptions(t));

const savePreferences = async () => {
  await mutateSavePreferences(preferencesForm.value);
  if (!isSaving.value && !saveError.value) { // Check if saving is complete and no error
    showSnackbar(t('userSettings.preferences.saveSuccess'), 'success');
  } else if (saveError.value) {
    showSnackbar(saveError.value.message || t('userSettings.preferences.saveError'), 'error');
  }
};
</script>
