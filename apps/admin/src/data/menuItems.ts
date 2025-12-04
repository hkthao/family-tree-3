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
        exact: true,
      },
      {
        titleKey: 'event.list.title', 
        icon: 'mdi-calendar-month-outline', 
        to: '/event', 
        exact: true,
      },
      {
        titleKey: 'search.face',
        icon: 'mdi-magnify-expand',
        to: '/face/search',
        exact: true,
      },
      {
        titleKey: 'memberFace.list.title',
        icon: 'mdi-face-recognition',
        to: '/member/faces',
        exact: true,
      },
      {
        titleKey: 'memberStory.list.title', 
        icon: 'mdi-book-open-outline', 
        to: '/member-story-studio/list', 
        exact: true,
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
    titleKey: 'menu.admin',
    items: [
      {
        titleKey: 'menu.prompts',
        icon: 'mdi-text-box-multiple-outline',
        to: '/prompts',
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
      },
      {
        titleKey: 'menu.supportAndLegal',
        icon: 'mdi-lifebuoy',
        to: '/support-legal',
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