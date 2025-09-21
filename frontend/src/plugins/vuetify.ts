// src/plugins/vuetify.ts

// Styles
import 'vuetify/styles';
import '@mdi/font/css/materialdesignicons.css';

// Composables
import { createVuetify, VCalendar, VResize } from 'vuetify';

// i18n
import { createVueI18nAdapter } from 'vuetify/locale/adapters/vue-i18n';
import { useI18n } from 'vue-i18n';
import i18n from './i18n';

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
  directives: {
    resize: VResize,
  },
  locale: {
    adapter: createVueI18nAdapter({ i18n, useI18n }),
  },
  theme: {
    defaultTheme: 'dark',
    themes: {
      light: {
        dark: false,
        colors: {
          primary: '#696CFF',
          secondary: '#8592A3',
          success: '#71DD37',
          error: '#FF3E1D',
          warning: '#FFAB00',
        },
      },
      dark: {
        dark: true,
        colors: {
          primary: '#696CFF',
          secondary: '#8592A3',
          success: '#71DD37',
          error: '#FF3E1D',
          warning: '#FFAB00',
        },
      },
    },
  },
  defaults: {
    global: {
    },
    VTextField: {
      variant: 'outlined',
      density: 'compact',
    },
    VAutocomplete: {
      variant: 'outlined',
      density: 'compact',
    },
    VSelect: {
      variant: 'outlined',
      density: 'compact',
    },
    VTextarea: {
      variant: 'outlined',
    },
    VBtn: {
      variant: 'outlined',
    },
  },
});