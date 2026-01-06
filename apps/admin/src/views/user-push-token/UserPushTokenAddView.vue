<template>
  <v-card :elevation="0" data-testid="user-push-token-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{
        t('userPushToken.form.addTitle')
      }}</span>
    </v-card-title>
    <v-card-text>
      <UserPushTokenForm ref="userPushTokenFormRef" @cancel="closeForm" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm" :disabled="isAddingUserPushToken">{{
        t('common.cancel')
      }}</v-btn>

      <v-btn color="primary" data-testid="button-save" @click="handleAddItem" :loading="isAddingUserPushToken"
        :disabled="isAddingUserPushToken">{{
          t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, type Ref } from 'vue';
import { useI18n } from 'vue-i18n';
import UserPushTokenForm, {
  type IUserPushTokenFormInstance,
} from '@/components/user-push-token/UserPushTokenForm.vue';
import { useCreateUserPushTokenMutation } from '@/composables/user-push-token/mutations/useCreateUserPushTokenMutation';
import { useGlobalSnackbar } from '@/composables';
import type { ApiError } from '@/types/apiError';

const userPushTokenFormRef: Ref<IUserPushTokenFormInstance | null> = ref(null);
const emit = defineEmits(['close', 'saved']);

const { t } = useI18n();
const { showSnackbar } = useGlobalSnackbar();

const {
  mutateAsync: createUserPushToken,
  isPending,
} = useCreateUserPushTokenMutation();
const isAddingUserPushToken = isPending;

const handleAddItem = async () => {
  if (userPushTokenFormRef.value) {
    const isValid = await userPushTokenFormRef.value.validate();
    if (isValid) {
      const formData = userPushTokenFormRef.value.getFormData();
      try {
        await createUserPushToken(formData);
        showSnackbar(t('userPushToken.messages.createSuccess'), 'success');
        emit('saved');
        emit('close');
      } catch (error) {
        showSnackbar((error as ApiError).message || t('userPushToken.messages.createError'), 'error');
      }
    }
  }
};

const closeForm = () => {
  emit('close');
};
</script>

<style scoped></style>
