<template>
  <v-card :elevation="0">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('member.detail.title') }}</span>
    </v-card-title>
    <v-progress-linear v-if="isLoadingMember || isDeletingMember" indeterminate color="primary"></v-progress-linear>
    <v-card-text>
      <div v-if="isLoadingMember">
        <v-progress-circular indeterminate color="primary"></v-progress-circular>
        {{ t('common.loading') }}
      </div>
      <div v-else-if="memberError">
        <v-alert type="error" :text="memberError?.message || t('member.detail.notFound')"></v-alert>
      </div>
      <div v-else-if="member">
        <v-tabs v-model="tab" color="primary" align-tabs="center">
          <v-tab value="information">{{ t('member.detail.tabs.information') }}</v-tab>
          <v-tab value="family-tree">{{ t('member.detail.tabs.familyTree') }}</v-tab>
          <v-tab value="faces">{{ t('member.detail.tabs.faces') }}</v-tab>
          <v-tab value="events">{{ t('member.detail.tabs.events') }}</v-tab>
          <v-tab value="locations">{{ t('member.detail.tabs.locations') }}</v-tab>
          <v-tab value="voice">{{ t('member.detail.tabs.voice') }}</v-tab>
        </v-tabs>

        <v-window v-model="tab">
          <v-window-item value="information">
            <MemberDetailView
              :member-id="props.memberId"
              @member-deleted="handleMemberDeleted"
              @edit-member="handleEditMember"
            />
          </v-window-item>
          <v-window-item value="family-tree">
            <!-- Content for Family Tree tab -->
            Family Tree Tab Content
          </v-window-item>
          <v-window-item value="faces">
            <MemberFacesTab :member-id="props.memberId" />
          </v-window-item>
          <v-window-item value="events">
            <!-- Content for Events tab -->
            Events Tab Content
          </v-window-item>
          <v-window-item value="locations">
            <!-- Content for Locations tab -->
            Locations Tab Content
          </v-window-item>
          <v-window-item value="voice">
            <!-- Content for Voice tab -->
            Voice Tab Content
          </v-window-item>
        </v-window>
      </div>
    </v-card-text>
    <v-card-actions class="justify-end">
      <v-btn color="grey" @click="emit('close')">{{ t('common.close') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, toRef } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberQuery, useDeleteMemberMutation } from '@/composables';
import MemberDetailView from './MemberDetailView.vue'; // Corrected import path
import MemberFacesTab from './MemberFacesTab.vue'; // New import

interface MemberDetailTabsViewProps {
  memberId: string;
}

const props = defineProps<MemberDetailTabsViewProps>();
const emit = defineEmits(['close', 'member-deleted', 'edit-member']);

const { t } = useI18n();
const tab = ref('information');

const memberIdRef = toRef(props, 'memberId');
const { data: member, isLoading: isLoadingMember, error: memberError } = useMemberQuery(memberIdRef);
const { isPending: isDeletingMember } = useDeleteMemberMutation(); // Only need isPending from here

const handleMemberDeleted = () => {
  emit('member-deleted');
  emit('close');
};

const handleEditMember = (memberId: string) => {
  emit('edit-member', memberId);
};
</script>

<style scoped>
/* Add any specific styles here */
</style>