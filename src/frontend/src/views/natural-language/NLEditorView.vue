<template>
  <v-container>
    <NLEditorInput :loading="naturalLanguageStore.loading" @parse-content="parseContent" />

    <ParsedDataList
      :parsed-result="naturalLanguageStore.parsedData"
      @delete-member="handleDeleteMember"
      @delete-event="handleDeleteEvent"
      @save-member="handleSaveMember"
      @save-event="handleSaveEvent"
    />
  </v-container>
</template>

<script setup lang="ts">
import { onMounted } from 'vue';
import ParsedDataList from '@/components/natural-language-input/ParsedDataList.vue';
import NLEditorInput from '@/components/natural-language-input/NLEditorInput.vue';
import { useNaturalLanguageStore } from '@/stores/naturalLanguage.store';
import type { MemberDataDto, EventDataDto } from '@/types/natural-language.d';
import i18n from '@/plugins/i18n';

const props = defineProps<{
  familyId: string;
}>();

const naturalLanguageStore = useNaturalLanguageStore();

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

const handleSaveMember = async (member: MemberDataDto) => {
  if (!member.id) return; // Should not happen as ID is generated in initializeItemStates

  member.loading = true;
  member.saveAlert = { show: false, type: 'success', message: '' };

  // Collect all related member IDs
  const relatedMemberIds: string[] = [];
  if (member.fatherId) relatedMemberIds.push(member.fatherId);
  if (member.motherId) relatedMemberIds.push(member.motherId);
  if (member.husbandId) relatedMemberIds.push(member.husbandId);
  if (member.wifeId) relatedMemberIds.push(member.wifeId);

  // First, check if related members are saved in the frontend store
  if (relatedMemberIds.length > 0) {
    const unsavedRelatedMembers: string[] = [];
    relatedMemberIds.forEach(relatedId => {
      const relatedMember = naturalLanguageStore.parsedData?.members.find(m => m.id === relatedId);
      if (!relatedMember || !relatedMember.savedSuccessfully) {
        unsavedRelatedMembers.push(relatedMember?.fullName || relatedId);
      }
    });

    if (unsavedRelatedMembers.length > 0) {
      member.saveAlert = {
        show: true,
        type: 'error',
        message: i18n.global.t('naturalLanguage.errors.unsavedRelatedMembers', {
          members: unsavedRelatedMembers.join(', '),
        }),
      };
      member.loading = false;
      return; // Prevent saving if related members are not saved in frontend
    }

    // Second, check if related members exist in the backend database
    const existenceResult = await naturalLanguageStore.checkRelatedMembersExistence(relatedMemberIds);
    if (!existenceResult.ok) {
      member.saveAlert = {
        show: true,
        type: 'error',
        message: existenceResult.error?.message || i18n.global.t('common.saveError'),
      };
      member.loading = false;
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
      member.saveAlert = {
        show: true,
        type: 'error',
        message: i18n.global.t('naturalLanguage.errors.relatedMembersNotFoundInDb', {
          members: nonExistentRelatedMembers.join(', '),
        }),
      };
      member.loading = false;
      return; // Prevent saving if related members do not exist in DB
    }
  }

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

onMounted(() => {
  naturalLanguageStore.familyId = props.familyId;
});
</script>

