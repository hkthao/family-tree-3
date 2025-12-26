<template>
  <v-form ref="formRef" @submit.prevent>
    <v-row>
      <v-col cols="12" md="12">
        <v-select v-model="form.locationType" :items="locationTypeOptions"
          :label="t('familyLocation.form.locationType')" :rules="rules.locationType" required
          data-testid="location-type" item-title="title" item-value="value"></v-select>
      </v-col>
      <v-col cols="12" md="12">
        <v-text-field v-model="form.name" :label="t('familyLocation.form.name')" :rules="rules.name" required
          data-testid="name"></v-text-field>
      </v-col>
      <v-col cols="12">
        <v-textarea v-model="form.description" :label="t('familyLocation.form.description')" rows="2"
          data-testid="description"></v-textarea>
      </v-col>
      <v-col cols="12">
        <v-text-field v-model="form.address" :label="t('familyLocation.form.address')"
          data-testid="address"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model.number="form.latitude" :label="t('familyLocation.form.latitude')" type="number" step="any"
          data-testid="latitude" :rules="rules.latitude"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model.number="form.longitude" :label="t('familyLocation.form.longitude')" type="number"
          step="any" data-testid="longitude" :rules="rules.longitude"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="form.accuracy" :items="locationAccuracyOptions" :label="t('familyLocation.form.accuracy')"
          :rules="rules.accuracy" required data-testid="accuracy" item-title="title" item-value="value"></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="form.source" :items="locationSourceOptions" :label="t('familyLocation.form.source')"
          :rules="rules.source" required data-testid="source" item-title="title" item-value="value"></v-select>
      </v-col>
    </v-row>
  </v-form>
</template>
<script setup lang="ts">
import { ref } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VForm } from 'vuetify/components';
import type { FamilyLocation } from '@/types';
import {
  useFamilyLocationFormLogic,
  useFamilyLocationValidationRules,
  useFamilyLocationFormActions,
} from '@/composables';
interface FamilyLocationFormProps {
  initialFamilyLocationData?: FamilyLocation | null; // Allow null for initial data
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
