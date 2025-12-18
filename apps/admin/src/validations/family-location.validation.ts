import { useRules } from 'vuetify/labs/rules';
export const useFamilyLocationValidationRules = () => {
  const rulesVuetify = useRules();

  const rules = {
    name: [
      rulesVuetify.required(),
      rulesVuetify.minLength(3),
      rulesVuetify.maxLength(100),
    ],
    locationType: [
      rulesVuetify.required(),
    ],
    accuracy: [
      rulesVuetify.required(),
    ],
    source: [
      rulesVuetify.required(),
    ],
    latitude: [], // Remove custom numeric validation
    longitude: [], // Remove custom numeric validation
  };

  return rules;
};
