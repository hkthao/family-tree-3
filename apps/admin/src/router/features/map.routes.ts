const MapView = () => import('@/views/map/MapView.vue');

export const mapRoutes = [
  {
    path: 'map',
    name: 'Map',
    component: MapView,
    meta: {
      requiresAuth: true,
      title: 'Map',
      roles: ['Admin', 'FamilyManager', 'Viewer'], // Adjust roles as needed
    },
  },
];
