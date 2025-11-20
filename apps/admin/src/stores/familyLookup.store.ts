import i18n from '@/plugins/i18n';
import type { Family } from '@/types';
import { defineStore } from 'pinia';

export const useFamilyLookupStore = defineStore('familyLookup', {
  state: () => ({
    // Cache for families, keyed by ID
    familyCache: new Map<string, Family>(),
    loading: false,
    error: null as string | null,
  }),
  actions: {
    /**
     * Fetches families by their IDs, utilizing a cache to avoid redundant API calls.
     * @param ids An array of family IDs to fetch.
     * @returns A promise that resolves to an array of Family objects.
     */
    async getByIds(ids: string[]): Promise<Family[]> {
      this.loading = true;
      this.error = null;
      const familiesToFetch: string[] = [];
      const foundFamilies: Family[] = [];

      // Check cache first
      ids.forEach((id) => {
        const cachedFamily = this.familyCache.get(id);
        if (cachedFamily) {
          foundFamilies.push(cachedFamily);
        } else {
          familiesToFetch.push(id);
        }
      });

      if (familiesToFetch.length > 0) {
        const result = await this.services.family.getByIds(familiesToFetch);
        if (result.ok) {
          result.value.forEach((family) => {
            this.familyCache.set(family.id, family);
            foundFamilies.push(family);
          });
        } else {
          this.error =
            result.error.message || i18n.global.t('family.errors.loadByIds');
          console.error(result.error);
        }
      }

      this.loading = false;
      return foundFamilies;
    },

    /**
     * Adds or updates a family in the cache.
     * @param family The family object to cache.
     */
    cacheFamily(family: Family) {
      this.familyCache.set(family.id, family);
    },

    /**
     * Clears the entire family cache.
     */
    clearCache() {
      this.familyCache.clear();
    },
  },
});
