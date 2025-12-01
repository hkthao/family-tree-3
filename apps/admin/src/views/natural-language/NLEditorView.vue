<template>
  <v-container>
    <NLEditorInput :loading="naturalLanguageStore.loading" @parse-content="parseContent" />

    <ParsedDataList :parsed-result="naturalLanguageStore.parsedData" @delete-member="handleDeleteMember"
      @delete-event="handleDeleteEvent" @delete-relationship="handleDeleteRelationship" @save-member="handleSaveMember"
      @save-event="handleSaveEvent" @save-relationship="handleSaveRelationship" @clear-all="handleClearAllParsedData" />
  </v-container>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import ParsedDataList from '@/components/natural-language-input/ParsedDataList.vue';
import NLEditorInput from '@/components/natural-language-input/NLEditorInput.vue';
import { useNaturalLanguageStore } from '@/stores/naturalLanguage.store';
import type { MemberDataDto, EventDataDto, RelationshipDataDto } from '@/types'; // Add RelationshipDataDto
import i18n from '@/plugins/i18n';

const props = defineProps<{
  familyId: string;
}>();

const naturalLanguageStore: ReturnType<typeof useNaturalLanguageStore> = useNaturalLanguageStore();

const parseContent = async (content: string | null) => {
  if (!content) {
    naturalLanguageStore.parsedData = null;
    return;
  }

  naturalLanguageStore.familyId = props.familyId;
  naturalLanguageStore.setInput(content);
  await naturalLanguageStore.analyzeContent();
};

const handleDeleteMember = (index: number) => {
  naturalLanguageStore.deleteParsedMember(index);
};

const handleDeleteEvent = (index: number) => {
  naturalLanguageStore.deleteParsedEvent(index);
};

const handleDeleteRelationship = (index: number) => {
  naturalLanguageStore.deleteParsedRelationship(index);
};

const handleClearAllParsedData = () => {
  naturalLanguageStore.parsedData = null;
};

const handleSaveMember = async (member: MemberDataDto) => {
  if (!member.id) return; // Should not happen as ID is generated in initializeItemStates

  member.loading = true;
  member.saveAlert = { show: false, type: 'success', message: '' };

  try {
    const saveResult = await naturalLanguageStore.saveMember(member);

    if (saveResult && saveResult.ok) {
      member.saveAlert = {
        show: true,
        type: 'success',
        message: i18n.global.t('common.saveSuccess'),
      };
      member.savedSuccessfully = true;
    } else {
      member.saveAlert = {
        show: true,
        type: 'error',
        message: saveResult?.error?.message || i18n.global.t('common.saveError'),
      };
      member.savedSuccessfully = false;
    }
  } catch (e: any) {
    member.saveAlert = {
      show: true,
      type: 'error',
      message: e.message || i18n.global.t('common.saveError'),
    };
    member.savedSuccessfully = false;
  } finally {
    member.loading = false;
  }
};

const handleSaveEvent = async (event: EventDataDto) => {
  if (!event.id) return; // Should not happen as ID is generated in initializeItemStates

  event.loading = true;
  event.saveAlert = { show: false, type: 'success', message: '' };

  try {
    const saveResult = await naturalLanguageStore.saveEvent(event);

    if (saveResult && saveResult.ok) {
      event.saveAlert = {
        show: true,
        type: 'success',
        message: i18n.global.t('common.saveSuccess'),
      };
      event.savedSuccessfully = true;
    } else {
      event.saveAlert = {
        show: true,
        type: 'error',
        message: saveResult?.error?.message || i18n.global.t('common.saveError'),
      };
      event.savedSuccessfully = false;
    }
  } catch (e: any) {
    event.saveAlert = {
      show: true,
      type: 'error',
      message: e.message || i18n.global.t('common.saveError'),
    };
    event.savedSuccessfully = false;
  } finally {
    event.loading = false;
  }
};

const handleSaveRelationship = async (relationship: RelationshipDataDto) => {
  if (!relationship.id) return; // Should not happen as ID is generated in initializeItemStates

  relationship.loading = true;
  relationship.saveAlert = { show: false, type: 'success', message: '' };

  // Check if related members are saved in the frontend store
  const unsavedRelatedMembers: string[] = [];
  const sourceMember = naturalLanguageStore.parsedData?.members.find(m => m.id === relationship.sourceMemberId);
  const targetMember = naturalLanguageStore.parsedData?.members.find(m => m.id === relationship.targetMemberId);

  if (!sourceMember || !sourceMember.savedSuccessfully) {
    unsavedRelatedMembers.push(sourceMember?.fullName || relationship.sourceMemberId);
  }
  if (!targetMember || !targetMember.savedSuccessfully) {
    unsavedRelatedMembers.push(targetMember?.fullName || relationship.targetMemberId);
  }

  if (unsavedRelatedMembers.length > 0) {
    relationship.saveAlert = {
      show: true,
      type: 'error',
      message: i18n.global.t('naturalLanguage.errors.unsavedRelatedMembers', {
        members: unsavedRelatedMembers.join(', '),
      }),
    };
    relationship.loading = false;
    return;
  }

  // Check if related members exist in the backend database
  const relatedMemberIds = [relationship.sourceMemberId, relationship.targetMemberId];
  const existenceResult = await naturalLanguageStore.checkRelatedMembersExistence(relatedMemberIds);
  if (!existenceResult.ok) {
    relationship.saveAlert = {
      show: true,
      type: 'error',
      message: existenceResult.error?.message || i18n.global.t('common.saveError'),
    };
    relationship.loading = false;
    return;
  }

  const nonExistentRelatedMembers: string[] = [];
  relatedMemberIds.forEach(relatedId => {
    if (!existenceResult.value.get(relatedId)) {
      const relatedMember = naturalLanguageStore.parsedData?.members.find(m => m.id === relatedId);
      nonExistentRelatedMembers.push(relatedMember?.fullName || relatedId);
    }
  });

  if (nonExistentRelatedMembers.length > 0) {
    relationship.saveAlert = {
      show: true,
      type: 'error',
      message: i18n.global.t('naturalLanguage.errors.relatedMembersNotFoundInDb', {
        members: nonExistentRelatedMembers.join(', '),
      }),
    };
    relationship.loading = false;
    return;
  }

  try {
    const saveResult = await naturalLanguageStore.saveRelationship(relationship);

    if (saveResult && saveResult.ok) {
      relationship.saveAlert = {
        show: true,
        type: 'success',
        message: i18n.global.t('common.saveSuccess'),
      };
      relationship.savedSuccessfully = true;
    } else {
      relationship.saveAlert = {
        show: true,
        type: 'error',
        message: saveResult?.error?.message || i18n.global.t('common.saveError'),
      };
      relationship.savedSuccessfully = false;
    }
  } catch (e: any) {
    relationship.saveAlert = {
      show: true,
      type: 'error',
      message: e.message || i18n.global.t('common.saveError'),
    };
    relationship.savedSuccessfully = false;
  } finally {
    relationship.loading = false;
  }
};

onMounted(() => {
  naturalLanguageStore.familyId = props.familyId;
});
</script>
