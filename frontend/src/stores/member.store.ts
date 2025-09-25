import { defineStore } from 'pinia';
import type { Member } from '@/types/family';
import type { MemberFilter } from '@/services/member';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

export const useMemberStore = defineStore('member', {
  state: () => ({
    items: [] as Member[],
    currentItem: null as Member | null,
    loading: false,
    error: null as string | null,
    filters: {
      fullName: '',
      dateOfBirth: null,
      dateOfDeath: null,
      gender: undefined,
      placeOfBirth: '',
      placeOfDeath: '',
      occupation: '',
      familyId: undefined,
    } as MemberFilter,
    currentPage: 1,
    itemsPerPage: DEFAULT_ITEMS_PER_PAGE,
    totalItems: 0,
    totalPages: 1,
  }),

  getters: {
    paginatedItems: (state): Member[] => {
      return state.items;
    },

    /** Lấy 1 member theo id */
    getItemById: (state) => (id: string) => {
      return state.items.find((m) => m.id === id);
    },
  },

  actions: {
    async _loadItems() {
      this.loading = true;
      this.error = null;

      // Simulate API call delay
      await new Promise(resolve => setTimeout(resolve, 500));

      let filteredItems = mockMembers;

      // Apply filters
      if (this.filters.fullName) {
        const searchLower = this.filters.fullName.toLowerCase();
        filteredItems = filteredItems.filter(m => m.fullName?.toLowerCase().includes(searchLower));
      }
      if (this.filters.familyId) {
        filteredItems = filteredItems.filter(m => m.familyId === this.filters.familyId);
      }
      if (this.filters.gender) {
        filteredItems = filteredItems.filter(m => m.gender === this.filters.gender);
      }
      // Add other filters as needed

      const totalFilteredItems = filteredItems.length;
      const startIndex = (this.currentPage - 1) * this.itemsPerPage;
      const endIndex = startIndex + this.itemsPerPage;
      const paginatedResult = filteredItems.slice(startIndex, endIndex);

      this.items = paginatedResult;
      this.totalItems = totalFilteredItems;
      this.totalPages = Math.ceil(totalFilteredItems / this.itemsPerPage);
      this.loading = false;
    },

    async addItem(newItem: Omit<Member, 'id'>) {
      this.loading = true;
      this.error = null;
      // Simulate adding item to mock data
      const newMember: Member = { ...newItem, id: (mockMembers.length + 1).toString(), fullName: `${newItem.firstName} ${newItem.lastName}` };
      mockMembers.push(newMember);
      await this._loadItems(); // Reload paginated items
      this.loading = false;
    },

    async updateItem(updatedItem: Member) {
      this.loading = true;
      this.error = null;
      // Simulate updating item in mock data
      const index = mockMembers.findIndex(m => m.id === updatedItem.id);
      if (index !== -1) {
        mockMembers[index] = updatedItem;
      }
      await this._loadItems(); // Reload paginated items
      this.loading = false;
    },

    async deleteItem(id: string) {
      this.loading = true;
      this.error = null;
      // Simulate deleting item from mock data
      mockMembers = mockMembers.filter(m => m.id !== id);
      await this._loadItems(); // Reload paginated items
      this.loading = false;
    },

    async searchItems(filters: MemberFilter) {
      const newFilters: MemberFilter = { ...filters };
      if (typeof newFilters.dateOfBirth === 'string') {
        newFilters.dateOfBirth = new Date(newFilters.dateOfBirth);
      }
      if (typeof newFilters.dateOfDeath === 'string') {
        newFilters.dateOfDeath = new Date(newFilters.dateOfDeath);
      }
      this.filters = { ...this.filters, ...newFilters };
      this.currentPage = 1;
      await this._loadItems();
    },

    async setPage(page: number) {
      if (page >= 1 && page <= this.totalPages) {
        this.currentPage = page;
        await this._loadItems();
      }
    },

    async setItemsPerPage(count: number) {
      if (count > 0) {
        this.itemsPerPage = count;
        this.currentPage = 1;
        await this._loadItems();
      }
    },

    setCurrentItem(item: Member | null) {
      this.currentItem = item;
    },
  },
});

// Mock data generation (outside the store definition)
let mockMembers: Member[] = [];
for (let i = 1; i <= 1200; i++) {
  mockMembers.push({
    id: i.toString(),
    lastName: `Last${i}`,
    firstName: `First${i}`,
    fullName: `First${i} Last${i}`,
    familyId: (i % 5 + 1).toString(), // Assign to 5 different families
    gender: i % 2 === 0 ? 'male' : 'female',
        dateOfBirth: new Date(1980 + (i % 30), (i % 12), (i % 28) + 1),
        dateOfDeath: i % 7 === 0 ? new Date(2010 + (i % 10), (i % 12), (i % 28) + 1) : undefined, // Add some death dates
        birthDeathYears: `(${1980 + (i % 30)} - ${i % 7 === 0 ? (2010 + (i % 10)) : ''})`, // Formatted years
        avatarUrl: `https://randomuser.me/api/portraits/${i % 2 === 0 ? 'men' : 'women'}/${i % 100}.jpg`,
        nickname: `Nick${i}`,    placeOfBirth: `City${i % 10}`,
    placeOfDeath: `City${(i + 1) % 10}`,
    occupation: `Occupation${i % 5}`,
    biography: `Biography of member ${i}`,
  });
}
