<template>
  <v-form ref="formRef" @submit.prevent="saveProfile">
    <v-alert type="info" variant="tonal" class="mb-4">
      {{ $t('donate.profileBanner') }}
      <router-link to="/donate" class="text-decoration-none text-primary ml-2">
        {{ $t('donate.learnMore') }}
      </router-link>
    </v-alert>
    <v-row>
      <v-col cols="12">
        <AvatarInput v-model="formData.avatar" :size="128" />
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.firstName" :label="t('userSettings.profile.firstName')"
          @blur="v$.firstName.$touch()" @input="v$.firstName.$touch()"
          :error-messages="v$.firstName.$errors.map(e => e.$message as string)"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.lastName" :label="t('userSettings.profile.lastName')"
          @blur="v$.lastName.$touch()" @input="v$.lastName.$touch()"
          :error-messages="v$.lastName.$errors.map(e => e.$message as string)"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="formData.email" :label="t('userSettings.profile.email')" @blur="v$.email.$touch()"
          @input="v$.email.$touch()" :disabled="true"
          :error-messages="v$.email.$errors.map(e => e.$message as string)" />
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
        <v-btn color="primary" type="submit" :loading="loading">{{ t('common.save') }}</v-btn>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { onMounted, computed, reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { useNotificationStore } from '@/stores/notification.store';
import { AvatarInput } from '@/components/common';
import { useUserProfileStore } from '@/stores';
import type { UserProfile } from '@/types';
import { useVuelidate } from '@vuelidate/core';
import { useProfileSettingsRules } from '@/validations/profile-settings.validation';

const { t } = useI18n();
const notificationStore = useNotificationStore();
const userProfileStore = useUserProfileStore();

const formRef = ref<HTMLFormElement | null>(null);
const loading = ref(false);

const formData = reactive({
  name: '',
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  avatar: null as string | null,
  externalId: '',
});

const rules = useProfileSettingsRules();

const v$ = useVuelidate(rules, formData);

const generatedFullName = computed(() => {
  return `${formData.firstName} ${formData.lastName}`.trim();
});

onMounted(async () => {
  await userProfileStore.fetchCurrentUserProfile();
  if (userProfileStore.userProfile) {
    formData.name = userProfileStore.userProfile.name;
    formData.firstName = userProfileStore.userProfile.firstName || '';
    formData.lastName = userProfileStore.userProfile.lastName || '';
    formData.email = userProfileStore.userProfile.email;
    formData.phone = userProfileStore.userProfile.phone || '';
    formData.avatar = userProfileStore.userProfile.avatar || null;
    formData.externalId = userProfileStore.userProfile.externalId;
  } else if (userProfileStore.error) {
    notificationStore.showSnackbar(userProfileStore.error, 'error');
  }
});

const saveProfile = async () => {
  loading.value = true;
  try {
    const result = await v$.value.$validate();
  

    if (result && userProfileStore.userProfile) {
      const updatedProfile: UserProfile = {
        id: userProfileStore.userProfile.id,
        externalId: userProfileStore.userProfile.externalId,
        email: formData.email,
        name: generatedFullName.value,
        firstName: formData.firstName,
        lastName: formData.lastName,
        phone: formData.phone,
        avatar: formData.avatar === null ? undefined : formData.avatar,
      };

      const success = await userProfileStore.updateUserProfile(updatedProfile);
      if (success) {
        notificationStore.showSnackbar(
          t('userSettings.profile.saveSuccess'),
          'success',
        );
      } else {
        notificationStore.showSnackbar(
          userProfileStore.error || t('userSettings.profile.saveError'),
          'error',
        );
      }
    } else if (!result) {
      notificationStore.showSnackbar(
        t('userSettings.profile.validationError'),
        'error',
      );
    }
  } finally {
    loading.value = false;
  }
};
</script>