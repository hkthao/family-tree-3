import { MainRouterView, FamilyAddView, FamilyTabsView, FamilyEditView, FamilyListView } from '@/views';
import PublicFamilyListView from '@/views/family/PublicFamilyListView.vue'; // Added for community routes
import type { RouteRecordRaw } from 'vue-router';

export const familyRoutes: RouteRecordRaw[] = [
  {
    path: 'family',
    name: 'FamilyManagement',
    component: MainRouterView,
    meta: { breadcrumb: 'family.management.title' },
    children: [
      {
        path: '',
        name: 'FamilyList',
        component: FamilyListView,
        meta: { breadcrumb: 'family.management.title' },
      },
      {
        path: 'add',
        name: 'AddFamily',
        component: FamilyAddView,
        meta: { breadcrumb: 'family.form.addTitle' },
      },
      {
        path: 'detail/:id',
        name: 'FamilyDetail',
        component: FamilyTabsView,
        meta: { breadcrumb: 'family.detail.title' },
      },
      {
        path: 'edit/:id',
        name: 'EditFamily',
        component: FamilyEditView,
        meta: { breadcrumb: 'family.form.editTitle' },
      },
    ],
  },
  {
    path: '/community/families', // Note the absolute path here, as it's a top-level route
    name: 'community',
    component: MainRouterView,
    children: [
      {
      path: '',
      name:"PublicFamilyList",
      component: PublicFamilyListView,
      meta: {
        requiresAuth: true,
        title: 'menu.community', // Use i18n key for title
        icon: 'mdi-account-group', // Add icon for the route
        breadcrumb: 'menu.community', // Add breadcrumb for the route
      },
    }
    ]
  },
];
