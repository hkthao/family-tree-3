export interface MenuItem {
  titleKey: string;
  icon: string;
  to?: string;
  roles?: string[];
  exact?: boolean;
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
        titleKey: 'family.manageFamilies',
        icon: 'mdi-home-group',
        to: '/family',
        roles: ['Admin', 'FamilyManager'],
        exact: true,
      },
      {
        titleKey: 'event.list.title', // New: Title for Event List
        icon: 'mdi-calendar-month-outline', // New: Icon for Event List
        to: '/event', // New: Route for Event List
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'], // New: Roles for Event List
        exact: true,
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
    titleKey: 'menu.utilitiesAndAI', // New section for AI Memorial Studio
    items: [
      {
        titleKey: 'menu.aiMemorialStudio', // New consolidated menu item
        icon: 'mdi-brain', // A suitable icon for AI Memorial Studio
        to: '/ai-memorial-studio', // Points to the selection page
        roles: ['Admin', 'FamilyManager', 'Editor'],
      },
    ],
  },
  {
    titleKey: 'menu.dictionary',
    items: [
      {
        titleKey: 'menu.familyDict',
        icon: 'mdi-book-multiple',
        to: '/family-dict',
        roles: ['Admin'],
      },
    ],
  },
  {
    titleKey: 'menu.supportAndInfo',
    items: [
      {
        titleKey: 'menu.applicationInfo',
        icon: 'mdi-information-outline',
        to: '/application-info',
        children: [
          {
            titleKey: 'about.title',
            icon: 'mdi-information-outline',
          },
          {
            titleKey: 'version.title',
            icon: 'mdi-tag-outline',
          },
        ],
      },
      {
        titleKey: 'menu.supportAndLegal',
        icon: 'mdi-lifebuoy',
        to: '/support-legal',
        children: [
          {
            titleKey: 'help.title',
            icon: 'mdi-help-circle-outline',
          },
          {
            titleKey: 'terms.title',
            icon: 'mdi-file-document-outline',
          },
          {
            titleKey: 'privacy.title',
            icon: 'mdi-shield-lock-outline',
          },
        ],
      },
      {
        titleKey: 'menu.donate',
        icon: 'mdi-gift-outline',
        to: '/donate',
      },
    ],
  },
];

export default menu;