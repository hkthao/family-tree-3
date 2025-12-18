import type { RouteRecordRaw } from 'vue-router';
import { FaqPage, SupportLegalPage } from '@/views';
import PublicFamilyTreeViewer from '@/views/PublicFamilyTreeViewer.vue'; // Direct import due to lazy loading syntax
import MobileMapView from '@/views/family-location/MobileMapView.vue'; // Import MobileMapView

export const publicRoutes: RouteRecordRaw[] = [
  {
    path: '/public/family-tree/:familyId/:rootId?',
    name: 'PublicFamilyTreeViewer',
    component: PublicFamilyTreeViewer,
    meta: { requiresAuth: false }, // Public route does not require authentication
  },
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
];
