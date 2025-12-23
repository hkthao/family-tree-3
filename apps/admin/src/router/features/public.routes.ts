import type { RouteRecordRaw } from 'vue-router';
import { FaqPage, SupportLegalPage, FamilyTreeMobileView } from '@/views'; // Add FamilyTreeMobileView
import MobileMapView from '@/views/family-location/MobileMapView.vue'; // Import MobileMapView
import MobileMapPickerView from '@/views/family-location/MobileMapPickerView.vue'; // Import MobileMapView

export const publicRoutes: RouteRecordRaw[] = [
  {
    path: '/public/support-legal',
    name: 'PublicSupportLegal',
    component: SupportLegalPage,
    meta: { requiresAuth: false }, // Public route does not require authentication
  },
  {
    path: '/public/faq', // New public FAQ route
    name: 'PublicFaqPage',
    component: FaqPage,
    meta: { requiresAuth: false }, // Public route does not require authentication
  },
  {
    path: '/public/mobile/map',
    name: 'MobileMap',
    component: MobileMapView,
    meta: { requiresAuth: false }, // Public route does not require authentication
  },
  {
    path: '/public/mobile/map-picker',
    name: 'MobileMapPicker',
    component: MobileMapPickerView,
    meta: { requiresAuth: false }, // Public route does not require authentication
  },
  {
    path: '/public/mobile/tree-view', // A more descriptive public path
    name: 'FamilyTreeMobileView',
    component: FamilyTreeMobileView,
    meta: { requiresAuth: false },
  },
];
