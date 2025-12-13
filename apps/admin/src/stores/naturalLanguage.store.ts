import { defineStore } from 'pinia';
import type { AnalyzedDataDto, MemberDataDto, EventDataDto, RelationshipDataDto, ApiError } from '@/types'; // Update type import to global
import i18n from '@/plugins/i18n';
import { v4 as uuidv4 } from 'uuid'; // Import uuid for sessionId
import { type Member, type Event, type Gender, type Result } from '@/types'; // Import Member, Event, Gender, EventType, Result, RelationshipType
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

      if (!this.familyId) {
        this.error = i18n.global.t('naturalLanguage.errors.familyIdMissing');
        this.loading = false;
        return false;
      }

      const sessionId = uuidv4(); // Generate sessionId here

      const command = {
      content: this.input,
      sessionId: sessionId,
      familyId: this.familyId,
    };
    const result = await this.services.family.generateFamilyData(command);

      if (result.ok) {
        this.parsedData = result.value; // Directly assign the object

        if (this.parsedData) {
          this.parsedData.members.forEach((member: MemberDataDto) => { // Explicitly type member
            if (!member.id) {
              member.id = uuidv4(); // Generate ID if missing
            }
            member.loading = false;
            member.savedSuccessfully = false;
            member.saveAlert = { show: false, type: 'success', message: '' };
          });
          this.parsedData.events.forEach((event: EventDataDto) => { // Explicitly type event
            if (!event.id) {
              event.id = uuidv4(); // Generate ID if missing
            }
            event.loading = false;
            event.savedSuccessfully = false;
            event.saveAlert = { show: false, type: 'success', message: '' };
          });
          this.parsedData.relationships.forEach((relationship: RelationshipDataDto) => { // Explicitly type relationship
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

    async saveMember(memberData: MemberDataDto): Promise<Result<string, ApiError>> { // Re-added function declaration
      this.loading = true;
      this.error = null;
      try {
        if (!this.familyId) {
          this.error = i18n.global.t('naturalLanguage.errors.familyIdMissing');
          return { ok: false, error: { message: this.error } } as Result<string, ApiError>;
        }

        const newMember: Omit<Member, 'id'> = {
          firstName: memberData.firstName ?? "",
          lastName: memberData.lastName ?? "",
          familyId: this.familyId,
          gender: memberData.gender as Gender,
          dateOfBirth: memberData.dateOfBirth ? new Date(memberData.dateOfBirth) : undefined,
          dateOfDeath: memberData.dateOfDeath ? new Date(memberData.dateOfDeath) : undefined,
        };
        const result: Result<string[], ApiError> = await this.services.member.addItems([newMember]); // Call addItems

        if (!result.ok) {
          this.error = result.error?.message || i18n.global.t('aiInput.saveError');
          return { ok: false, error: result.error } as Result<string, ApiError>;
        }
        // Assuming only one member is added, return the first ID
        return { ok: true, value: result.value[0] } as Result<string, ApiError>;
      } catch (e: any) {
        this.error = e.message;
        return { ok: false, error: { message: this.error } } as Result<string, ApiError>;
      } finally {
        this.loading = false;
      }
    },

    async saveEvent(eventData: EventDataDto): Promise<Result<string, ApiError>> { // Re-added function declaration
      this.loading = true;
      this.error = null;
      try {
        if (!this.familyId) {
          this.error = i18n.global.t('naturalLanguage.errors.familyIdMissing');
          return { ok: false, error: { message: this.error } } as Result<string, ApiError>;
        }

        const newEvent: Omit<Event, 'id'> = {
          name: eventData.description, // Using description as name
          description: eventData.description,
          startDate: eventData.date ? new Date(eventData.date) : null,
          location: eventData.location || undefined,
          familyId: this.familyId,
          type: eventData.type,
          relatedMembers: eventData.relatedMemberIds,
        };

        const result: Result<Event, ApiError> = await this.services.event.add(newEvent);
        if (!result.ok) {
          this.error = result.error?.message || i18n.global.t('aiInput.saveError');
          return { ok: false, error: result.error } as Result<string, ApiError>;
        }
        // Assuming only one event is added, return its ID
        return { ok: true, value: result.value.id! } as Result<string, ApiError>;
      } catch (e: any) {
        this.error = e.message;
        return { ok: false, error: { message: this.error } } as Result<string, ApiError>;
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