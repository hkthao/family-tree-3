import FamilyMediaListView from '@/views/family-media/FamilyMediaListView.vue';
import { AppRoute } from '@/types'; // AppRoute is now in '@/types'

export const familyMediaRoutes: AppRoute[] = [
  {
    path: 'family-media',
    name: 'FamilyMediaList',
    component: FamilyMediaListView,
    meta: {
      requiresAuth: true,
      // Add any specific roles required to view family media
      // roles: ['Admin', 'FamilyManager', 'Viewer'],
      title: 'familyMedia.list.pageTitle', // For i18n
      icon: 'mdi-folder-multiple-image', // Example icon
    },
  },
];
