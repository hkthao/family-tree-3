<template>
  <v-card :elevation="0" data-testid="user-push-token-detail-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('userPushToken.detail.title') }}</span>
    </v-card-title>
    <v-card-text>
      <div v-if="isLoading">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="error">
        <v-alert
          type="error"
          :text="error?.message || t('userPushToken.detail.errorLoading')"
        ></v-alert>
      </div>
      <div v-else-if="userPushToken">
        <UserPushTokenForm :initial-data="userPushToken" :read-only="true" />
      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="gray" @click="closeView" data-testid="button-close">
        {{ t('common.close') }}
      </v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { type PropType, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';
import UserPushTokenForm from '@/components/user-push-token/UserPushTokenForm.vue';
import { useUserPushTokenDetailQuery } from '@/composables/user-push-token/queries/useUserPushTokenDetailQuery';

const props = defineProps({
  userPushTokenId: {
    type: String as PropType<string>,
    required: true,
  },
});

const emit = defineEmits(['close']);

const { t } = useI18n();

const userPushTokenIdRef = ref(props.userPushTokenId);

const {
  state: { userPushToken, isLoading, error },
  actions: { refetch: refetchUserPushToken },
} = useUserPushTokenDetailQuery(userPushTokenIdRef);

watch(
  () => props.userPushTokenId,
  (newId) => {
    if (newId) {
      userPushTokenIdRef.value = newId;
      refetchUserPushToken();
    }
  },
);

const closeView = () => {
  emit('close');
};
</script>

<style scoped></style>
