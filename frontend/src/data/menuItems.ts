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


    ],
  },
  {
    titleKey: 'menu.profileAndContent',
    items: [
      {
        titleKey: 'profile.myProfile',
        icon: 'mdi-account',
        to: '/profile',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
      {
        titleKey: 'profile.edit',
        icon: 'mdi-account-edit',
        to: '/profile/edit',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
      {
        titleKey: 'profile.attachments',
        icon: 'mdi-file-document-multiple',
        to: '/profile/attachments',
        roles: ['Admin', 'FamilyManager', 'Editor'],
      },
      {
        titleKey: 'profile.voice',
        icon: 'mdi-microphone',
        to: '/profile/voice',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
      {
        titleKey: 'ai.bioSuggest',
        icon: 'mdi-robot',
        to: '/ai/bio-suggest',
        roles: ['Admin', 'FamilyManager', 'Editor'],
      },
    ],
  },
  {
    titleKey: 'menu.admin',
    items: [
      {
        titleKey: 'admin.users',
        icon: 'mdi-account-cog',
        to: '/admin/users',
        roles: ['Admin'],
      },
      {
        titleKey: 'admin.roles',
        icon: 'mdi-shield-account',
        to: '/admin/roles',
        roles: ['Admin'],
      },
      {
        titleKey: 'admin.audit',
        icon: 'mdi-file-document',
        to: '/admin/audit',
        roles: ['Admin'],
      },
      {
        titleKey: 'admin.invite',
        icon: 'mdi-email-plus',
        to: '/admin/invite',
        roles: ['Admin', 'FamilyManager'],
        badge: { text: '5', color: 'purple' },
      },
      {
        titleKey: 'admin.multiTree',
        icon: 'mdi-sitemap',
        to: '/admin/multi-tree',
        roles: ['Admin', 'FamilyManager'],
      },
      {
        titleKey: 'admin.duplicates',
        icon: 'mdi-magnify-remove-outline',
        to: '/admin/duplicates',
        roles: ['Admin', 'FamilyManager'],
      },
    ],
  },
  {
    titleKey: 'menu.utilitiesAndAI',
    items: [
      {
        titleKey: 'search.smart',
        icon: 'mdi-magnify',
        to: '/search/smart',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
      {
        titleKey: 'search.relationship',
        icon: 'mdi-link-variant',
        to: '/search/relationship',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
      {
        titleKey: 'ai.faceTag',
        icon: 'mdi-face-recognition',
        to: '/ai/face-tag',
        roles: ['Admin', 'FamilyManager'],
      },
      {
        titleKey: 'search.face',
        icon: 'mdi-image-search',
        to: '/search/face',
        roles: ['Admin', 'FamilyManager'],
      },
      {
        titleKey: 'ai.chatbot',
        icon: 'mdi-chat',
        to: '/ai/chatbot',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
      {
        titleKey: 'realtime.collab',
        icon: 'mdi-account-group',
        to: '/realtime/collab',
        roles: ['Admin', 'FamilyManager', 'Editor'],
      },
    ],
  },
  {
    titleKey: 'menu.cultureAndTraditions',
    items: [
      {
        titleKey: 'culture.traditions',
        icon: 'mdi-calendar-star',
        to: '/culture/traditions',
        roles: ['Admin', 'FamilyManager', 'Editor', 'Viewer'],
      },
      {
        titleKey: 'notifications.anniversaries',
        icon: 'mdi-bell-ring',
        to: '/notifications/anniversaries',
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
        to: '/settings/system',
        roles: ['Admin'],
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
