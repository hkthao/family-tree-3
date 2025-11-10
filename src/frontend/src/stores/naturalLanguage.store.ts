import { defineStore } from 'pinia';
import type { AnalyzedDataDto, MemberDataDto, EventDataDto } from '@/types/natural-language.d'; // Update import
import type { ApiError } from '@/plugins/axios';
import i18n from '@/plugins/i18n';
import { v4 as uuidv4 } from 'uuid'; // Import uuid for sessionId
import type { Member, Event, Gender, EventType } from '@/types'; // Import Member, Event, Gender, EventType

export const useNaturalLanguageStore = defineStore('naturalLanguage', {
  state: () => ({
    input: '' as string,
    parsedData: null as AnalyzedDataDto | null, // Update type
    loading: false as boolean,
    error: null as string | null,
    familyId: null as string | null, // Rename currentFamilyId to familyId
  }),

  actions: {
    async analyzeContent(): Promise<boolean> { // Rename action to analyzeContent
      this.loading = true;
      this.error = null;
      this.parsedData = null;

      if (!this.input.trim()) {
        this.error = i18n.global.t('naturalLanguage.errors.emptyInput');
        this.loading = false;
        return false;
      }

      const sessionId = uuidv4(); // Generate sessionId here

      const result = await this.services.naturalLanguage.analyzeContent(this.input, sessionId); // Use new service

      if (result.ok) {
        this.parsedData = result.value; // Directly assign the object
        this.loading = false;
        return true;
      } else {
        this.error =
          result.error?.message || i18n.global.t('naturalLanguage.errors.parseFailed');
        this.loading = false;
        return false;
      }
    },

    clearState() {
      this.input = '';
      this.parsedData = null;
      this.error = null;
      this.loading = false;
    },

    setInput(newInput: string) {
      this.input = newInput;
    },

    async saveMember(memberData: MemberDataDto) {
      this.loading = true;
      this.error = null;
      try {
        if (!this.familyId) { // Use this.familyId
          this.error = i18n.global.t('naturalLanguage.errors.familyIdMissing');
          return;
        }

        const newMember: Omit<Member, 'id'> = {
          firstName: memberData.fullName.split(' ').slice(0, -1).join(' ') || memberData.fullName,
          lastName: memberData.fullName.split(' ').pop() || '',
          familyId: this.familyId, // Use this.familyId
          gender: memberData.gender as Gender,
          dateOfBirth: memberData.dateOfBirth ? new Date(memberData.dateOfBirth) : undefined,
          dateOfDeath: memberData.dateOfDeath ? new Date(memberData.dateOfDeath) : undefined,
        };

        const result = await this.services.member.add(newMember);
        if (result.ok) {
          // Optionally, handle success (e.g., show notification)
        } else {
          this.error = result.error?.message || 'Failed to save member';
        }
      } catch (e: any) {
        this.error = e.message;
      } finally {
        this.loading = false;
      }
    },

    async saveEvent(eventData: EventDataDto) {
      this.loading = true;
      this.error = null;
      try {
        if (!this.familyId) { // Use this.familyId
          this.error = i18n.global.t('naturalLanguage.errors.familyIdMissing');
          return;
        }

        const newEvent: Omit<Event, 'id'> = {
          name: eventData.description, // Using description as name
          description: eventData.description,
          startDate: eventData.date ? new Date(eventData.date) : null,
          location: eventData.location || undefined,
          familyId: this.familyId, // Use this.familyId
          type: eventData.type as unknown as EventType, // Cast as EventType, assuming string matches enum
          relatedMembers: eventData.relatedMemberIds,
        };

        const result = await this.services.event.add(newEvent);
        if (result.ok) {
          // Optionally, handle success
        } else {
          this.error = result.error?.message || 'Failed to save event';
        }
      } catch (e: any) {
        this.error = e.message;
      } finally {
        this.loading = false;
      }
    },
  },
});
