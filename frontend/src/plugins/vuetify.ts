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
        colors: {
          primary: '#696CFF',
          secondary: '#5CBBF6',
        },
      },
    },
  },
});
