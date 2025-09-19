// src/plugins/vuetify.ts

// Styles
import 'vuetify/styles';
import '@mdi/font/css/materialdesignicons.css';

// Composables
import { createVuetify } from 'vuetify';

// https://vuetifyjs.com/en/introduction/why-vuetify/#feature-guides
export default createVuetify({
  theme: {
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
});