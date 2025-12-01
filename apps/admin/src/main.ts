import { createApp } from 'vue';
import { createPinia } from 'pinia';
import { Resize } from 'vuetify/directives';

import App from './App.vue';
import router from './router';
import './styles/variables.scss'; // Custom SASS variables
import './styles/n8n-chat.scss'; // n8n chat custom variables
import 'vuetify/styles';
import vuetify from './plugins/vuetify';
import i18n from './plugins/i18n';

import { ServicesPlugin } from './plugins/services.plugin'; // Import ServicesPlugin
import { setAuthService } from '@/services/auth/authService';
import { auth0Service } from '@/services/auth/auth0Service';

// Globally register common components
import MemberAutocomplete from '@/components/common/MemberAutocomplete.vue';
import FamilyAutocomplete from '@/components/common/FamilyAutocomplete.vue';

const app = createApp(App);

app.component('member-auto-complete', MemberAutocomplete);
app.component('family-auto-complete', FamilyAutocomplete);

const pinia = createPinia(); // Create pinia instance
pinia.use(ServicesPlugin()); // Use the services plugin
app.use(pinia); // Use pinia with the app
app.use(router);
app.use(vuetify);
app.use(i18n);
i18n.global.locale.value = 'vi';
app.directive('resize', Resize);
setAuthService(auth0Service);
app.mount('#app');
