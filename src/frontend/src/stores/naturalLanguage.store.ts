import { defineStore } from 'pinia';
import type { AnalyzedDataDto, MemberDataDto, EventDataDto } from '@/types/natural-language.d'; // Update import
import type { ApiError } from '@/plugins/axios';
import i18n from '@/plugins/i18n';
import { v4 as uuidv4 } from 'uuid'; // Import uuid for sessionId
import type { Member, Event, Gender, EventType, Result } from '@/types'; // Import Member, Event, Gender, EventType, Result
import { useMemberStore } from './member.store'; // Import member store
import { useEventStore } from './event.store'; // Import event store

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

    async saveMember(memberData: MemberDataDto): Promise<Result<Member, ApiError>> { // Re-added function declaration
      this.loading = true;
      this.error = null;
      try {
        if (!this.familyId) {
          this.error = i18n.global.t('naturalLanguage.errors.familyIdMissing');
          return { ok: false, error: { message: this.error } } as Result<Member, ApiError>;
        }

        const memberStore = useMemberStore(); // Access member store

        const newMember: Omit<Member, 'id'> = {
          firstName: memberData.fullName.split(' ').slice(0, -1).join(' ') || memberData.fullName,
          lastName: memberData.fullName.split(' ').pop() || '',
          familyId: this.familyId,
          gender: memberData.gender as Gender,
          dateOfBirth: memberData.dateOfBirth ? new Date(memberData.dateOfBirth) : undefined,
          dateOfDeath: memberData.dateOfDeath ? new Date(memberData.dateOfDeath) : undefined,
        };

        const result = await memberStore.addItem(newMember); // Use memberStore.addItem
        if (!result.ok) {
          this.error = result.error?.message || i18n.global.t('aiInput.saveError'); // Use i18n for error
        }
        return result; // Explicitly return the result
      } catch (e: any) {
        this.error = e.message;
        return { ok: false, error: { message: this.error } } as Result<Member, ApiError>; // Return a failure result
      } finally {
        this.loading = false;
      }
    },

    async saveEvent(eventData: EventDataDto): Promise<Result<Event, ApiError>> { // Re-added function declaration
      this.loading = true;
      this.error = null;
      try {
        if (!this.familyId) {
          this.error = i18n.global.t('naturalLanguage.errors.familyIdMissing');
          return { ok: false, error: { message: this.error } } as Result<Event, ApiError>;
        }

        const eventStore = useEventStore(); // Access event store

        const newEvent: Omit<Event, 'id'> = {
          name: eventData.description, // Using description as name
          description: eventData.description,
          startDate: eventData.date ? new Date(eventData.date) : null,
          location: eventData.location || undefined,
          familyId: this.familyId,
          type: eventData.type as unknown as EventType, // Cast as EventType, assuming string matches enum
          relatedMembers: eventData.relatedMemberIds,
        };

        const result = await eventStore.addItem(newEvent); // Use eventStore.addItem
        if (!result.ok) {
          this.error = result.error?.message || i18n.global.t('aiInput.saveError'); // Use i18n for error
        }
        return result; // Explicitly return the result
      } catch (e: any) {
        this.error = e.message;
        return { ok: false, error: { message: this.error } } as Result<Event, ApiError>; // Return a failure result
      } finally {
        this.loading = false;
      }
    },
  },
});
