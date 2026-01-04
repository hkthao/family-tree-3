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
          data-testid="family-name-input" prepend-inner-icon="mdi-home-group"></v-text-field>
      </v-col>
      <v-col cols="6">
        <v-select v-model="formData.visibility" :items="visibilityItems" :label="$t('family.form.visibilityLabel')"
          required data-testid="family-visibility-select" prepend-inner-icon="mdi-eye"></v-select>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <LocationInputField
          v-model="formData.address"
          :family-id="(formData as FamilyDto).id || undefined"
          :read-only="props.readOnly"
          prepend-inner-icon="mdi-map-marker"
        ></LocationInputField>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <v-textarea v-model="formData.description" :rows="2" :auto-grow="true"
          :label="$t('family.form.description')" data-testid="family-description-input"
          prepend-inner-icon="mdi-text-box-outline"></v-textarea>
      </v-col>
    </v-row>
    <v-row v-if="props.displayLimitConfig">
      <v-col cols="6">
        <v-text-field
          :label="$t('family.form.maxMembers')"
          :model-value="formData.familyLimitConfiguration?.maxMembers"
          readonly
          data-testid="family-max-members"
          prepend-inner-icon="mdi-account-group"
        ></v-text-field>
      </v-col>
      <v-col cols="6">
        <v-text-field
          :label="$t('family.form.maxStorageMb')"
          :model-value="formData.familyLimitConfiguration?.maxStorageMb ? formData.familyLimitConfiguration.maxStorageMb + ' MB' : ''"
          readonly
          data-testid="family-max-storage-mb"
          prepend-inner-icon="mdi-database"
        ></v-text-field>
      </v-col>
      <v-col cols="6">
        <v-text-field
          :label="$t('family.form.aiChatMonthlyLimit')"
          :model-value="formData.familyLimitConfiguration?.aiChatMonthlyLimit ?? ''"
          readonly
          data-testid="family-ai-chat-monthly-limit"
          prepend-inner-icon="mdi-robot"
        ></v-text-field>
      </v-col>
      <v-col cols="6">
        <v-text-field
          :label="$t('family.form.aiChatMonthlyUsage')"
          :model-value="formData.familyLimitConfiguration?.aiChatMonthlyUsage ?? ''"
          readonly
          data-testid="family-ai-chat-monthly-usage"
          prepend-inner-icon="mdi-chart-bar"
        ></v-text-field>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12">
        <UserAutocomplete v-model="managers" multiple :disabled="props.readOnly" hideDetails
          :label="$t('family.permissions.managers')" data-testid="family-managers-select" :loading="isLoadingUsers"
          prepend-inner-icon="mdi-shield-account"></UserAutocomplete>
      </v-col>
      <v-col cols="12">
        <UserAutocomplete v-model="viewers" multiple :disabled="props.readOnly" hideDetails
          :label="$t('family.permissions.viewers')" data-testid="family-viewers-select" :loading="isLoadingUsers"
          prepend-inner-icon="mdi-account-search"></UserAutocomplete>
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
import LocationInputField from '@/components/common/LocationInputField.vue'; // Import the new component
import { useFamilyForm } from '@/composables';
// Removed useLocationDrawerStore as its logic is now in LocationInputField
// import { useLocationDrawerStore } from '@/stores/locationDrawer.store'; 

const props = defineProps<{
  data?: FamilyDto;
  readOnly?: boolean;
  displayLimitConfig?: boolean;
}>();
// Removed emit from props, as it's handled internally now.
defineEmits(['submit']);

const formRef = ref<VForm | null>(null);
// Removed locationDrawerStore as its logic is now in LocationInputField
// const locationDrawerStore = useLocationDrawerStore(); 

const {
  state: { formData, initialAvatarDisplay, managers, viewers, visibilityItems, getFamilyAvatarUrl, rules, isLoadingUsers },
  actions: { validate, getFormData },
} = useFamilyForm(props, formRef);

// Removed openLocationPicker as its logic is now in LocationInputField
/* const openLocationPicker = async () => {
  if (props.readOnly) return;
  try {
    const selectedLocation = await locationDrawerStore.openDrawer((formData as FamilyDto).id || undefined); // Pass familyId, handle undefined
    if (selectedLocation && selectedLocation.address) {
      formData.address = selectedLocation.address;
    }
  } catch (error) {
    console.error('Location selection cancelled or failed:', error);
  }
}; */

defineExpose({
  validate,
  getFormData,
});


</script>
