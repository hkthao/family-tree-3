<template>
  <v-form ref="localFormRef" @submit.prevent="saveProfile">
    <!-- Loading State -->
    <div v-if="isFetchingProfile" class="text-center pa-4">
      <v-progress-circular indeterminate color="primary"></v-progress-circular>
      <p class="mt-2">{{ t('userSettings.profile.loadingProfile') }}</p>
    </div>

    <!-- Error State -->
    <v-alert v-else-if="fetchError" type="error" variant="tonal" class="mb-4">
      {{ fetchError.message || t('userSettings.profile.fetchError') }}
    </v-alert>

    <!-- Form Content (only visible when not loading and no error) -->
    <template v-else-if="userProfile">
      <v-alert type="info" variant="tonal" class="mb-4">
        {{ $t('donate.profileBanner') }}
        <router-link to="/donate" class="text-decoration-none text-primary ml-2">
          {{ $t('donate.learnMore') }}
        </router-link>
      </v-alert>
      <v-row>
        <v-col cols="12">
          <AvatarInput v-model="formData.avatarBase64" :size="128" :initial-avatar="initialAvatarDisplay" />
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model="formData.firstName" :label="t('userSettings.profile.firstName')"
            :rules="validationRules.firstName"></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model="formData.lastName" :label="t('userSettings.profile.lastName')"
            :rules="validationRules.lastName"></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model="formData.email" :label="t('userSettings.profile.email')" :disabled="true"
            :rules="validationRules.email" />
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model="formData.phone" :label="t('userSettings.profile.phone')"></v-text-field>
        </v-col>
        <v-col cols="12" md="6">
          <v-text-field v-model="formData.externalId" :label="t('userSettings.profile.externalId')"
            disabled></v-text-field>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" class="text-right">
          <v-btn color="primary" type="submit" :loading="isSavingProfile || isFetchingProfile">{{ t('common.save') }}</v-btn>
        </v-col>
      </v-row>
    </template>
  </v-form>
</template>

<script setup lang="ts">
import { useI18n } from 'vue-i18n';
import { AvatarInput } from '@/components/common';
import { useProfileSettings } from '@/composables';
import { ref, watch } from 'vue';
import type { VForm } from 'vuetify/components';

const { t } = useI18n();

const localFormRef = ref<VForm | null>(null);

const {
  state: { formData, formRef, initialAvatarDisplay, isFetchingProfile, isSavingProfile, userProfile, fetchError, validationRules },
  actions: { saveProfile },
} = useProfileSettings();

// Assign the local form ref to the composable's formRef
watch(localFormRef, (newValue) => {
  if (newValue) {
    formRef.value = newValue;
  }
});
</script>
