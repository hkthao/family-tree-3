// src/plugins/i18n.ts
import { createI18n } from 'vue-i18n';
import en from '../locales/en.json';
import vi from '../locales/vi.json';

// Vuetify locales
import { en as vuetifyEn, vi as vuetifyVi } from 'vuetify/locale';

export default createI18n({
  legacy: false, // use composition API
  locale: 'vi', // set default locale
  fallbackLocale: 'en', // set fallback locale
  messages: {
    en: {
      ...en,
      $vuetify: vuetifyEn,
    },
    vi: {
      ...vi,
      $vuetify: vuetifyVi,
    },
  },
});
