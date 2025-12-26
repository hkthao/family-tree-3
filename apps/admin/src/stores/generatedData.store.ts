import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { MemberDto, EventDto, FamilyLocation } from '@/types'; // Import FamilyLocation

export const useGeneratedDataStore = defineStore('generatedData', () => {
  const memberToAdd = ref<MemberDto | null>(null);
  const eventToAdd = ref<EventDto | null>(null);
  const locationToAdd = ref<FamilyLocation | null>(null); // NEW

  function setMemberToAdd(member: MemberDto) {
    memberToAdd.value = member;
  }

  function setEventToAdd(event: EventDto) {
    eventToAdd.value = event;
  }

  function setLocationToAdd(location: FamilyLocation) { // NEW
    locationToAdd.value = location;
  }

  function clearMemberToAdd() {
    memberToAdd.value = null;
  }

  function clearEventToAdd() {
    eventToAdd.value = null;
  }

  function clearLocationToAdd() { // NEW
    locationToAdd.value = null;
  }

  return {
    memberToAdd,
    eventToAdd,
    locationToAdd, // NEW
    setMemberToAdd,
    setEventToAdd,
    setLocationToAdd, // NEW
    clearMemberToAdd,
    clearEventToAdd,
    clearLocationToAdd, // NEW
  };
});
