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

const { t } = useI18n();
const authStore = useAuthStore();
const notificationStore = useNotificationStore();

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

onMounted(() => {
  if (authStore.user) {
    profileForm.value.fullName = authStore.user.name;
    profileForm.value.email = authStore.user.email;
    profileForm.value.avatar = authStore.user.avatar || null;
  }
});

const saveProfile = async () => {
  if (profileFormRef.value) {
    const { valid } = await profileFormRef.value.validate();
    if (valid) {
      // Simulate API call
      console.log('Saving profile:', profileForm.value);
      notificationStore.showSnackbar(
        t('userSettings.profile.saveSuccess'),
        'success',
      );
      // In a real app, you would dispatch an action to update the user in authStore
    } else {
      notificationStore.showSnackbar(
        t('userSettings.profile.saveError'),
        'error',
      );
    }
  }
};
</script>
