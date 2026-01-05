import { useRules } from 'vuetify/labs/rules';
export const useFamilyLocationValidationRules = () => {
  const rulesVuetify = useRules();

  const rules = {
    locationName: [
      rulesVuetify.required(),
      rulesVuetify.minLength(3),
      rulesVuetify.maxLength(200),
    ],
    locationDescription: [
      rulesVuetify.maxLength(1000),
    ],
    locationAddress: [
      rulesVuetify.maxLength(500),
    ],
    locationType: [
      rulesVuetify.required(),
    ],
    locationAccuracy: [
      rulesVuetify.required(),
    ],
    locationSource: [
      rulesVuetify.required(),
    ],
    locationLatitude: [], // Remove custom numeric validation
    locationLongitude: [], // Remove custom numeric validation
  };

  return rules;
};
