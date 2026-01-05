import { LocationSource, type AddFamilyLocationDto, type UpdateFamilyLocationDto } from '@/types';

interface UseFamilyLocationFormActionsProps {
  form: AddFamilyLocationDto | (UpdateFamilyLocationDto & { id?: string });
}

export function useFamilyLocationFormActions(props: UseFamilyLocationFormActionsProps) {

  const getFormData = (): AddFamilyLocationDto | (UpdateFamilyLocationDto & { id?: string }) => {
    return { ...props.form };
  };

  const setCoordinates = (latitude: number, longitude: number) => {
    props.form.locationLatitude = latitude;
    props.form.locationLongitude = longitude;
    props.form.locationSource = LocationSource.UserSelected; // Set source to UserSelected when coordinates are chosen from map
  };

  const setAddress = (address: string) => {
    props.form.locationAddress = address;
    props.form.locationSource = LocationSource.UserSelected; // Also set source to UserSelected when address is chosen from map
  };

  return {
    actions: {
      getFormData,
      setCoordinates,
      setAddress,
    },
  };
}