<template>
  <v-form ref="profileFormRef" @submit.prevent="saveProfile">
    <v-row>
      <v-col cols="12">
        <AvatarInput v-model="profileForm.avatar" :size="128" />
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="profileForm.firstName" :label="t('userSettings.profile.firstName')"
          :rules="[rules.required]"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="profileForm.lastName" :label="t('userSettings.profile.lastName')"
          :rules="[rules.required]"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="profileForm.email" :label="t('userSettings.profile.email')"
          :rules="[rules.required, rules.email]" />
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="profileForm.phone" :label="t('userSettings.profile.phone')"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model="profileForm.externalId" :label="t('userSettings.profile.externalId')"
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
import { ref, onMounted, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useNotificationStore } from '@/stores/notification.store';
import { AvatarInput } from '@/components/common';
import { useUserProfileStore } from '@/stores';
import type { UserProfile } from '@/types';

const { t } = useI18n();
const notificationStore = useNotificationStore();
const userProfileStore = useUserProfileStore();

const profileForm = ref({
  name: '',
  firstName: '',
  lastName: '',
  email: '',
  phone: '',
  avatar: null as string | null,
  externalId: '',
});

const profileFormRef = ref<HTMLFormElement | null>(null);

const rules = {
  required: (value: string) => !!value || t('validation.required'),
  email: (value: string) => /.+@.+\..+/.test(value) || t('validation.email'),
};

const generatedFullName = computed(() => {
  return `${profileForm.value.firstName} ${profileForm.value.lastName}`.trim();
});

onMounted(async () => {
  await userProfileStore.fetchCurrentUserProfile();
  if (userProfileStore.userProfile) {
    profileForm.value.name = userProfileStore.userProfile.name;
    profileForm.value.firstName = userProfileStore.userProfile.firstName || '';
    profileForm.value.lastName = userProfileStore.userProfile.lastName || '';
    profileForm.value.email = userProfileStore.userProfile.email;
    profileForm.value.phone = userProfileStore.userProfile.phone || '';
    profileForm.value.avatar = userProfileStore.userProfile.avatar || null;
    profileForm.value.externalId = userProfileStore.userProfile.externalId;
  } else if (userProfileStore.error) {
    notificationStore.showSnackbar(userProfileStore.error, 'error');
  }
});

const saveProfile = async () => {
  if (profileFormRef.value) {
    const { valid } = await profileFormRef.value.validate();
    if (valid && userProfileStore.userProfile) {
      const updatedProfile: UserProfile = {
        id: userProfileStore.userProfile.id,
        externalId: userProfileStore.userProfile.externalId,
        email: profileForm.value.email,
        name: generatedFullName.value,
        firstName: profileForm.value.firstName,
        lastName: profileForm.value.lastName,
        phone: profileForm.value.phone,
        avatar: profileForm.value.avatar === null ? undefined : profileForm.value.avatar,
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
    } else if (!valid) {
      notificationStore.showSnackbar(
        t('userSettings.profile.validationError'),
        'error',
      );
    }
  }
};
</script>
