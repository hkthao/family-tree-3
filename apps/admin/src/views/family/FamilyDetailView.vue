<template>
  <div v-if="isLoading || familyFollowState.isLoading.value">
    <v-progress-linear indeterminate color="primary"></v-progress-linear>
  </div>

  <div v-if="familyData">
    <PrivacyAlert v-if="familyData" :is-private="familyData.isPrivate" />
    <FamilyForm :data="familyData" :read-only="props.readOnly" :title="t('family.detail.title')" :display-limit-config="true" />
    <v-card-actions class="justify-end pa-0">
      <v-btn color="gray" @click="actions.closeView" data-testid="button-close">
        {{ t('common.close') }}
      </v-btn>
      <v-btn
        :color="familyFollowState.isFollowing.value ? 'warning' : 'success'"
        @click="emitToggleFollowSettings()"
        :loading="familyFollowState.isLoading.value"
        data-testid="button-toggle-follow"
      >
        <span v-if="familyFollowState.isFollowing.value">{{ t('family.unfollow') }}</span>
        <span v-else>{{ t('family.follow') }}</span>
      </v-btn>
      <v-btn
        v-if="familyFollowState.isFollowing.value"
        color="info"
        @click="emitToggleFollowSettings()"
        :loading="familyFollowState.isLoading.value"
        data-testid="button-notification-settings"
        class="ml-2"
      >
        {{ t('family.followSettings.title') }}
      </v-btn>
      <v-btn color="primary" @click="actions.openEditDrawer()" data-testid="button-edit" v-if="canManageFamily">
        {{ t('common.edit') }}
      </v-btn>
      <v-btn color="info" @click="emit('openUpdateFamilyLimitDrawer', familyData.id)" data-testid="button-update-limits" v-if="isAdmin">
        {{ t('family.updateLimits') }}
      </v-btn>
      <!-- New button for admin to generate KB -->
      <v-btn
        v-if="isAdmin"
        color="success"
        :loading="generateKbMutation.isPending.value"
        @click="triggerGenerateKb"
        data-testid="button-generate-kb"
        class="ml-2"
      >
        {{ t('family.generateKb.button') }}
      </v-btn>
    </v-card-actions>
  </div>

  <v-snackbar
    v-model="snackbar.show"
    :color="snackbar.color"
    :timeout="snackbar.timeout"
    location="bottom right"
  >
    {{ snackbar.message }}
  </v-snackbar>
</template>

<script setup lang="ts">
import { toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyForm } from '@/components/family';
import { useFamilyDetail } from '@/composables/family/logic/useFamilyDetail';
import { useFamilyFollow } from '@/composables/family/logic/useFamilyFollow';
import { useAuthStore } from '@/stores/auth.store';
import PrivacyAlert from '@/components/common/PrivacyAlert.vue';
import { useGenerateFamilyKb } from '@/composables/family/logic/useGenerateFamilyKb'; // Import the new composable

const { t } = useI18n();
const authStore = useAuthStore();
const isAdmin = authStore.isAdmin;

const props = defineProps<{
  familyId: string;
  readOnly: boolean;
}>();

const emit = defineEmits(['openEditDrawer', 'openUpdateFamilyLimitDrawer', 'toggle-follow-settings']);

const { state: { familyData, isLoading, canManageFamily }, actions } = useFamilyDetail(props, emit);
const { state: familyFollowState } = useFamilyFollow(toRef(props, 'familyId'));

// Use the new composable for generate KB logic
const { generateKbMutation, triggerGenerateKb, snackbar } = useGenerateFamilyKb(props.familyId);

const emitToggleFollowSettings = () => {
  emit('toggle-follow-settings', {
    familyId: props.familyId,
    isFollowing: familyFollowState.isFollowing.value,
  });
};
</script>