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
        titleKey: 'event.list.title', // New: Title for Event List
        icon: 'mdi-calendar-month-outline', // New: Icon for Event List
        to: '/event', // New: Route for Event List
        exact: true,
      },
      {
        titleKey: 'search.face',
        icon: 'mdi-magnify-expand',
        to: '/face/search',
      },
      {
        titleKey: 'memberFace.list.title',
        icon: 'mdi-face-recognition',
        to: '/member/faces',
        roles: ['Admin'],
        exact: true,
      },
      {
        titleKey: 'memberStory.list.title', // New: Title for MemberStory List
        icon: 'mdi-book-open-outline', // New: Icon for MemberStory List
        to: '/member-story-studio/list', // New: Route for MemberStory List
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