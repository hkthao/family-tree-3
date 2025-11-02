<template>
  <v-form ref="profileFormRef" @submit.prevent="saveProfile">
    <v-row>
      <v-col cols="12">
        <AvatarInput v-model="form.avatar" :size="128" />
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="form.firstName" :label="t('userSettings.profile.firstName')"
          @blur="v$.firstName.$touch()" @input="v$.firstName.$touch()"
          :error-messages="v$.firstName.$errors.map(e => e.$message as string)"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="form.lastName" :label="t('userSettings.profile.lastName')"
          @blur="v$.lastName.$touch()" @input="v$.lastName.$touch()"
          :error-messages="v$.lastName.$errors.map(e => e.$message as string)"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="form.email" :label="t('userSettings.profile.email')"
          @blur="v$.email.$touch()" @input="v$.email.$touch()"
          :error-messages="v$.email.$errors.map(e => e.$message as string)" />
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="form.phone" :label="t('userSettings.profile.phone')"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="form.externalId" :label="t('userSettings.profile.externalId')"
          readonly></v-text-field>
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
import { onMounted, computed, reactive } from 'vue';
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

const form = reactive({
  name: '',
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  avatar: null as string | null,
  externalId: '',
});

const state = reactive({
  firstName: form.firstName,
  lastName: form.lastName,
  email: form.email,
});

const rules = useProfileSettingsRules();

const v$ = useVuelidate(rules, state);

const generatedFullName = computed(() => {
  return `${form.firstName} ${form.lastName}`.trim();
});

onMounted(async () => {
  await userProfileStore.fetchCurrentUserProfile();
  if (userProfileStore.userProfile) {
    form.name = userProfileStore.userProfile.name;
    form.firstName = userProfileStore.userProfile.firstName || '';
    form.lastName = userProfileStore.userProfile.lastName || '';
    form.email = userProfileStore.userProfile.email;
    form.phone = userProfileStore.userProfile.phone || '';
    form.avatar = userProfileStore.userProfile.avatar || null;
    form.externalId = userProfileStore.userProfile.externalId;
  } else if (userProfileStore.error) {
    notificationStore.showSnackbar(userProfileStore.error, 'error');
  }
});

const saveProfile = async () => {
  const result = await v$.value.$validate();
  if (result && userProfileStore.userProfile) {
    const updatedProfile: UserProfile = {
      id: userProfileStore.userProfile.id,
      externalId: userProfileStore.userProfile.externalId,
      email: form.email,
      name: generatedFullName.value,
      firstName: form.firstName,
      lastName: form.lastName,
      phone: form.phone,
      avatar: form.avatar === null ? undefined : form.avatar,
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
};
</script>
