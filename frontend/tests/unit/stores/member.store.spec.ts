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
  public errorType:
    | 'load'
    | 'add'
    | 'update'
    | 'delete'
    | 'getById'
    | 'getByIds'
    | null = null;

  reset() {
    this.members = JSON.parse(JSON.stringify(fix_members));
    this.shouldThrowError = false;
    this.errorType = null;
  }

  async fetch(): Promise<Result<Member[], ApiError>> {
    if (this.shouldThrowError && this.errorType === 'load') {
      return err({ message: 'Không thể tải danh sách thành viên.' });
    }
    return ok(await simulateLatency(this.members));
  }

  async fetchMembersByFamilyId(
    familyId: string,
  ): Promise<Result<Member[], ApiError>> {
    if (this.shouldThrowError && this.errorType === 'load') {
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
    if (this.shouldThrowError && this.errorType === 'getById') {
      return err({ message: 'Không thể tải thành viên theo ID.' });
    }
    const member = this.members.find((m) => m.id === id);
    return ok(await simulateLatency(member));
  }

  async add(newItem: Omit<Member, 'id'>): Promise<Result<Member, ApiError>> {
    if (this.shouldThrowError && this.errorType === 'add') {
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
    if (this.shouldThrowError && this.errorType === 'update') {
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
    if (this.shouldThrowError && this.errorType === 'delete') {
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
    if (this.shouldThrowError && this.errorType === 'load') {
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
    if (this.shouldThrowError && this.errorType === 'getByIds') {
      return err({ message: 'Không thể tải danh sách thành viên theo ID.' });
    }
    const foundMembers = this.members.filter((m) => ids.includes(m.id));
    return ok(await simulateLatency(foundMembers));
  }
}

const createStore = (
  shouldThrowError: boolean = false,
  errorType: MockMemberServiceForTest['errorType'] = null,
) => {
  const mockMemberService = new MockMemberServiceForTest();
  mockMemberService.shouldThrowError = shouldThrowError;
  mockMemberService.errorType = errorType;
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
    await store._loadItems();
    await store.getById(store.items[0].id);
    const member = store.currentItem;
    expect(member).toBeDefined();
    expect(member?.id).toBe(store.items[0].id);
  });

  it('addItem should add a new member and reload items', async () => {
    const store = createStore();
    await store._loadItems(); // Initialize store with 0 items
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    const initialTotalItems = store.totalItems;
    const newMemberData: Omit<Member, 'id'> = {
      lastName: 'New',
      firstName: 'Member',
      fullName: 'New Member',
      familyId: 'family-001',
      dateOfBirth: new Date('2000-01-01'),
    };
    await store.addItem(newMemberData);
    expect(store.error).toBeNull();
    expect(loadItemsSpy).toHaveBeenCalled();
    expect(store.totalItems).toBe(initialTotalItems + 1);
  });

  it('addItem should set error on add failure', async () => {
    const store = createStore(true, 'add');
    const newMemberData: Omit<Member, 'id'> = {
      lastName: 'New',
      firstName: 'Member',
      fullName: 'New Member',
      familyId: 'family-001',
      dateOfBirth: new Date('2000-01-01'),
    };
    await store.addItem(newMemberData);
    expect(store.error).toBe('member.errors.add');
    expect(store.loading).toBe(false);
  });

  it('updateItem should update an existing member and reload', async () => {
    const store = createStore();
    await store._loadItems();
    const memberToUpdate = { ...store.items[0] };
    const updatedLastName = 'Updated';
    memberToUpdate.lastName = updatedLastName;
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    await store.updateItem(memberToUpdate);
    expect(store.error).toBeNull();
    expect(loadItemsSpy).toHaveBeenCalled();
    // After reload, the updated member should be in the store.items
    const updatedMemberInStore = store.items.find(
      (m) => m.id === memberToUpdate.id,
    );
    expect(updatedMemberInStore?.lastName).toBe(updatedLastName);
  });

  it('updateItem should set error on update failure', async () => {
    const store = createStore(true, 'update');
    await store._loadItems();
    const memberToUpdate = { ...store.items[0], lastName: 'Failed Update' };
    await store.updateItem(memberToUpdate);
    expect(store.error).toBe('member.errors.update');
    expect(store.loading).toBe(false);
  });

  it('deleteItem should remove a member and reload', async () => {
    const store = createStore();
    await store._loadItems();
    const memberToDeleteId = store.items[0].id;
    const initialTotalItems = store.totalItems;
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    await store.deleteItem(memberToDeleteId);
    expect(store.error).toBeNull();
    expect(loadItemsSpy).toHaveBeenCalled();
    expect(store.totalItems).toBe(initialTotalItems - 1);
  });

  it('deleteItem should set error on delete failure', async () => {
    const store = createStore(true, 'delete');
    await store._loadItems();
    const memberToDeleteId = store.items[0].id;
    await store.deleteItem(memberToDeleteId);
    expect(store.error?.length).greaterThan(0);
    expect(store.loading).toBe(false);
  });

  it('getByIds should return correct members', async () => {
    const store = createStore();
    await store._loadItems();
    const idsToFetch = [store.items[0].id, store.items[1].id];
    const result = await store.getByIds(idsToFetch);
    expect(result.length).toBe(2);
    expect(result[0].id).toBe(store.items[0].id);
    expect(result[1].id).toBe(store.items[1].id);
  });

  it('getByIds should set error on fetch failure', async () => {
    const store = createStore(true, 'getByIds');
    await store._loadItems();
    const idsToFetch = [store.items[0].id];
    const result = await store.getByIds(idsToFetch);
    expect(store.error).toBe('member.errors.load');
    expect(result).toEqual([]);
    expect(store.loading).toBe(false);
  });

  it('setPage should update currentPage and reload items', async () => {
    const store = createStore();
    await store._loadItems();
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    await store.setPage(2);
    expect(store.currentPage).toBe(2);
    expect(loadItemsSpy).toHaveBeenCalled();
  });

  it('setPage should not update currentPage for invalid page values', async () => {
    const store = createStore();
    await store._loadItems();
    const initialPage = store.currentPage;
    const loadItemsSpy = vi.spyOn(store, '_loadItems');

    await store.setPage(0); // Invalid page (too low)
    expect(store.currentPage).toBe(initialPage);
    expect(loadItemsSpy).not.toHaveBeenCalled();

    await store.setPage(store.totalPages + 1); // Invalid page (too high)
    expect(store.currentPage).toBe(initialPage);
    expect(loadItemsSpy).not.toHaveBeenCalled();

    await store.setPage(initialPage); // Same page
    expect(store.currentPage).toBe(initialPage);
    expect(loadItemsSpy).not.toHaveBeenCalled();
  });

  it('setItemsPerPage should update itemsPerPage and reload', async () => {
    const store = createStore();
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    await store.setItemsPerPage(5);
    expect(store.itemsPerPage).toBe(5);
    expect(store.currentPage).toBe(1);
    expect(loadItemsSpy).toHaveBeenCalled();
  });

  it('setItemsPerPage should not update itemsPerPage for invalid count values', async () => {
    const store = createStore();
    await store._loadItems();
    const initialItemsPerPage = store.itemsPerPage;
    const loadItemsSpy = vi.spyOn(store, '_loadItems');

    await store.setItemsPerPage(0); // Invalid count (too low)
    expect(store.itemsPerPage).toBe(initialItemsPerPage);
    expect(loadItemsSpy).not.toHaveBeenCalled();

    await store.setItemsPerPage(initialItemsPerPage); // Same count
    expect(store.itemsPerPage).toBe(initialItemsPerPage);
    expect(loadItemsSpy).not.toHaveBeenCalled();
  });

  it('_loadItems should set error on fetch failure', async () => {
    const store = createStore(true, 'load');
    await store._loadItems();
    expect(store.error).toBe('member.errors.load');
    expect(store.items.length).toBe(0);
    expect(store.totalItems).toBe(0);
  });

  it('getById should set error on fetch failure', async () => {
    const store = createStore(true, 'getById');
    await store.getById('some-id');
    expect(store.error).toBe('member.errors.loadById');
    expect(store.currentItem).toBeNull();
    expect(store.loading).toBe(false);
  });

  it('getByFamilyId should set familyId filter, reset page, set items per page and reload items', async () => {
    const store = createStore();
    const loadItemsSpy = vi.spyOn(store, '_loadItems');
    const setPageSpy = vi.spyOn(store, 'setPage');
    const setItemsPerPageSpy = vi.spyOn(store, 'setItemsPerPage');

    const familyId = 'family-001';
    await store.getByFamilyId(familyId);

    expect(store.filters.familyId).toBe(familyId);
    expect(setPageSpy).toHaveBeenCalledWith(1);
    expect(setItemsPerPageSpy).toHaveBeenCalledWith(5000);
    expect(loadItemsSpy).toHaveBeenCalled();
  });
});
