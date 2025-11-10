import { defineStore } from 'pinia';
import type { ParsedMember, ParsedEvent } from '@/types/natural-language.d';
import type { Family, Event, Member, Relationship } from '@/types';
import type { ApiError } from '@/plugins/axios';
import i18n from '@/plugins/i18n';

export const useNaturalLanguageStore = defineStore('naturalLanguage', {
  state: () => ({
    input: '' as string,
    parsedData: null as (ParsedMember | ParsedEvent | Family | Event | Member | Relationship | null),
    entityType: null as ('Member' | 'Event' | 'Family' | 'Relationship' | null),
    loading: false as boolean,
    error: null as string | null,
  }),

  actions: {
    async parseInput(): Promise<boolean> {
      this.loading = true;
      this.error = null;
      this.parsedData = null;
      this.entityType = null;

      if (!this.input.trim()) {
        this.error = i18n.global.t('naturalLanguage.errors.emptyInput');
        this.loading = false;
        return false;
      }

      const result = await this.services.naturalLanguageInput.parseInput(this.input);

      if (result.ok) {
        this.parsedData = result.value.data;
        this.entityType = result.value.entityType as 'Member' | 'Event' | 'Family' | 'Relationship';
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
      this.entityType = null;
      this.error = null;
      this.loading = false;
    },

    setInput(newInput: string) {
      this.input = newInput;
    },
  },
});
