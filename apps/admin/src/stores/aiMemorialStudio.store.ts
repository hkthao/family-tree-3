import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n'; // Import the i18n instance
import type { Member } from '@/types';
import { useRouter } from 'vue-router'; // For navigation

interface LoadMembersOptions {
  page: number;
  itemsPerPage: number;
  sortBy?: { key: string; order: string }[]; // Updated to match v-data-table-server output
}

export const useAIMemorialStudioStore = defineStore('aiMemorialStudio', {
  state: () => ({
    selectedFamilyId: null as string | null,
    searchMember: '',
    members: [] as Member[],
    totalMembers: 0,
    loadingMembers: false,
    itemsPerPage: 10,
    selectingMember: null as string | null, // To track which member is being selected
  }),
  getters: {
    headers: () => {
      // Accessing i18n.global directly because it's a global instance
      const t = i18n.global.t;
      return [
        { title: t('member.list.headers.avatar'), key: 'avatarUrl', sortable: false },
        { title: t('member.list.headers.fullName'), key: 'fullName' },
        { title: t('member.list.headers.gender'), key: 'gender' },
        { title: t('member.list.headers.birthDeathYears'), key: 'birthDeathYears' },
        { title: t('aiMemorialStudio.selection.actions'), key: 'actions', sortable: false },
      ];
    },
  },
  actions: {
    async loadMembers(options: LoadMembersOptions) {
      if (!this.selectedFamilyId) {
        this.members = [];
        this.totalMembers = 0;
        return;
      }

      this.loadingMembers = true;

      // Extract sort parameters
      const sortBy = options.sortBy && options.sortBy.length > 0 ? options.sortBy[0].key : undefined;
      const sortOrder = options.sortBy && options.sortBy.length > 0 ? (options.sortBy[0].order as 'asc' | 'desc') : undefined;


      const result = await this.services.member.loadItems(
        {
          familyId: this.selectedFamilyId,
          searchQuery: this.searchMember,
          sortBy: sortBy,
          sortOrder: sortOrder,
        },
        options.page,
        options.itemsPerPage,
      );

      if (result.ok) {
        this.members = result.value.items || [];
        this.totalMembers = result.value.totalItems || 0;
      } else {
        this.members = [];
        this.totalMembers = 0;
        // Optionally, handle error notification here using i18n.global.t
        // console.error(i18n.global.t('family.errors.loadMembers'));
      }
      this.loadingMembers = false;
    },
    handleFamilySelection(familyId: string | null) {
      this.selectedFamilyId = familyId;
      if (familyId) {
        this.loadMembers({ page: 1, itemsPerPage: this.itemsPerPage });
      } else {
        this.members = [];
        this.totalMembers = 0;
      }
    },
    async selectMember(member: Member, aiMemorialStudioType: 'story' | 'photo' | 'voice') {
      this.selectingMember = member.id;
      const router = useRouter(); // Access router inside action
      // Navigate to the member's memories studio with the specified type
      await router.push({ name: 'MemberMemories', params: { memberId: member.id, aiMemorialStudioType } });
      this.selectingMember = null;
    },
    // You might want to add an initialization action if necessary
    // For example, if you want to load from query params or a previous state
    init(initialFamilyId: string | null = null) {
      if (initialFamilyId) {
        this.selectedFamilyId = initialFamilyId;
        this.loadMembers({ page: 1, itemsPerPage: this.itemsPerPage });
      }
    },
  },
});
