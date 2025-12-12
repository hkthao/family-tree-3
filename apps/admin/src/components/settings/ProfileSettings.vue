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
        <AvatarInput v-model="formData.avatarBase64" :size="128" :initial-avatar="initialAvatarDisplay" />
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
        <v-btn color="primary" type="submit" :loading="isSavingProfile || isFetchingProfile">{{ t('common.save') }}</v-btn>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { AvatarInput } from '@/components/common';
import { useProfileSettings } from '@/composables/user/useProfileSettings'; // Import the new composable

const { t } = useI18n();

const {
  formData,
  v$,
  initialAvatarDisplay,
  isFetchingProfile,
  isSavingProfile,
  saveProfile,
} = useProfileSettings();

</script>