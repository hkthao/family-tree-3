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
    ],
  },
  {
    titleKey: 'face.sidebar.title',
    items: [
      {
        titleKey: 'face.recognition.title',
        icon: 'mdi-face-recognition',
        to: '/face/recognition',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
      {
        titleKey: 'search.face',
        icon: 'mdi-magnify-expand',
        to: '/face/search',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
    ],
  },

  {
    titleKey: 'menu.system',
    items: [
      {
        titleKey: 'settings.system',
        icon: 'mdi-cog',
        to: '/admin/config',
        roles: ['Admin'],
      },
    ],
  },
];

export default menu;
