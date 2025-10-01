import { describe, it, expect, beforeEach, vi } from 'vitest';
import { setActivePinia, createPinia } from 'pinia';
import { useMemberStore } from '@/stores/member.store';
import type { IMemberService } from '@/services/member/member.service.interface';
import { simulateLatency } from '@/utils/mockUtils';
import { createServices } from '@/services/service.factory';
import type { ApiError } from '@/utils/api';
import {
  err,
  ok,
  type Member,
  type MemberFilter,
  type Paginated,
  type Result,
} from '@/types';
import fix_members from '@/data/mock/members.json';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';

vi.mock('@/plugins/i18n', () => ({
  default: {
    global: {
      t: (key: string) => key,
    },
  },
}));

// Create a mock service for testing
class MockMemberServiceForTest implements IMemberService {
  public members: Member[] = JSON.parse(JSON.stringify(fix_members));
  public shouldThrowError: boolean = false;

  reset() {
    this.members = JSON.parse(JSON.stringify(fix_members));
    this.shouldThrowError = false;
  }

  async fetch(): Promise<Result<Member[], ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Không thể tải danh sách thành viên.' });
    }
    return ok(await simulateLatency(this.members));
  }

  async fetchMembersByFamilyId(
    familyId: string,
  ): Promise<Result<Member[], ApiError>> {
    if (this.shouldThrowError) {
      return err({
        message: 'Không thể tải danh sách thành viên theo ID gia đình.',
      });
    }
    const filteredItems = this.members.filter(
      (member) => member.familyId === familyId,
    );
    return ok(await simulateLatency(filteredItems));
  }

  async getById(id: string): Promise<Result<Member | undefined, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Không thể tải thành viên theo ID.' });
    }
    const member = this.members.find((m) => m.id === id);
    return ok(await simulateLatency(member));
  }

  async add(newItem: Omit<Member, 'id'>): Promise<Result<Member, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Không thể thêm thành viên.' });
    }
    const memberToAdd: Member = {
      ...newItem,
      id: new Date().getTime().toString(),
      dateOfBirth: newItem.dateOfBirth
        ? new Date(newItem.dateOfBirth)
        : undefined,
      dateOfDeath: newItem.dateOfDeath
        ? new Date(newItem.dateOfDeath)
        : undefined,
    };
    this.members.push(memberToAdd);
    return ok(await simulateLatency(memberToAdd));
  }

  async update(updatedItem: Member): Promise<Result<Member, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Không thể cập nhật thành viên.' });
    }
    const index = this.members.findIndex((m) => m.id === updatedItem.id);
    if (index !== -1) {
      const memberToUpdate: Member = {
        ...updatedItem,
        dateOfBirth: updatedItem.dateOfBirth
          ? new Date(updatedItem.dateOfBirth)
          : undefined,
        dateOfDeath: updatedItem.dateOfDeath
          ? new Date(updatedItem.dateOfDeath)
          : undefined,
      };
      this.members[index] = memberToUpdate;
      return ok(await simulateLatency(memberToUpdate));
    }
    return err({ message: 'Không tìm thấy thành viên.', statusCode: 404 });
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Không thể xóa thành viên.' });
    }
    const initialLength = this.members.length;
    this.members = this.members.filter((m) => m.id !== id);
    if (this.members.length === initialLength) {
      return err({ message: 'Không tìm thấy thành viên.', statusCode: 404 });
    }
    return ok(await simulateLatency(undefined));
  }

  async loadItems(
    filters: MemberFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Member>, ApiError>> {
    if (this.shouldThrowError) {
      return err({ message: 'Không thể tìm kiếm thành viên.' });
    }
    let filteredItems = [...this.members];

    if (filters.searchQuery) {
      const lowerCaseQuery = filters.searchQuery.toLowerCase();
      filteredItems = filteredItems.filter((m) => {
        const fullName = `${m.lastName} ${m.firstName}`.toLowerCase();
        return (
          fullName.includes(lowerCaseQuery) ||
          m.occupation?.toLowerCase().includes(lowerCaseQuery) ||
          m.placeOfBirth?.toLowerCase().includes(lowerCaseQuery)
        );
      });
    }

    const totalItems = filteredItems.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filteredItems.slice(start, end);

    return ok(
      await simulateLatency({
        items,
        totalItems,
        totalPages,
      }),
    );
  }

  async getByIds(ids: string[]): Promise<Result<Member[], ApiError>> {
    const foundMembers = this.members.filter((m) => ids.includes(m.id));
    return ok(await simulateLatency(foundMembers));
  }
}

const createStore = (shouldThrowError: boolean = false) => {
  const mockMemberService = new MockMemberServiceForTest();
  mockMemberService.shouldThrowError = shouldThrowError;
  const store = useMemberStore();
  store.services = createServices('test', { member: mockMemberService });
  return store;
};

describe('Member Store', () => {
  beforeEach(async () => {
    const pinia = createPinia();
    setActivePinia(pinia);
  });

  it('should have correct initial state after loading members', async () => {
    const store = createStore();
    expect(store.items.length).toBe(0);
    expect(store.loading).toBe(false);
    expect(store.error).toBe(null);
    expect(store.filters).toEqual({
      fullName: '',
      dateOfBirth: null,
      dateOfDeath: null,
      gender: undefined,
      placeOfBirth: '',
      placeOfDeath: '',
      occupation: '',
      familyId: undefined,
      searchQuery: '',
    });
    expect(store.currentPage).toBe(1);
    expect(store.itemsPerPage).toBe(DEFAULT_ITEMS_PER_PAGE);
    expect(store.totalItems).toBe(0);
    expect(store.totalPages).toBe(1);
  });

  it('getById should return the correct member', async () => {
    const store = createStore();
    await store.getById(fix_members[0].id);
    const member = store.currentItem;
    expect(member).toBeDefined();
    expect(member?.id).toBe(fix_members[0].id);
  });

  it('addItem should add a new member and reload items', async () => {
    const store = createStore();
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    const newMemberData: Member = {
      id: 'test-id',
      lastName: 'New',
      firstName: 'Member',
      fullName: 'New Member',
      familyId: 'family-001',
      dateOfBirth: new Date('2000-01-01'),
    };
    await store.addItem(newMemberData);
    await store.getById(newMemberData.id);
    expect(store.error).toBeNull();
    expect(loadItemsSpy).toHaveBeenCalled();
    expect(store.currentItem).toBeDefined();
  });

  it('updateItem should update an existing member and reload', async () => {
    const store = createStore();
    await store._loadItems();
    const memberToUpdate = { ...store.items[0] };
    const updatedLastName = 'Updated';
    memberToUpdate.lastName = updatedLastName;
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    await store.updateItem(memberToUpdate);
    await store.getById(memberToUpdate.id);
    const updatedMember = store.currentItem;
    expect(store.error).toBeNull();
    expect(loadItemsSpy).toHaveBeenCalled();
    expect(updatedMember?.lastName).toBe(updatedLastName);
  });

  it('deleteItem should remove a member and reload', async () => {
    const store = createStore();
    await store._loadItems();
    const memberToDeleteId = store.items[0].id;
    const initialTotalItems = store.totalItems;
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    await store.deleteItem(memberToDeleteId);
    const memberDeleted = store.currentItem;

    expect(store.error).toBeNull();
    expect(loadItemsSpy).toHaveBeenCalled();
    expect(store.totalItems).toBe(initialTotalItems - 1);
    expect(memberDeleted).toBeNull();
  });

  it('loadItems should filter members by search query', async () => {
    const store = createStore();
    const _loadItemsSpy = vi.spyOn(store, '_loadItems');
    const searchQuery = 'Nguyen';
    store.filters = {
      searchQuery: searchQuery,
    };
    await store._loadItems();
    expect(_loadItemsSpy).toHaveBeenCalled();
    expect(store.filters.searchQuery).toBe(searchQuery);
  });

  it('setPage should update currentPage and show correct items', async () => {
    const store = createStore();
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    await store._loadItems();
    await store.setPage(2);
    expect(loadItemsSpy).toHaveBeenCalledTimes(2);
    expect(store.currentPage).toBe(2);
  });

  it('setItemsPerPage should update itemsPerPage and reload', async () => {
    const store = createStore();
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    await store.setItemsPerPage(5);

    expect(store.itemsPerPage).toBe(5);
    expect(store.currentPage).toBe(1);
    expect(loadItemsSpy).toHaveBeenCalled();
  });

  it('_loadItems should set error on fetch failure', async () => {
    const store = createStore(true);
    await store._loadItems();
    expect(store.error).toBe('member.errors.load');
    expect(store.items.length).toBe(0);
    expect(store.totalItems).toBe(0);
  });
});
