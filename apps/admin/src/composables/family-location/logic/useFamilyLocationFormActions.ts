import { LocationSource, type FamilyLocation } from '@/types';

interface UseFamilyLocationFormActionsProps {
  form: FamilyLocation;
}

export function useFamilyLocationFormActions(props: UseFamilyLocationFormActionsProps) {

  const getFormData = (): FamilyLocation => {
    return { ...props.form };
  };

  const setCoordinates = (latitude: number, longitude: number) => {
    props.form.latitude = latitude;
    props.form.longitude = longitude;
    props.form.source = LocationSource.UserSelected; // Set source to UserSelected when coordinates are chosen from map
  };

  const setAddress = (address: string) => {
    props.form.address = address;
    props.form.source = LocationSource.UserSelected; // Also set source to UserSelected when address is chosen from map
  };

  return {
    getFormData,
    setCoordinates,
    setAddress,
  };
}