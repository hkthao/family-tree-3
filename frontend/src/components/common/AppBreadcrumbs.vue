<template>
  <v-breadcrumbs :items="breadcrumbs">
    <template v-slot:divider>
      <v-icon icon="mdi-chevron-right"></v-icon>
    </template>
  </v-breadcrumbs>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useI18n } from 'vue-i18n';

interface BreadcrumbItem {
  title: string;
  disabled: boolean;
  href?: string;
}

const route = useRoute();
const router = useRouter();
const { t } = useI18n();

const breadcrumbs = ref<BreadcrumbItem[]>([]);

const generateBreadcrumbs = () => {
  const matchedRoutes = route.matched;
  const newBreadcrumbs: BreadcrumbItem[] = [];

  matchedRoutes.forEach((match) => {
    if (match.meta && match.meta.breadcrumb) {
      let href = match.path;
      // For dynamic routes, try to navigate to the parent list view
      if (match.path.includes(':id') && route.params.id) {
        href = match.path.substring(0, match.path.lastIndexOf('/')); // Remove the /:id part
      }
      // Special case for /members/add or /family/add, their parent is /members or /family
      if (match.name === 'AddMember' || match.name === 'AddFamily' || match.name === 'AddEvent') {
        href = match.path.substring(0, match.path.lastIndexOf('/'));
      }

      newBreadcrumbs.push({
        title: t(match.meta.breadcrumb as string),
        disabled: match.path === route.path,
        href: href,
      });
    }
  });

  // Add a default home breadcrumb if not present
  if (newBreadcrumbs.length === 0 || newBreadcrumbs[0].href !== '/') {
    newBreadcrumbs.unshift({
      title: t('dashboard.overview'), // Assuming 'dashboard.overview' is the home breadcrumb
      disabled: false,
      href: '/',
    });
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
</script>
