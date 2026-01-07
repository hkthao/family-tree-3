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
        @click="toggleFollow()"
        :loading="familyFollowState.isLoading.value"
        data-testid="button-toggle-follow"
      >
        <span v-if="familyFollowState.isFollowing.value">{{ t('family.unfollow') }}</span>
        <span v-else>{{ t('family.follow') }}</span>
      </v-btn>
      <v-btn color="primary" @click="actions.openEditDrawer()" data-testid="button-edit" v-if="canManageFamily">
        {{ t('common.edit') }}
      </v-btn>
      <v-btn color="info" @click="emit('openUpdateFamilyLimitDrawer', familyData.id)" data-testid="button-update-limits" v-if="isAdmin">
        {{ t('family.updateLimits') }}
      </v-btn>
    </v-card-actions>
  </div>


</template>

<script setup lang="ts">
import { toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { FamilyForm } from '@/components/family';
import { useFamilyDetail } from '@/composables/family/logic/useFamilyDetail';
import { useFamilyFollow } from '@/composables/family/logic/useFamilyFollow'; // Import useFamilyFollow
import { useAuthStore } from '@/stores/auth.store'; // Import auth store
import PrivacyAlert from '@/components/common/PrivacyAlert.vue'; // Import PrivacyAlert
import { useGlobalSnackbar } from '@/composables/ui/useGlobalSnackbar'; // NEW import

const { t } = useI18n();
const authStore = useAuthStore();
const isAdmin = authStore.isAdmin; // Get admin status
const { showSnackbar } = useGlobalSnackbar(); // NEW: initialize global snackbar

const props = defineProps<{
  familyId: string;
  readOnly: boolean;
}>();

const emit = defineEmits(['openEditDrawer', 'openUpdateFamilyLimitDrawer']);

const { state: { familyData, isLoading, canManageFamily }, actions } = useFamilyDetail(props, emit);
const { state: familyFollowState, actions: familyFollowActions } = useFamilyFollow(toRef(props, 'familyId'));

const toggleFollow = async () => {
  if (familyFollowState.isFollowing.value) {
    await familyFollowActions.unfollowFamily();
  } else {
    await familyFollowActions.followFamily();
  }

  // NEW: Check for error after action and use global snackbar
  if (familyFollowState.error.value) {
    showSnackbar(familyFollowState.error.value.message || t('family.detail.errorLoading'), 'error');
  }
};
</script>