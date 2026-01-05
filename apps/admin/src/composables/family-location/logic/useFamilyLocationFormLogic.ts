import { watch, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import type { AddFamilyLocationDto, UpdateFamilyLocationDto } from '@/types';
import { LocationAccuracy, LocationSource, LocationType } from '@/types';
import { getLocationTypeOptions, getLocationAccuracyOptions, getLocationSourceOptions } from '@/composables/utils/familyLocationOptions';

interface UseFamilyLocationFormLogicProps {
  initialFamilyLocationData?: (AddFamilyLocationDto & Partial<UpdateFamilyLocationDto> & { id?: string }) | null;
  familyId: string;
}

export function useFamilyLocationFormLogic(props: UseFamilyLocationFormLogicProps) {
  const { t } = useI18n();

  // The form state should directly reflect the structure of Add/Update DTOs
  const defaultForm: Omit<AddFamilyLocationDto & Partial<UpdateFamilyLocationDto>, 'familyId'> & { id?: string } = {
    locationName: '',
    locationDescription: undefined,
    locationLatitude: undefined,
    locationLongitude: undefined,
    locationAddress: undefined,
    locationType: LocationType.Other,
    locationAccuracy: LocationAccuracy.Estimated,
    locationSource: LocationSource.UserSelected,
  };

  const initializeForm = (data?: (AddFamilyLocationDto & Partial<UpdateFamilyLocationDto> & { id?: string }) | null): (AddFamilyLocationDto & Partial<UpdateFamilyLocationDto> & { id?: string }) => {
    if (data) {
      return {
        ...data,
      };
    }
    return { ...defaultForm, familyId: props.familyId };
  };

  const form = reactive<AddFamilyLocationDto & Partial<UpdateFamilyLocationDto> & { id?: string }>(initializeForm(props.initialFamilyLocationData));

  // Watch for changes in initialFamilyLocationData and update the form
  watch(() => props.initialFamilyLocationData, (newData) => {
    Object.assign(form, initializeForm(newData));
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
