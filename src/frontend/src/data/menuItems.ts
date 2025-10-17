export interface MenuItem {
  titleKey: string;
  icon: string;
  to?: string;
  roles?: string[];
  badge?: {
    text: string;
    color: string;
  };
  children?: MenuItem[];
}

export interface MenuSection {
  title?: string;
  titleKey?: string;
  items: MenuItem[];
}

const menu: MenuSection[] = [
  {
    titleKey: 'menu.dashboards',
    items: [
      {
        titleKey: 'dashboard.overview',
        icon: 'mdi-view-dashboard',
        to: '/dashboard',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
    ],
  },
  {
    titleKey: 'menu.family',
    items: [
      {
        titleKey: 'family.view',
        icon: 'mdi-family-tree',
        to: '/family/tree',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },

      {
        titleKey: 'family.manageMembers',
        icon: 'mdi-account-multiple',
        to: '/members',
        roles: ['Admin', 'FamilyManager', 'Editor'],
      },
      {
        titleKey: 'family.manageFamilies',
        icon: 'mdi-home-group',
        to: '/family',
        roles: ['Admin', 'FamilyManager'],
      },
      {
        titleKey: 'event.list.title',
        icon: 'mdi-calendar-month',
        to: '/events',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
      {
        titleKey: 'relationship.list.title',
        icon: 'mdi-relation-many-to-many',
        to: '/relationships',
        roles: ['Admin', 'FamilyManager', 'Editor'],
      },
      {
        titleKey: 'ai.faceTag',
        icon: 'mdi-face-recognition',
        to: '/face/labeling',
        roles: ['Admin', 'FamilyManager'],
      },
      {
        titleKey: 'search.face',
        icon: 'mdi-image-search',
        to: '/search/face',
        roles: ['Admin', 'FamilyManager'],
      },
      {
        titleKey: 'face.upload',
        icon: 'mdi-upload',
        to: '/face/upload',
        roles: ['Admin', 'FamilyManager'],
      },
    ],
  },

  {
    titleKey: 'menu.system',
    items: [
      {
        titleKey: 'settings.system',
        icon: 'mdi-cog',
        to: '/settings/system',
        roles: ['Admin'],
      },
      {
        titleKey: 'userSettings.title',
        icon: 'mdi-cog',
        to: '/settings',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
      {
        titleKey: 'settings.account',
        icon: 'mdi-lock',
        to: '/settings/account',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
      {
        titleKey: 'settings.importExport',
        icon: 'mdi-database-export',
        to: '/settings/import-export',
        roles: ['Admin', 'FamilyManager'],
      },
      {
        titleKey: 'reports.statistics',
        icon: 'mdi-chart-box',
        to: '/reports',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
    ],
  },
];

export default menu;
