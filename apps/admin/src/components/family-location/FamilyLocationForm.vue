<template>
  <v-form ref="formRef" @submit.prevent>
    <v-row>
      <v-col cols="12" md="12">
        <v-select v-model="form.locationType" :items="locationTypeOptions"
          :label="t('familyLocation.form.locationType')" :rules="rules.locationType" required
          data-testid="location-type" item-title="title" item-value="value"
          prepend-inner-icon="mdi-map-marker-question"></v-select>
      </v-col>
      <v-col cols="12" md="12">
        <v-text-field v-model="form.locationName" :label="t('familyLocation.form.name')" :rules="rules.locationName" required
          data-testid="location-name" prepend-inner-icon="mdi-map-marker-account"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-textarea v-model="form.locationDescription" :label="t('familyLocation.form.description')" rows="2"
          data-testid="location-description" prepend-inner-icon="mdi-text-box-outline"></v-textarea>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model="form.locationAddress" :label="t('familyLocation.form.address')"
          data-testid="location-address" prepend-inner-icon="mdi-map-marker"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model.number="form.locationLatitude" :label="t('familyLocation.form.latitude')" type="number" step="any"
          data-testid="location-latitude" :rules="rules.locationLatitude" prepend-inner-icon="mdi-latitude"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model.number="form.locationLongitude" :label="t('familyLocation.form.longitude')" type="number"
          step="any" data-testid="location-longitude" :rules="rules.locationLongitude" prepend-inner-icon="mdi-longitude"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="form.locationAccuracy" :items="locationAccuracyOptions" :label="t('familyLocation.form.accuracy')"
          :rules="rules.locationAccuracy" required data-testid="location-accuracy" item-title="title" item-value="value"
          prepend-inner-icon="mdi-chart-bar"></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="form.locationSource" :items="locationSourceOptions" :label="t('familyLocation.form.source')"
          :rules="rules.locationSource" required data-testid="location-source" item-title="title" item-value="value"
          prepend-inner-icon="mdi-map-marker-check"></v-select>
      </v-col>
    </v-row>
  </v-form>
</template>
<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VForm } from 'vuetify/components';
import type { FamilyLocation, AddFamilyLocationDto, UpdateFamilyLocationDto } from '@/types';
import {
  useFamilyLocationFormLogic,
  useFamilyLocationValidationRules,
  useFamilyLocationFormActions,
} from '@/composables';
interface FamilyLocationFormProps {
  initialFamilyLocationData?: (AddFamilyLocationDto & Partial<UpdateFamilyLocationDto> & { id?: string }) | null; // Allow null for initial data
  familyId: string;
  readOnly?: boolean;
}
const props = defineProps<FamilyLocationFormProps>();
const formRef = ref<VForm | null>(null);
const { t } = useI18n();
const { state: { form, locationTypeOptions, locationAccuracyOptions, locationSourceOptions } } = useFamilyLocationFormLogic(props);
const rules = useFamilyLocationValidationRules();
const { actions: { getFormData, setCoordinates, setAddress } } = useFamilyLocationFormActions({ form });
async function validate() {
  return (await formRef.value?.validate())?.valid || false;
}
defineExpose({
  validate,
  getFormData,
  setCoordinates,
  setAddress, // Expose setAddress
});
</script>
