<template>
  <v-card flat>
    <v-card-title class="d-flex align-center pe-2">
      <v-icon icon="mdi-shield-lock"></v-icon> &nbsp;{{ t('family.privacy.title') }}
    </v-card-title>
    <v-card-text>
      <p class="mb-4">{{ t('family.privacy.description') }}</p>

      <v-alert type="info" variant="tonal" class="mb-4">
        {{ t('family.privacy.infoMessage') }}
      </v-alert>

      <v-form ref="formRef">
        <v-row>
          <v-col cols="12" sm="6" md="4" v-for="prop in memberProperties" :key="prop.value">
            <v-checkbox
              v-model="selectedProperties"
              :label="prop.text"
              :value="prop.value"
              hide-details
              density="compact"
            ></v-checkbox>
          </v-col>
        </v-row>
      </v-form>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn
        color="primary"
        :loading="isLoading || isUpdating"
        @click="savePrivacySettings"
        data-testid="save-privacy-settings-button"
      >
        {{ t('common.save') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, watch, computed, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { useGlobalSnackbar } from '@/composables';
import { usePrivacyConfiguration } from '@/composables/family/usePrivacyConfiguration'; // Import the new composable

const props = defineProps<{
  familyId: string;
}>();

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const {
  privacyConfiguration,
  isLoading,
  isUpdating,
  updatePrivacySettings,
} = usePrivacyConfiguration(toRef(props, 'familyId')); // Pass familyId as a ref

const selectedProperties = ref<string[]>([]);
const formRef = ref<HTMLFormElement | null>(null);

// Define all member properties that can be made public/private
const memberProperties = computed(() => [
  { text: t('member.form.lastName'), value: 'LastName' },
  { text: t('member.form.firstName'), value: 'FirstName' },
  { text: t('member.form.nickname'), value: 'Nickname' },
  { text: t('member.form.gender'), value: 'Gender' },
  { text: t('member.form.dateOfBirth'), value: 'DateOfBirth' },
  { text: t('member.form.dateOfDeath'), value: 'DateOfDeath' },
  { text: t('member.form.placeOfBirth'), value: 'PlaceOfBirth' },
  { text: t('member.form.placeOfDeath'), value: 'PlaceOfBirth' },
  { text: t('member.form.phone'), value: 'Phone' },
  { text: t('member.form.email'), value: 'Email' },
  { text: t('member.form.address'), value: 'Address' },
  { text: t('member.form.occupation'), value: 'Occupation' },
  { text: t('member.form.biography'), value: 'Biography' },
  { text: t('member.form.fatherFullName'), value: 'FatherFullName' },
  { text: t('member.form.motherFullName'), value: 'MotherFullName' },
  { text: t('member.form.husbandFullName'), value: 'HusbandFullName' },
  { text: t('member.form.wifeFullName'), value: 'WifeFullName' },
]);

// Watch for changes in privacyConfiguration fetched by the composable
watch(privacyConfiguration, (newConfig) => {
  if (newConfig) {
    selectedProperties.value = newConfig.publicMemberProperties;
  } else {
    // If no config exists, default to all properties being public
    selectedProperties.value = memberProperties.value.map(p => p.value);
  }
}, { immediate: true }); // Immediate to set initial value

const savePrivacySettings = async () => {
  try {
    await updatePrivacySettings(selectedProperties.value);
    showSnackbar(t('family.privacy.saveSuccess'), 'success');
  } catch (err: any) {
    showSnackbar(t('family.privacy.saveError'), 'error');
    console.error('Error saving privacy settings:', err);
  }
};
</script>

<style scoped>
/* Add any specific styles here */
</style>
