import { createApp } from 'vue';
import { createPinia } from 'pinia';
import { Resize } from 'vuetify/directives';

import App from './App.vue';
import router from './router';
import './styles/variables.scss'; // Custom SASS variables
import 'vuetify/styles';
import vuetify from './plugins/vuetify';
import i18n from './plugins/i18n';

import { ServicesPlugin } from './plugins/services.plugin'; // Import ServicesPlugin
import { setAuthService } from '@/services/auth/authService';
import { fakeAuthService } from '@/services/auth/fakeAuthService';

const app = createApp(App);

const pinia = createPinia(); // Create pinia instance
pinia.use(ServicesPlugin()); // Use the services plugin
app.use(pinia); // Use pinia with the app
app.use(router);
app.use(vuetify);
app.use(i18n);
i18n.global.locale.value = 'vi';
app.directive('resize', Resize);

setAuthService(fakeAuthService); // Initialize auth service

app.mount('#app');
