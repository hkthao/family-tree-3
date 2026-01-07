<template>
  <div data-testid="user-push-token-list-view">
    <UserPushTokenList
      :items="userPushTokens"
      :total-items="totalItems"
      :loading="isLoadingUserPushTokens"
      :search-query="searchQuery"
      @update:options="handleListOptionsUpdate"
      @update:search="handleSearchUpdate"
      @create="openAddDrawer()"
      @view="openDetailDrawer"
      @edit="openEditDrawer"
      @delete="confirmDelete"
      :allow-add="true"
      :allow-edit="true"
      :allow-delete="true"
    />

    <!-- Add UserPushToken Drawer -->
    <BaseCrudDrawer v-model="addDrawer" @close="handleUserPushTokenClosed">
      <UserPushTokenAddView v-if="addDrawer" @close="handleUserPushTokenClosed" @saved="handleUserPushTokenSaved" />
    </BaseCrudDrawer>

    <!-- Edit UserPushToken Drawer -->
    <BaseCrudDrawer v-model="editDrawer" @close="handleUserPushTokenClosed">
      <UserPushTokenEditView
        v-if="selectedItemId && editDrawer"
        :user-push-token-id="selectedItemId"
        @close="handleUserPushTokenClosed"
        @saved="handleUserPushTokenSaved"
      />
    </BaseCrudDrawer>

    <!-- Detail UserPushToken Drawer -->
    <BaseCrudDrawer v-model="detailDrawer" @close="handleUserPushTokenClosed">
      <UserPushTokenDetailView
        v-if="selectedItemId && detailDrawer"
        :user-push-token-id="selectedItemId"
        @close="handleUserPushTokenClosed"
      />
    </BaseCrudDrawer>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useCrudDrawer, useConfirmDialog, useGlobalSnackbar } from '@/composables';
import { useQueryClient } from '@tanstack/vue-query';
import {
  useUserPushTokensQuery,
  useDeleteUserPushTokenMutation,
  useUserPushTokenDataManagement, // Import the new composable
} from '@/composables/user-push-token';
import type { UserPushTokenDto } from '@/types';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import UserPushTokenList from '@/components/user-push-token/UserPushTokenList.vue';
import UserPushTokenAddView from './UserPushTokenAddView.vue';
import UserPushTokenEditView from './UserPushTokenEditView.vue';
import UserPushTokenDetailView from './UserPushTokenDetailView.vue';
// const props = defineProps<UserPushTokenListViewProps>(); // Removed userId prop

const { t } = useI18n();
const queryClient = useQueryClient();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();

const searchQuery = ref('');

// const userIdRef = ref(props.userId); // Removed userId prop usage
const userIdRef = ref<string | null>(null); // Initialize userIdRef as null to represent no specific user filter by default

// Integrate useUserPushTokenDataManagement
const {
  state: { paginationOptions, filters },
  actions: { setPage, setItemsPerPage, setSortBy, setFilters },
} = useUserPushTokenDataManagement(userIdRef);

const {
  state: { userPushTokens: fetchedUserPushTokens, isLoading: isLoadingUserPushTokens, totalItems: queryTotalItems },
} = useUserPushTokensQuery(userIdRef, paginationOptions, filters); // Pass paginationOptions and filters

const userPushTokens = computed<UserPushTokenDto[]>(() => {
  return fetchedUserPushTokens.value || [];
});

const totalItems = computed(() => queryTotalItems.value); // Use totalItems from query

const { mutate: deleteUserPushToken } = useDeleteUserPushTokenMutation();

const {
  addDrawer,
  editDrawer,
  detailDrawer,
  selectedItemId,
  openAddDrawer,
  openEditDrawer,
  openDetailDrawer,
  closeAllDrawers,
} = useCrudDrawer<string>();

const handleListOptionsUpdate = (options: {
  page: number;
  itemsPerPage: number;
  sortBy: { key: string; order: string }[];
}) => {
  setPage(options.page);
  setItemsPerPage(options.itemsPerPage);
  setSortBy(options.sortBy as { key: string; order: 'asc' | 'desc' }[]);
};

const confirmDelete = async (id: string) => {
  const itemToDelete = userPushTokens.value.find((item) => item.id === id);
  if (!itemToDelete) {
    showSnackbar(t('userPushToken.messages.notFound'), 'error');
    return;
  }

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('userPushToken.list.confirmDelete', { id: itemToDelete.id }),
    confirmText: t('common.delete'),
    cancelText: t('common.cancel'),
    confirmColor: 'error',
  });

  if (confirmed) {
    handleDeleteConfirm(itemToDelete.id);
  }
};

const handleDeleteConfirm = (id: string) => {
  deleteUserPushToken(id, {
    onSuccess: () => {
      showSnackbar(t('userPushToken.messages.deleteSuccess'), 'success');
      queryClient.invalidateQueries({ queryKey: ['user-push-tokens', userIdRef.value, paginationOptions, filters] });
    },
    onError: (error) => {
      showSnackbar((error as Error).message || t('userPushToken.messages.deleteError'), 'error');
    },
  });
};

const handleUserPushTokenSaved = () => {
  closeAllDrawers();
  queryClient.invalidateQueries({ queryKey: ['user-push-tokens', userIdRef.value, paginationOptions, filters] });
};

const handleUserPushTokenClosed = () => {
  closeAllDrawers();
};

const handleSearchUpdate = (newSearchQuery: string) => {
  searchQuery.value = newSearchQuery;
  setFilters({ search: newSearchQuery });
};

// watch(
//   () => props.userId,
//   (newUserId) => {
//     userIdRef.value = newUserId;
//     // refetch(); // Refetch is now handled by the useQuery's queryKey reactivity
//   },
// );

</script>

<style scoped></style>
