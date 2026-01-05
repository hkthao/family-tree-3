<template>
  <v-card :elevation="0">
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
          <v-tab value="relationships">{{ t('member.detail.tabs.relationships') }}</v-tab>

          <v-tab value="faces">{{ t('member.detail.tabs.faces') }}</v-tab>
          <v-tab value="events">{{ t('member.detail.tabs.events') }}</v-tab>
          <v-tab value="locations">{{ t('member.detail.tabs.locations') }}</v-tab>

        </v-tabs>

        <v-window v-model="tab">
          <v-window-item value="information">
            <MemberInformationTab
              :member-id="props.memberId"
              @member-deleted="handleMemberDeleted"
              @edit-member="handleEditMember"
            />
          </v-window-item>
          <v-window-item value="relationships" class="mt-3">
            <TreeChart
              v-if="familyId"
              :family-id="familyId"
              :initial-member-id="props.memberId"
              :read-only="true"
            />
          </v-window-item>
          <v-window-item value="faces">
            <MemberFacesTab :member-id="props.memberId" />
          </v-window-item>
          <v-window-item value="events">
            <MemberEventsTab :member-id="props.memberId" @show-event-detail="handleShowEventDetail" />
          </v-window-item>
          <v-window-item value="locations">
            <MemberLocationsTab :member-id="props.memberId" @show-location-detail="handleShowLocationDetail" />
          </v-window-item>

        </v-window>
      </div>
    </v-card-text>

  </v-card>
</template>

<script setup lang="ts">
import { ref, toRef, computed } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberQuery, useDeleteMemberMutation } from '@/composables';
import MemberInformationTab from './MemberInformationTab.vue'; // Corrected import path
import MemberFacesTab from './MemberFacesTab.vue';
import MemberLocationsTab from './MemberLocationsTab.vue'; // New import
import MemberEventsTab from './MemberEventsTab.vue'; // New import
import TreeChart from '@/components/family/TreeChart.vue'; // Import TreeChart

interface MemberDetailTabsViewProps {
  memberId: string;
  readOnly?: boolean; // Make readOnly optional
}

const props = withDefaults(defineProps<MemberDetailTabsViewProps>(), {
  readOnly: false, // Set a default value
});
const emit = defineEmits(['close', 'member-deleted', 'edit-member', 'show-event-detail', 'show-location-detail']); // Added show-location-detail

const { t } = useI18n();
const tab = ref('information');

const memberIdRef = toRef(props, 'memberId');
const { data: member, isLoading: isLoadingMember, error: memberError } = useMemberQuery(memberIdRef);
const { isPending: isDeletingMember } = useDeleteMemberMutation(); // Only need isPending from here

const familyId = computed(() => member.value?.familyId || null);

const handleMemberDeleted = () => {
  emit('member-deleted');
  emit('close');
};

const handleEditMember = (memberId: string) => {
  emit('edit-member', memberId);
};

const handleShowEventDetail = (eventId: string) => {
  emit('show-event-detail', eventId);
};

const handleShowLocationDetail = (locationId: string) => {
  // TODO: Implement logic to show location detail, e.g., open a dialog or navigate
  console.log('Show location detail for:', locationId);
};

</script>

<style scoped>
/* Add any specific styles here */
</style>