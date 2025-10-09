<template>
  <v-form ref="profileForm" @submit.prevent="saveProfile">
    <v-row>
      <v-col cols="12">
        <AvatarInput v-model="profileForm.avatar" :size="128" />
      </v-col>
      <v-col cols="12">
        <v-text-field
          v-model="profileForm.fullName"
          :label="t('userSettings.profile.fullName')"
          :rules="[rules.required]"
          class="mb-2"
        ></v-text-field>
        <v-text-field
          v-model="profileForm.email"
          :label="t('userSettings.profile.email')"
          :rules="[rules.required, rules.email]"
          class="mb-2"
        ></v-text-field>
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
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useAuthStore } from '@/stores/auth.store';
import { useNotificationStore } from '@/stores/notification.store';
import { AvatarInput } from '@/components/common';
import { useUserProfileStore } from '@/stores/userProfile.store';
import type { UserProfile } from '@/types';

const { t } = useI18n();
const authStore = useAuthStore();
const notificationStore = useNotificationStore();
const userProfileStore = useUserProfileStore();

const profileForm = ref({
  fullName: '',
  email: '',
  avatar: null as string | null,
});

const profileFormRef = ref<HTMLFormElement | null>(null);

const rules = {
  required: (value: string) => !!value || t('validation.required'),
  email: (value: string) => /.+@.+\..+/.test(value) || t('validation.email'),
};

onMounted(async () => {
  if (authStore.user) {
    await userProfileStore.fetchUserProfile(authStore.user.id);
    if (userProfileStore.userProfile) {
      profileForm.value.fullName = userProfileStore.userProfile.name;
      profileForm.value.email = userProfileStore.userProfile.email;
      profileForm.value.avatar = userProfileStore.userProfile.avatar || null;
    } else if (userProfileStore.error) {
      notificationStore.showSnackbar(userProfileStore.error, 'error');
    }
  }
});

const saveProfile = async () => {
  if (profileFormRef.value) {
    const { valid } = await profileFormRef.value.validate();
    if (valid && authStore.user) {
      const updatedProfile: UserProfile = {
        id: authStore.user.id,
        externalId: authStore.user.externalId,
        email: profileForm.value.email,
        name: profileForm.value.fullName,
        avatar: profileForm.value.avatar === null ? undefined : profileForm.value.avatar,
      };

      const success = await userProfileStore.updateUserProfile(updatedProfile);

      if (success && userProfileStore.userProfile) {
        authStore.user = userProfileStore.userProfile; // Update authStore user with new data
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
    } else if (!valid) {
      notificationStore.showSnackbar(
        t('userSettings.profile.validationError'),
        'error',
      );
    }
  }
};
</script>
