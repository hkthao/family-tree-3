import { ref, watch, onMounted } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';

interface BreadcrumbItem {
  title: string;
  disabled: boolean;
  to?: string;
}

export function useAppBreadcrumbs() {
  const route = useRoute();
  const { t } = useI18n();

  const breadcrumbs = ref<BreadcrumbItem[]>([]);

  const generateBreadcrumbs = () => {
    const matchedRoutes = route.matched;
    const newBreadcrumbs: BreadcrumbItem[] = [];

    matchedRoutes.forEach((match) => {
      if (match.meta && match.meta.breadcrumb) {
        let to = match.path;
        // For dynamic routes, the breadcrumb should link to the parent list view
        if (match.path.includes(':id')) {
          // Example: /member/detail/:id -> /member
          // Example: /member/edit/:id -> /member
          to = match.path.substring(0, match.path.lastIndexOf('/'));
        }
        // For add routes, the breadcrumb should link to the parent list view
        if (match.path.endsWith('/add')) {
          // Example: /member/add -> /member
          to = match.path.substring(0, match.path.lastIndexOf('/'));
        }

        const newBreadcrumb: BreadcrumbItem = {
          title: t(match.meta.breadcrumb as string),
          disabled: match.path === route.path,
          to: to,
        };

        // Prevent adding duplicate breadcrumbs (e.g., for parent and default child with same title)
        if (newBreadcrumbs.length > 0 &&
            newBreadcrumbs[newBreadcrumbs.length - 1].title === newBreadcrumb.title &&
            newBreadcrumbs[newBreadcrumbs.length - 1].to === newBreadcrumb.to) {
          return; // Skip adding duplicate
        }

        newBreadcrumbs.push(newBreadcrumb);
      }
    });

    // Add a default home breadcrumb if not present
    if (newBreadcrumbs.length === 0 || newBreadcrumbs[0].to !== '/') {
      newBreadcrumbs.unshift({
        title: t('dashboard.overview'), // Assuming 'dashboard.overview' is the home breadcrumb
        disabled: false,
        to: '/',
      });
    }

    if (newBreadcrumbs.length > 0) {
      newBreadcrumbs[newBreadcrumbs.length - 1].disabled = true;
    }

    breadcrumbs.value = newBreadcrumbs;
  };

  watch(
    () => route.path,
    () => {
      generateBreadcrumbs();
    },
    { immediate: true }
  );

  onMounted(() => {
    generateBreadcrumbs();
  });

  return {
    breadcrumbs,
  };
}
