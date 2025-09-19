import { createApp } from 'vue';
import { createPinia } from 'pinia';

import App from './App.vue';
import router from './router';
import './styles/variables.scss'; // Custom SASS variables
import 'vuetify/styles';
import vuetify from './plugins/vuetify';
import i18n from './plugins/i18n';

import VueApexCharts from "vue3-apexcharts";

const app = createApp(App);

app.use(createPinia());
app.use(router);
app.use(vuetify);
app.use(i18n);
app.use(VueApexCharts);

app.mount('#app');
