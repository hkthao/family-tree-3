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
        :loading="privacyConfigurationStore.loading"
        @click="savePrivacySettings"
        data-testid="save-privacy-settings-button"
      >
        {{ t('common.save') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { usePrivacyConfigurationStore } from '@/stores/privacy-configuration.store';
import { useGlobalSnackbar } from '@/composables';

const props = defineProps<{
  familyId: string;
}>();

const { t } = useI18n();
const privacyConfigurationStore = usePrivacyConfigurationStore();
const { showSnackbar } = useGlobalSnackbar();

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
  { text: t('member.form.placeOfDeath'), value: 'PlaceOfDeath' },
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

const fetchPrivacySettings = async () => {
  if (props.familyId) {
    const result = await privacyConfigurationStore.fetchPrivacyConfiguration(props.familyId);
    if (result.ok && result.value) {
      selectedProperties.value = result.value.publicMemberProperties;
    } else {
      // If no config exists, default to all properties being public
      selectedProperties.value = memberProperties.value.map(p => p.value);
    }
  }
};

const savePrivacySettings = async () => {
  const result = await privacyConfigurationStore.updatePrivacyConfiguration(
    props.familyId,
    selectedProperties.value,
  );
  if (result.ok) {
    showSnackbar(t('family.privacy.saveSuccess'), 'success');
  } else {
    showSnackbar(t('family.privacy.saveError'), 'error');
    console.error('Error saving privacy settings:', result.error);
  }
};

onMounted(fetchPrivacySettings);

watch(() => props.familyId, fetchPrivacySettings);
</script>

<style scoped>
/* Add any specific styles here */
</style>
