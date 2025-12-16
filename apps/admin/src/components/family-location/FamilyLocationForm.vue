<template>
  <v-form ref="formRef" @submit.prevent>
    <v-row>
      <v-col cols="12" md="12">
        <v-select v-model="form.locationType" :items="locationTypeOptions"
          :label="t('familyLocation.form.locationType')"
          :rules="[v => v !== null && v !== undefined || t('common.validations.required')]" required
          data-testid="location-type" item-title="title" item-value="value"></v-select>
      </v-col>
      <v-col cols="12" md="12">
        <v-text-field v-model="form.name" :label="t('familyLocation.form.name')"
          :rules="[v => !!v || t('common.validations.required')]" required data-testid="name"></v-text-field>
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
          data-testid="latitude"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-text-field v-model.number="form.longitude" :label="t('familyLocation.form.longitude')" type="number"
          step="any" data-testid="longitude"></v-text-field>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="form.accuracy" :items="locationAccuracyOptions" :label="t('familyLocation.form.accuracy')"
          :rules="[v => v !== null && v !== undefined || t('common.validations.required')]" required
          data-testid="accuracy" item-title="title" item-value="value"></v-select>
      </v-col>
      <v-col cols="12" md="6">
        <v-select v-model="form.source" :items="locationSourceOptions" :label="t('familyLocation.form.source')"
          :rules="[v => v !== null && v !== undefined || t('common.validations.required')]" required
          data-testid="source" item-title="title" item-value="value"></v-select>
      </v-col>
    </v-row>
  </v-form>
</template>

<script setup lang="ts">
import { ref, computed, watch, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import type { VForm } from 'vuetify/components';
import type { FamilyLocation } from '@/types';
import { LocationAccuracy, LocationSource, LocationType } from '@/types';

interface FamilyLocationFormProps {
  initialFamilyLocationData?: FamilyLocation;
  familyId: string;
  readOnly?: boolean;
}

const props = defineProps<FamilyLocationFormProps>();

const formRef = ref<VForm | null>(null);
const { t } = useI18n();

const defaultForm: Omit<FamilyLocation, 'id' | 'created' | 'createdBy' | 'lastModified' | 'lastModifiedBy'> = {
  familyId: props.familyId,
  name: '',
  description: undefined,
  latitude: undefined,
  longitude: undefined,
  address: undefined,
  locationType: LocationType.Other,
  accuracy: LocationAccuracy.Estimated,
  source: LocationSource.UserSelected,
};

const form = reactive<FamilyLocation>(
  props.initialFamilyLocationData ? { ...props.initialFamilyLocationData } : { ...defaultForm, id: '' },
);

// Watch for changes in initialFamilyLocationData and update the form
watch(() => props.initialFamilyLocationData, (newData) => {
  if (newData) {
    Object.assign(form, newData);
  } else {
    Object.assign(form, { ...defaultForm, id: '' });
  }
});

const locationTypeOptions = computed(() => [
  { title: t('familyLocation.locationType.grave'), value: LocationType.Grave },
  { title: t('familyLocation.locationType.homeland'), value: LocationType.Homeland },
  { title: t('familyLocation.locationType.ancestralHall'), value: LocationType.AncestralHall },
  { title: t('familyLocation.locationType.cemetery'), value: LocationType.Cemetery },
  { title: t('familyLocation.locationType.eventLocation'), value: LocationType.EventLocation },
  { title: t('familyLocation.locationType.other'), value: LocationType.Other },
]);

const locationAccuracyOptions = computed(() => [
  { title: t('familyLocation.accuracy.exact'), value: LocationAccuracy.Exact },
  { title: t('familyLocation.accuracy.approximate'), value: LocationAccuracy.Approximate },
  { title: t('familyLocation.accuracy.estimated'), value: LocationAccuracy.Estimated },
]);

const locationSourceOptions = computed(() => [
  { title: t('familyLocation.source.userselected'), value: LocationSource.UserSelected },
  { title: t('familyLocation.source.geocoded'), value: LocationSource.Geocoded },
]);

const validate = async () => {
  if (!formRef.value) return false;
  const { valid } = await formRef.value.validate();
  return valid;
};

const getFormData = (): FamilyLocation => {
  return { ...form };
};

const setCoordinates = (latitude: number, longitude: number) => {
  form.latitude = latitude;
  form.longitude = longitude;
  form.source = LocationSource.UserSelected; // Set source to UserSelected when coordinates are chosen from map
};

const setAddress = (address: string) => {
  form.address = address;
  form.source = LocationSource.UserSelected; // Also set source to UserSelected when address is chosen from map
};

defineExpose({
  validate,
  getFormData,
  setCoordinates,
  setAddress, // Expose setAddress
});
</script>
