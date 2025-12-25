import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { MemberDto, EventDto } from '@/types';

export const useGeneratedDataStore = defineStore('generatedData', () => {
  const memberToAdd = ref<MemberDto | null>(null);
  const eventToAdd = ref<EventDto | null>(null);

  function setMemberToAdd(member: MemberDto) {
    memberToAdd.value = member;
  }

  function setEventToAdd(event: EventDto) {
    eventToAdd.value = event;
  }

  function clearMemberToAdd() {
    memberToAdd.value = null;
  }

  function clearEventToAdd() {
    eventToAdd.value = null;
  }

  return {
    memberToAdd,
    eventToAdd,
    setMemberToAdd,
    setEventToAdd,
    clearMemberToAdd,
    clearEventToAdd,
  };
});
