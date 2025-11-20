import i18n from '@/plugins/i18n';
import type { Member } from '@/types';
import { defineStore } from 'pinia';

export const useMemberLookupStore = defineStore('memberLookup', {
  state: () => ({
    // Cache for members, keyed by ID
    memberCache: new Map<string, Member>(),
    loading: false,
    error: null as string | null,
  }),
  actions: {
    /**
     * Fetches members by their IDs, utilizing a cache to avoid redundant API calls.
     * @param ids An array of member IDs to fetch.
     * @returns A promise that resolves to an array of Member objects.
     */
    async getByIds(ids: string[]): Promise<Member[]> {
      this.loading = true;
      this.error = null;
      const membersToFetch: string[] = [];
      const foundMembers: Member[] = [];

      // Check cache first
      ids.forEach((id) => {
        const cachedMember = this.memberCache.get(id);
        if (cachedMember) {
          foundMembers.push(cachedMember);
        } else {
          membersToFetch.push(id);
        }
      });

      if (membersToFetch.length > 0) {
        const result = await this.services.member.getByIds(membersToFetch);
        if (result.ok) {
          result.value.forEach((member) => {
            this.memberCache.set(member.id, member);
            foundMembers.push(member);
          });
        } else {
          this.error =
            result.error.message || i18n.global.t('member.errors.loadByIds');
          console.error(result.error);
        }
      }

      this.loading = false;
      return foundMembers;
    },

    /**
     * Adds or updates a member in the cache.
     * @param member The member object to cache.
     */
    cacheMember(member: Member) {
      this.memberCache.set(member.id, member);
    },

    /**
     * Clears the entire member cache.
     */
    clearCache() {
      this.memberCache.clear();
    },
  },
});
