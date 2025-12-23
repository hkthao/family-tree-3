import { watch, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyLocation } from '@/types';
import { LocationAccuracy, LocationSource, LocationType } from '@/types';
import { getLocationTypeOptions, getLocationAccuracyOptions, getLocationSourceOptions } from '@/composables/utils/familyLocationOptions';

interface UseFamilyLocationFormLogicProps {
  initialFamilyLocationData?: FamilyLocation;
  familyId: string;
}

export function useFamilyLocationFormLogic(props: UseFamilyLocationFormLogicProps) {
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

  const locationTypeOptions = getLocationTypeOptions(t);
  const locationAccuracyOptions = getLocationAccuracyOptions(t);
  const locationSourceOptions = getLocationSourceOptions(t);

  return {
    state: {
      form,
      defaultForm,
      locationTypeOptions,
      locationAccuracyOptions,
      locationSourceOptions,
    },
    actions: {}, // No explicit actions returned from this composable
  };
}
