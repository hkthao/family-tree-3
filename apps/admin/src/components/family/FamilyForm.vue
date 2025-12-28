<template>
  <v-form ref="formRef" :disabled="props.readOnly">
    <AvatarInput v-if="!props.readOnly" v-model="formData.avatarBase64" :size="96"
      :initial-avatar="initialAvatarDisplay" />
    <div v-else class="d-flex justify-center mb-4">
      <AvatarDisplay :src="getFamilyAvatarUrl(formData.avatarUrl)" :size="96" />
    </div>
    <v-row>
      <v-col cols="6">
        <v-text-field v-model="formData.name" :label="$t('family.form.name')" :rules="rules.name" required
          data-testid="family-name-input"></v-text-field>
      </v-col>
      <v-col cols="6">
        <v-select v-model="formData.visibility" :items="visibilityItems" :label="$t('family.form.visibilityLabel')"
          required data-testid="family-visibility-select"></v-select>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-text-field v-model="formData.address" :label="$t('family.form.address')"
          data-testid="family-address-input"></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-textarea v-model="formData.description" :rows="2" :auto-grow="true"
          :label="$t('family.form.description')" data-testid="family-description-input"></v-textarea>
      </v-col>
    </v-row>
    <v-row v-if="props.displayLimitConfig">
      <v-col cols="6">
        <v-text-field
          :label="$t('family.form.maxMembers')"
          :model-value="formData.familyLimitConfiguration?.maxMembers"
          readonly
          data-testid="family-max-members"
        ></v-text-field>
      </v-col>
      <v-col cols="6">
        <v-text-field
          :label="$t('family.form.maxStorageMb')"
          :model-value="formData.familyLimitConfiguration?.maxStorageMb ? formData.familyLimitConfiguration.maxStorageMb + ' MB' : ''"
          readonly
          data-testid="family-max-storage-mb"
        ></v-text-field>
      </v-col>
      <v-col cols="6">
        <v-text-field
          :label="$t('family.form.aiChatMonthlyLimit')"
          :model-value="formData.familyLimitConfiguration?.aiChatMonthlyLimit ?? ''"
          readonly
          data-testid="family-ai-chat-monthly-limit"
        ></v-text-field>
      </v-col>
      <v-col cols="6">
        <v-text-field
          :label="$t('family.form.aiChatMonthlyUsage')"
          :model-value="formData.familyLimitConfiguration?.aiChatMonthlyUsage ?? ''"
          readonly
          data-testid="family-ai-chat-monthly-usage"
        ></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <UserAutocomplete v-model="managers" multiple :disabled="props.readOnly" hideDetails
          :label="$t('family.permissions.managers')" data-testid="family-managers-select" :loading="isLoadingUsers"></UserAutocomplete>
      </v-col>
      <v-col cols="12">
        <UserAutocomplete v-model="viewers" multiple :disabled="props.readOnly" hideDetails
          :label="$t('family.permissions.viewers')" data-testid="family-viewers-select" :loading="isLoadingUsers"></UserAutocomplete>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import type { VForm } from 'vuetify/components';
import type { FamilyDto } from '@/types';
import { AvatarInput, AvatarDisplay } from '@/components/common';
import UserAutocomplete from '@/components/common/UserAutocomplete.vue';
import { useFamilyForm } from '@/composables';

const props = defineProps<{
  data?: FamilyDto;
  readOnly?: boolean;
  displayLimitConfig?: boolean;
}>();
// Removed emit from props, as it's handled internally now.
defineEmits(['submit']);

const formRef = ref<VForm | null>(null);

const {
  state: { formData, initialAvatarDisplay, managers, viewers, visibilityItems, getFamilyAvatarUrl, rules, isLoadingUsers },
  actions: { validate, getFormData },
} = useFamilyForm(props, formRef);

defineExpose({
  validate,
  getFormData,
});


</script>
