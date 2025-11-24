import { defineStore } from 'pinia';
import i18n from '@/plugins/i18n'; // Import the i18n instance
import type { Member } from '@/types';
import { useRouter } from 'vue-router'; // For navigation

interface LoadMembersOptions {
  page: number;
  itemsPerPage: number;
  sortBy?: string | null;
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
      // Access memberService via this.services as per convention
      // eslint-disable-next-line @typescript-eslint/ban-ts-comment
      // @ts-ignore
      const result = await this.services.member.searchMembers({
        familyId: this.selectedFamilyId,
        searchQuery: this.searchMember,
        page: options.page,
        itemsPerPage: options.itemsPerPage,
        sortBy: options.sortBy,
      });

      if (result.isSuccess) {
        this.members = result.value?.items || [];
        this.totalMembers = result.value?.totalCount || 0;
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
    async selectMember(member: Member) {
      this.selectingMember = member.id;
      const router = useRouter(); // Access router inside action
      // Navigate to the member's memories studio
      await router.push({ name: 'MemberMemories', params: { memberId: member.id } });
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
