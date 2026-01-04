<template>
  <v-card class="pa-0" :elevation="0">
    <v-progress-linear v-if="isLoadingMember || isDeletingMember" indeterminate color="primary"></v-progress-linear>
    <v-card-text class="pa-0">
      <div v-if="isLoadingMember">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="memberError">
        <v-alert type="error" :text="memberError?.message || t('member.detail.notFound')"></v-alert>
      </div>
      <div v-else-if="member">
        <PrivacyAlert :is-private="member.isPrivate" />
        <MemberForm :initial-member-data="member" :family-id="member.familyId" :read-only="true" />

        <div v-if="member.biography" class="mt-4">
          <div class="d-flex justify-space-between align-center px-0">
            <span class="text-h6">{{ t('member.detail.biography') }}</span>
            <v-btn icon size="small" variant="text" @click="showBiography = !showBiography">
              <v-icon>{{ showBiography ? 'mdi-chevron-up' : 'mdi-chevron-down' }}</v-icon>
            </v-btn>
          </div>
          <v-expand-transition>
            <v-card-text v-show="showBiography" class="text-body-2 px-0 biography-content">
              {{ member?.biography }}
            </v-card-text>
          </v-expand-transition>
        </div>
      </div>
    </v-card-text>
    <v-card-actions class="justify-end pa-0">
      <v-btn color="grey" @click="handleClose" :disabled="isLoadingMember || isDeletingMember">{{ t('common.close')
        }}</v-btn>
      <v-btn color="primary" @click="handleEdit" :disabled="!member || isLoadingMember || isDeletingMember"
        v-if="canEditOrDelete">{{
          t('common.edit') }}</v-btn>
      <v-btn color="error" @click="handleDelete" :disabled="!member || isLoadingMember || isDeletingMember"
        v-if="canEditOrDelete">{{
          t('common.delete') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, computed, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { MemberForm } from '@/components/member';
import { useConfirmDialog, useAuth, useGlobalSnackbar } from '@/composables';
import { useMemberQuery, useDeleteMemberMutation } from '@/composables';
import PrivacyAlert from '@/components/common/PrivacyAlert.vue'; // Import PrivacyAlert

interface MemberDetailViewProps {
  memberId: string;
}

const props = defineProps<MemberDetailViewProps>();
const emit = defineEmits(['close', 'member-deleted', 'add-member-with-relationship', 'edit-member']);

const { t } = useI18n();
const { showConfirmDialog } = useConfirmDialog();
const { state } = useAuth();
const { showSnackbar } = useGlobalSnackbar();

const memberIdRef = toRef(props, 'memberId');
const { data: member, isLoading: isLoadingMember, error: memberError } = useMemberQuery(memberIdRef);
const { mutate: deleteMember, isPending: isDeletingMember } = useDeleteMemberMutation();

const showBiography = ref(false);

const canEditOrDelete = computed(() => {
  return state.isAdmin.value || state.isFamilyManager.value(member.value?.familyId || '');
});

const handleClose = () => {
  emit('close');
};

const handleEdit = () => {
  if (member.value) {
    emit('edit-member', member.value.id);
  }
};

const handleDelete = async () => {
  if (!member.value) return;

  const confirmed = await showConfirmDialog({
    title: t('confirmDelete.title'),
    message: t('member.list.confirmDelete', { fullName: member.value.fullName }),
  });

  if (confirmed) {
    deleteMember(member.value.id, {
      onSuccess: () => {
        showSnackbar(t('member.messages.deleteSuccess'), 'success');
        emit('member-deleted');
        emit('close');
      },
      onError: (error) => {
        showSnackbar(error.message || t('member.messages.deleteError'), 'error');
      },
    });
  }
};
</script>

<style scoped>
.biography-content {
  white-space: pre-wrap;
}
</style>