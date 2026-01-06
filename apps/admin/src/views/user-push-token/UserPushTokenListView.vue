<template>
  <div data-testid="user-push-token-list-view">
    <UserPushTokenList
      :items="userPushTokens"
      :total-items="totalItems"
      :loading="isLoadingUserPushTokens"
      @update:options="handleListOptionsUpdate"
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
} from '@/composables/user-push-token';
import type { UserPushTokenDto } from '@/types';
import BaseCrudDrawer from '@/components/common/BaseCrudDrawer.vue';
import UserPushTokenList from '@/components/user-push-token/UserPushTokenList.vue';
import UserPushTokenAddView from './UserPushTokenAddView.vue';
import UserPushTokenEditView from './UserPushTokenEditView.vue';
import UserPushTokenDetailView from './UserPushTokenDetailView.vue';
import { useRouter } from 'vue-router';

interface UserPushTokenListViewProps {
  userId: string;
}

const props = defineProps<UserPushTokenListViewProps>();

const { t } = useI18n();
const queryClient = useQueryClient();
const { showConfirmDialog } = useConfirmDialog();
const { showSnackbar } = useGlobalSnackbar();
const router = useRouter();

const userIdRef = ref(props.userId);

const paginationOptions = ref({
  page: 1,
  itemsPerPage: 10,
  sortBy: [],
});
const filters = ref({});

const {
  state: { userPushTokens: fetchedUserPushTokens, isLoading: isLoadingUserPushTokens },
  actions: { refetch },
} = useUserPushTokensQuery(userIdRef);

const userPushTokens = computed<UserPushTokenDto[]>(() => {
  return fetchedUserPushTokens.value || [];
});

const totalItems = computed(() => userPushTokens.value.length); // Assuming no server-side pagination for now

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
  // In a real scenario with server-side pagination, you would update paginationOptions here
  // and trigger a refetch of useUserPushTokensQuery
  console.log('List options updated:', options);
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
      queryClient.invalidateQueries({ queryKey: ['user-push-tokens'] });
    },
    onError: (error) => {
      showSnackbar((error as Error).message || t('userPushToken.messages.deleteError'), 'error');
    },
  });
};

const handleUserPushTokenSaved = () => {
  closeAllDrawers();
  queryClient.invalidateQueries({ queryKey: ['user-push-tokens'] });
};

const handleUserPushTokenClosed = () => {
  closeAllDrawers();
};

watch(
  () => props.userId,
  (newUserId) => {
    userIdRef.value = newUserId;
    refetch();
  },
);
</script>

<style scoped></style>
