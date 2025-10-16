import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n';
import type { Family, Member, Event, Relationship } from '@/types';

export const useNaturalLanguageInputStore = defineStore('naturalLanguageInput', {
  state: () => ({
    loading: false,
    error: null as string | null,
  }),
  actions: {
    async generateFamilyData(prompt: string): Promise<Family[] | null> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.naturalLanguageInput.generateFamilyData(prompt);
        if (result.ok) {
          return result.value;
        } else {
          this.error = i18n.global.t('aiInput.generateError', { entity: i18n.global.t('aiInput.families') });
          console.error(result.error);
          return null;
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('aiInput.generateError', { entity: i18n.global.t('aiInput.families') });
        console.error(err);
        return null;
      } finally {
        this.loading = false;
      }
    },

    async generateMemberData(prompt: string): Promise<Member[] | null> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.naturalLanguageInput.generateMemberData(prompt);
        if (result.ok) {
          return result.value;
        } else {
          this.error = i18n.global.t('aiInput.generateError', { entity: i18n.global.t('aiInput.members') });
          console.error(result.error);
          return null;
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('aiInput.generateError', { entity: i18n.global.t('aiInput.members') });
        console.error(err);
        return null;
      } finally {
        this.loading = false;
      }
    },

    async generateEventData(prompt: string): Promise<Event[] | null> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.naturalLanguageInput.generateEventData(prompt);
        if (result.ok) {
          return result.value;
        } else {
          this.error = i18n.global.t('aiInput.generateError', { entity: i18n.global.t('aiInput.events') });
          console.error(result.error);
          return null;
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('aiInput.generateError', { entity: i18n.global.t('aiInput.events') });
        console.error(err);
        return null;
      } finally {
        this.loading = false;
      }
    },

    async generateRelationshipData(prompt: string): Promise<Relationship[] | null> {
      this.loading = true;
      this.error = null;
      try {
        const result = await this.services.naturalLanguageInput.generateRelationshipData(prompt);
        if (result.ok) {
          return result.value;
        } else {
          this.error = i18n.global.t('aiInput.generateError', { entity: i18n.global.t('aiInput.relationships') });
          console.error(result.error);
          return null;
        }
      } catch (err: any) {
        this.error = err.message || i18n.global.t('aiInput.generateError', { entity: i18n.global.t('aiInput.relationships') });
        console.error(err);
        return null;
      } finally {
        this.loading = false;
      }
    },
  },
});