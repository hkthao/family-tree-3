import { defineStore } from 'pinia';
import type { AnalyzedDataDto, MemberDataDto, EventDataDto, RelationshipDataDto } from '@/types/natural-language.d'; // Update import
import type { ApiError } from '@/plugins/axios';
import i18n from '@/plugins/i18n';
import { v4 as uuidv4 } from 'uuid'; // Import uuid for sessionId
import { type Member, type Event, type Gender, EventType, type Result, type RelationshipType } from '@/types'; // Import Member, Event, Gender, EventType, Result, RelationshipType
import { useMemberStore } from './member.store'; // Import member store
import { useEventStore } from './event.store'; // Import event store
import { useRelationshipStore } from './relationship.store'; // Import relationship store

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

        if (this.parsedData) {
          this.parsedData.members.forEach(member => {
            if (!member.id) {
              member.id = uuidv4(); // Generate ID if missing
            }
            member.loading = false;
            member.savedSuccessfully = false;
            member.saveAlert = { show: false, type: 'success', message: '' };
          });
          this.parsedData.events.forEach(event => {
            if (!event.id) {
              event.id = uuidv4(); // Generate ID if missing
            }
            event.loading = false;
            event.savedSuccessfully = false;
            event.saveAlert = { show: false, type: 'success', message: '' };
          });
          this.parsedData.relationships.forEach(relationship => { // Initialize relationships
            if (!relationship.id) {
              relationship.id = uuidv4(); // Generate ID if missing
            }
            relationship.loading = false;
            relationship.savedSuccessfully = false;
            relationship.saveAlert = { show: false, type: 'success', message: '' };
          });
        }

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

    setInput(newInput: string): void {
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

        const newMember: Member = {
          id: memberData.id!,
          firstName: memberData.firstName ?? "",
          lastName: memberData.lastName ?? "",
          familyId: this.familyId,
          gender: memberData.gender as Gender,
          dateOfBirth: memberData.dateOfBirth ? new Date(memberData.dateOfBirth) : undefined,
          dateOfDeath: memberData.dateOfDeath ? new Date(memberData.dateOfDeath) : undefined,
        };
        const result: Result<Member, ApiError> = await memberStore.addItem(newMember); // Call addItem

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
          type: eventData.type,
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

    async saveRelationship(relationshipData: RelationshipDataDto): Promise<Result<any, ApiError>> {
      this.loading = true;
      this.error = null;
      try {
        if (!this.familyId) {
          this.error = i18n.global.t('naturalLanguage.errors.familyIdMissing');
          return { ok: false, error: { message: this.error } } as Result<any, ApiError>;
        }

        const relationshipStore = useRelationshipStore();

        const newRelationship = {
          sourceMemberId: relationshipData.sourceMemberId,
          targetMemberId: relationshipData.targetMemberId,
          type: relationshipData.type,
          order: relationshipData.order,
          familyId: this.familyId,
        };

        const result = await relationshipStore.addItem(newRelationship);
        if (!result.ok) {
          this.error = result.error?.message || i18n.global.t('aiInput.saveError');
        }
        return result;
      } catch (e: any) {
        this.error = e.message;
        return { ok: false, error: { message: this.error } } as Result<any, ApiError>;
      } finally {
        this.loading = false;
      }
    },

    deleteParsedMember(index: number) {
      if (this.parsedData && this.parsedData.members) {
        this.parsedData.members.splice(index, 1);
      }
    },

    deleteParsedEvent(index: number) {
      if (this.parsedData && this.parsedData.events) {
        this.parsedData.events.splice(index, 1);
      }
    },

    deleteParsedRelationship(index: number) {
      if (this.parsedData && this.parsedData.relationships) {
        this.parsedData.relationships.splice(index, 1);
      }
    },

    async checkRelatedMembersExistence(memberIds: string[]): Promise<Result<Map<string, boolean>, ApiError>> {
      const existenceMap = new Map<string, boolean>();
      if (memberIds.length === 0) {
        return { ok: true, value: existenceMap };
      }

      const result = await this.services.member.getByIds(memberIds);

      if (result.ok) {
        const existingMemberIds = new Set(result.value.map(m => m.id));
        memberIds.forEach(id => {
          existenceMap.set(id, existingMemberIds.has(id));
        });
        return { ok: true, value: existenceMap };
      } else {
        return { ok: false, error: result.error };
      }
    },
  },
});