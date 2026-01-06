import { computed, type ComputedRef } from 'vue';
// Interfaces copied from src/data/menuItems.ts
export interface MenuItem {
  titleKey: string;
  icon: string;
  to?: string;
  roles?: string[]; // Use string[] for roles
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

// Raw menu data copied from src/data/menuItems.ts
const rawMenu: MenuSection[] = [
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
        titleKey: 'menu.community', // This is now the item title
        icon: 'mdi-account-group',
        to: '/community/families',
        exact: true,
      },
      {
        titleKey: 'relationshipDetection.title',
        icon: 'mdi-graph-outline',
        to: '/relationship-detection',
        roles: ['Admin', 'Manager'],
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
      {
        titleKey: 'admin.userPushTokens',
        icon: 'mdi-bell-badge-outline',
        to: '/users/current/push-tokens', // Use a placeholder or actual user ID for now, will refine
        roles: ['Admin'], // Ensure this role matches the backend role
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

// hasPermissionToMenuItem logic (copied from src/utils/auth.ts or adapted)
const hasPermissionToMenuItem = (item: MenuItem, userRoles: string[]): boolean => {
  if (!item.roles || item.roles.length === 0) {
    return true; // No roles defined, so accessible by everyone
  }
  return item.roles.some(role => userRoles.includes(role));
};

export function useSidebarMenu(userRoles: ComputedRef<string[]>) {
  const filteredMenu = computed(() => {
    const menuData = rawMenu.map(section => {
      const filteredItems = section.items.map(item => {
        if (item.children && item.children.length > 0) {
          const filteredChildren = item.children.filter(child =>
            hasPermissionToMenuItem(child, userRoles.value)
          );
          // Only return the parent item if it or any of its children are visible
          if (hasPermissionToMenuItem(item, userRoles.value) || filteredChildren.length > 0) {
            return { ...item, children: filteredChildren };
          }
          return null; // Exclude parent if neither it nor its children are visible
        }
        return hasPermissionToMenuItem(item, userRoles.value) ? item : null;
      }).filter(item => item !== null); // Remove null items

      return { ...section, items: filteredItems };
    }).filter(section => section.items.length > 0); // Remove empty sections
    return menuData;
  });

  return {
    filteredMenu,
  };
}
