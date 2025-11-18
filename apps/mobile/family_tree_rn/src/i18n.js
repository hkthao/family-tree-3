import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

// Import translation files
import en from '../assets/locales/en.json';
import vi from '../assets/locales/vi.json';

const resources = {
  en: {
    translation: en,
  },
  vi: {
    translation: vi,
  },
};

i18n
  .use(initReactI18next) // passes i18n down to react-i18next
  .init({
    resources,
    lng: 'vi', // Set default language to Vietnamese
    fallbackLng: 'vi', // Fallback language if not found

    interpolation: {
      escapeValue: false, // react already safes from xss
    },
  });

export default i18n;
