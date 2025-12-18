import { computed, watch, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import type { FamilyLocation } from '@/types';
import { LocationAccuracy, LocationSource, LocationType } from '@/types';

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

  return {
    form,
    defaultForm,
    locationTypeOptions,
    locationAccuracyOptions,
    locationSourceOptions,
  };
}
