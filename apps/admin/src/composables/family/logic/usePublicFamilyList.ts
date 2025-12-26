import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useConfirmDialog, useGlobalSnackbar } from '@/composables'; // Removed useCrudDrawer, useFamilyTour
import type { FamilyFilter, FamilyDto } from '@/types';
import { useFamilyListFilters } from '@/composables/family/filters/useFamilyListFilters'; // Re-use FamilyListFilters
import { usePublicFamiliesQuery } from '@/composables/family/queries/usePublicFamiliesQuery'; // NEW public query

interface UsePublicFamilyListDeps {
  useI18n: typeof useI18n;
  useRouter: typeof useRouter;
  useConfirmDialog: typeof useConfirmDialog;
  useGlobalSnackbar: typeof useGlobalSnackbar;
  useFamilyListFilters: typeof useFamilyListFilters;
  usePublicFamiliesQuery: typeof usePublicFamiliesQuery;
}

const defaultDeps: UsePublicFamilyListDeps = {
  useI18n,
  useRouter,
  useConfirmDialog,
  useGlobalSnackbar,
  useFamilyListFilters,
  usePublicFamiliesQuery,
};

export function usePublicFamilyList(deps: UsePublicFamilyListDeps = defaultDeps) {
  const {
    useI18n,
    useRouter,
    useConfirmDialog,
    useGlobalSnackbar,
    useFamilyListFilters,
    usePublicFamiliesQuery,
  } = deps;

  const router = useRouter();
  const { t } = useI18n();

  const { showConfirmDialog } = useConfirmDialog(); // Keep if needed for other purposes, but not delete
  const { showSnackbar } = useGlobalSnackbar();

  const familyListFiltersComposables = useFamilyListFilters();
  const {
    state: {
      searchQuery: familyListSearchQuery,
      itemsPerPage,
      sortBy,
      filters,
    },
    actions: {
      setPage,
      setItemsPerPage,
      setSortBy,
      setSearchQuery,
      setFilters,
    }
  } = familyListFiltersComposables;

  const { families, totalItems, loading, refetch } = usePublicFamiliesQuery(filters); // Use public query

  const handleFilterUpdate = (newFilters: FamilyFilter) => {
    setFilters(newFilters);
  };

  const navigateToFamilyDetail = (item: FamilyDto) => {
    router.push({ name: 'FamilyDetail', params: { id: item.id } }); // Navigates to existing FamilyDetail route
  };

  const handleSearchUpdate = (search: string) => {
    setSearchQuery(search);
  };

  const handleListOptionsUpdate = (options: {
    page: number;
    itemsPerPage: number;
    sortBy: { key: string; order: string }[];
  }) => {
    setPage(options.page);
    setItemsPerPage(options.itemsPerPage);
    setSortBy(options.sortBy as { key: string; order: 'asc' | 'desc' }[]);
  };

  return {
    state: {
      familyListSearchQuery,
      itemsPerPage,
      sortBy,
      filters,
      families,
      totalItems,
      loading,
    },
    actions: {
      handleFilterUpdate,
      navigateToFamilyDetail,
      handleSearchUpdate,
      handleListOptionsUpdate,
      setItemsPerPage,
      setPage,
      setSortBy,
      setSearchQuery,
      setFilters,
      t,
    },
  };
}