<template>
  <v-card :elevation="0" data-testid="user-push-token-edit-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('userPushToken.form.editTitle')
      }}</span>
    </v-card-title>
    <v-card-text>
      <div v-if="isLoadingUserPushToken">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <UserPushTokenForm
        v-else-if="userPushToken"
        ref="userPushTokenFormRef"
        :initial-data="userPushToken"
        :is-editing="true"
      />
      <v-alert v-else type="error">{{ t('userPushToken.messages.notFound') }}</v-alert>
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn
        color="grey"
        data-testid="button-cancel"
        @click="closeForm"
        :disabled="isLoadingUserPushToken || isUpdatingUserPushToken"
      >{{ t('common.cancel') }}</v-btn>

      <v-btn
        color="primary"
        data-testid="button-save"
        @click="handleUpdateItem"
        :loading="isUpdatingUserPushToken"
        :disabled="isLoadingUserPushToken || isUpdatingUserPushToken"
      >{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, type PropType, type Ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import UserPushTokenForm, {
  type IUserPushTokenFormInstance,
} from '@/components/user-push-token/UserPushTokenForm.vue';
import { useUserPushTokenDetailQuery } from '@/composables/user-push-token/queries/useUserPushTokenDetailQuery';
import { useUpdateUserPushTokenMutation } from '@/composables/user-push-token/mutations/useUpdateUserPushTokenMutation';
import { useGlobalSnackbar } from '@/composables';
import type { ApiError } from '@/types/apiError';

const props = defineProps({
  userPushTokenId: {
    type: String as PropType<string>,
    required: true,
  },
});

const emit = defineEmits(['close', 'saved']);

const userPushTokenFormRef: Ref<IUserPushTokenFormInstance | null> = ref(null);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const userPushTokenIdRef = ref(props.userPushTokenId);

const {
  state: { userPushToken, isLoading: isLoadingUserPushToken },
  actions: { refetch: refetchUserPushToken },
} = useUserPushTokenDetailQuery(userPushTokenIdRef);

const {
  mutateAsync: updateUserPushToken,
  isPending,
} = useUpdateUserPushTokenMutation();
const isUpdatingUserPushToken = isPending;

watch(
  () => props.userPushTokenId,
  (newId) => {
    if (newId) {
      userPushTokenIdRef.value = newId;
      refetchUserPushToken();
    }
  },
);

const handleUpdateItem = async () => {
  if (userPushTokenFormRef.value && userPushToken.value) {
    const isValid = await userPushTokenFormRef.value.validate();
    if (isValid) {
      const formData = userPushTokenFormRef.value.getFormData();
      try {
        await updateUserPushToken({ id: userPushToken.value?.id as string, ...formData });
        showSnackbar(t('userPushToken.messages.updateSuccess'), 'success');
        emit('saved');
        emit('close');
      } catch (error) {
        showSnackbar((error as ApiError).message || t('userPushToken.messages.updateError'), 'error');
      }
    }
  }
};

const closeForm = () => {
  emit('close');
};
</script>

<style scoped></style>
