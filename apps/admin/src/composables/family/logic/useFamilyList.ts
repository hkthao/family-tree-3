import { useI18n } from 'vue-i18n';
import { useRouter } from 'vue-router';
import { useConfirmDialog, useGlobalSnackbar, useCrudDrawer, useFamilyTour } from '@/composables';
import type { FamilyFilter, FamilyDto } from '@/types';
import { useFamilyListFilters, useFamiliesQuery, useDeleteFamilyMutation } from '@/composables';

interface UseFamilyListDeps {
  useI18n: typeof useI18n;
  useRouter: typeof useRouter;
  useConfirmDialog: typeof useConfirmDialog;
  useGlobalSnackbar: typeof useGlobalSnackbar;
  useFamilyTour: typeof useFamilyTour;
  useFamilyListFilters: typeof useFamilyListFilters;
  useFamiliesQuery: typeof useFamiliesQuery;
  useDeleteFamilyMutation: typeof useDeleteFamilyMutation;
  useCrudDrawer: typeof useCrudDrawer;
}

const defaultDeps: UseFamilyListDeps = {
  useI18n,
  useRouter,
  useConfirmDialog,
  useGlobalSnackbar,
  useFamilyTour,
  useFamilyListFilters,
  useFamiliesQuery,
  useDeleteFamilyMutation,
  useCrudDrawer,
};

export function useFamilyList(deps: UseFamilyListDeps = defaultDeps) {
  const {
    useI18n,
    useRouter,
    useConfirmDialog,
    useGlobalSnackbar,
    useFamilyTour,
    useFamilyListFilters,
    useFamiliesQuery,
    useDeleteFamilyMutation,
    useCrudDrawer,
  } = deps;

  const router = useRouter();
  const { t } = useI18n();

  const { showConfirmDialog } = useConfirmDialog();
  const { showSnackbar } = useGlobalSnackbar();
  useFamilyTour();

  const familyListFiltersComposables = useFamilyListFilters();
  const {
    state: {
      searchQuery: familyListSearchQuery,
      itemsPerPage,
      sortBy,
      filters,
      page, // Added
    },
    actions: {
      setPage,
      setItemsPerPage,
      setSortBy,
      setSearchQuery,
      setFilters,
    }
  } = familyListFiltersComposables;

  const { families, totalItems, loading, refetch } = useFamiliesQuery(filters);
  const { mutate: deleteFamily } = useDeleteFamilyMutation();

  const {
    addDrawer,
    openAddDrawer,
    closeAllDrawers,
  } = useCrudDrawer();

  const handleFilterUpdate = (newFilters: FamilyFilter) => {
    setFilters(newFilters);
  };

  const navigateToFamilyDetail = (item: FamilyDto) => {
    router.push({ name: 'FamilyDetail', params: { id: item.id } });
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

  const confirmDelete = async (family: FamilyDto) => {
    const confirmed = await showConfirmDialog({
      title: t('confirmDelete.title'),
      message: t('confirmDelete.message', { name: family.name || '' }),
      confirmText: t('common.delete'),
      cancelText: t('common.cancel'),
      confirmColor: 'error',
    });

    if (confirmed) {
      deleteFamily(family.id, {
        onSuccess: () => {
          showSnackbar(
            t('family.management.messages.deleteSuccess'),
            'success',
          );
          refetch(); // Refetch the list after successful deletion
        },
        onError: (error) => {
          showSnackbar(
            error.message || t('family.management.messages.deleteError'),
            'error',
          );
        },
      });
    }
  };

  const handleFamilyAddClosed = () => {
    closeAllDrawers(); // Close the drawer on cancel
    refetch(); // Refetch the list in case a family was added
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
      addDrawer,
      page,
    },
    actions: {
      handleFilterUpdate,
      navigateToFamilyDetail,
      handleSearchUpdate,
      handleListOptionsUpdate,
      confirmDelete,
      openAddDrawer,
      handleFamilyAddClosed,
      setItemsPerPage,
      setPage,
      setSortBy,
      setSearchQuery,
      setFilters,
      t,
      refetch,
    },
  };
}
