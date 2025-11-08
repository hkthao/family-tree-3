<template>
  <v-card :elevation="0" data-testid="member-add-view">
    <v-card-title class="text-center">
      <span class="text-h5 text-uppercase">{{ t('member.form.addTitle') }}</span>
    </v-card-title>
    <v-card-text>
      <v-alert v-if="initialRelationshipData && targetMember" type="info" class="mb-4">
        {{ getRelationshipMessage() }}
      </v-alert>
      <MemberForm ref="memberFormRef" @close="closeForm" :family-id="props.familyId" />
    </v-card-text>
    <v-card-actions>
      <v-spacer></v-spacer>
      <v-btn color="grey" data-testid="button-cancel" @click="closeForm">{{ t('common.cancel') }}</v-btn>
      <v-btn color="primary" @click="handleAddMember" data-testid="save-member-button">{{ t('common.save') }}</v-btn>
    </v-card-actions>
  </v-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useMemberStore } from '@/stores/member.store';
import { useNotificationStore } from '@/stores/notification.store';
import { useRelationshipStore } from '@/stores/relationship.store'; // Import relationship store
import { MemberForm } from '@/components/member';
import type { Member } from '@/types';
import { RelationshipType } from '@/types'; // Import RelationshipType

interface MemberAddViewProps {
  familyId: string | null;
  initialRelationshipData?: {
    targetMemberId?: string;
    sourceMemberId?: string;
    relationshipType: RelationshipType;
    familyId: string;
  };
}

const props = defineProps<MemberAddViewProps>();
const emit = defineEmits(['close', 'saved']);

const memberFormRef = ref<InstanceType<typeof MemberForm> | null>(null);

const { t } = useI18n();
const memberStore = useMemberStore();
const notificationStore = useNotificationStore();
const relationshipStore = useRelationshipStore(); // Initialize relationship store

const targetMember = ref<Member | undefined>(undefined);

onMounted(async () => {
  if (props.initialRelationshipData && (props.initialRelationshipData.targetMemberId || props.initialRelationshipData.sourceMemberId)) {
    const memberIdToFetch = props.initialRelationshipData.targetMemberId || props.initialRelationshipData.sourceMemberId;
    if (memberIdToFetch) {
      await memberStore.getById(memberIdToFetch);
      if (memberStore.currentItem)
        targetMember.value = memberStore.currentItem;
    }
  }
});

const getRelationshipMessage = () => {
  if (!targetMember.value || !props.initialRelationshipData) return '';

  const memberName = targetMember.value.fullName;
  switch (props.initialRelationshipData.relationshipType) {
    case RelationshipType.Father:
      return t('member.messages.addingFatherTo', { memberName });
    case RelationshipType.Mother:
      return t('member.messages.addingMotherTo', { memberName });
    case RelationshipType.Child:
      return t('member.messages.addingChildTo', { memberName });
    default:
      return '';
  }
};

const handleAddMember = async () => {
  if (!memberFormRef.value) return;
  const isValid = await memberFormRef.value.validate();
  if (!isValid) return;

  const memberData = memberFormRef.value.getFormData();
  if (props.familyId) {
    memberData.familyId = props.familyId;
  }

  try {
    await memberStore.addItem(memberData as Omit<Member, 'id'>);
    if (!memberStore.error && memberStore.currentItem) {
      notificationStore.showSnackbar(t('member.messages.addSuccess'), 'success');

      // If initialRelationshipData is present, create the relationship
      if (props.initialRelationshipData) {
        const newMemberId = memberStore.currentItem.id;
        let sourceMemberId = '';
        let targetMemberId = '';
        const relationshipType = props.initialRelationshipData.relationshipType;

        if (relationshipType === RelationshipType.Father || relationshipType === RelationshipType.Mother) {
          sourceMemberId = newMemberId;
          targetMemberId = props.initialRelationshipData.targetMemberId!;
        } else if (relationshipType === RelationshipType.Child) {
          sourceMemberId = props.initialRelationshipData.sourceMemberId!;
          targetMemberId = newMemberId;
        }

        await relationshipStore.addItem({
          sourceMemberId,
          targetMemberId,
          type: relationshipType,
          familyId: props.familyId!,
        });

        if (relationshipStore.error) {
          notificationStore.showSnackbar(relationshipStore.error || t('relationship.messages.saveError'), 'error');
        } else {
          notificationStore.showSnackbar(t('relationship.messages.addSuccess'), 'success');
        }
      }
      emit('saved'); // Emit saved event
    } else {
      notificationStore.showSnackbar(memberStore.error || t('member.messages.saveError'), 'error');
    }
  } catch (error) {
    notificationStore.showSnackbar(t('member.messages.saveError'), 'error');
  }
};

const closeForm = () => {
  emit('close'); // Emit close event
};
</script>