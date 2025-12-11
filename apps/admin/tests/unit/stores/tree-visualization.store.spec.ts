import { setActivePinia, createPinia } from 'pinia';
import { useTreeVisualizationStore } from '@/stores/tree-visualization.store';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import type { ApiError, Member, Relationship } from '@/types';
import { ok, err, RelationshipType,  } from '@/types';
import { createServices } from '@/services/service.factory';

// Mock services
const mockGetById = vi.fn();
const mockGetRelatives = vi.fn();
const mockFetchMembersByFamilyId = vi.fn();
const mockSearch = vi.fn();

vi.mock('@/services/service.factory', () => ({
  createServices: vi.fn(() => ({
    member: {
      getById: mockGetById,
      getRelatives: mockGetRelatives,
      fetchMembersByFamilyId: mockFetchMembersByFamilyId,
    },
    relationship: {
      search: mockSearch,
    },
  })),
}));

describe('tree-visualization.store', () => {
  let store: ReturnType<typeof useTreeVisualizationStore>;

  const mockFamilyId = 'family-1';
  const mockMember1: Member = {
    id: 'member-1',
    firstName: 'John',
    lastName: 'Doe',
    fullName: 'John Doe',
    code: 'JD1',
    familyId: mockFamilyId,
    isRoot: false,
    dateOfBirth: undefined,
    dateOfDeath: undefined,
    gender: undefined,
    nickname: undefined,
    occupation: undefined,
    placeOfBirth: undefined,
    placeOfDeath: undefined,
    avatarUrl: undefined,
    biography: undefined
  };
  const mockMember2: Member = {
    id: 'member-2',
    firstName: 'Jane',
    lastName: 'Doe',
    fullName: 'Jane Doe',
    code: 'JD2',
    familyId: mockFamilyId,
    isRoot: false,
    dateOfBirth: undefined,
    dateOfDeath: undefined,
    gender: undefined,
    nickname: undefined,
    occupation: undefined,
    placeOfBirth: undefined,
    placeOfDeath: undefined,
    avatarUrl: undefined,
    biography: undefined
  };
  const mockRelationship: Relationship = {
    id: 'rel-1',
    familyId: mockFamilyId,
    sourceMemberId: mockMember1.id,
    targetMemberId: mockMember2.id,
    type: RelationshipType.Father
  };

  beforeEach(() => {
    setActivePinia(createPinia());
    store = useTreeVisualizationStore();
    store.$reset();
    store.services = createServices('test');

    mockGetById.mockReset();
    mockGetRelatives.mockReset();
    mockFetchMembersByFamilyId.mockReset();
    mockSearch.mockReset();

    // Default successful mocks
    mockGetById.mockResolvedValue(ok(mockMember1));
    mockGetRelatives.mockResolvedValue(ok([]));
    mockFetchMembersByFamilyId.mockResolvedValue(ok([mockMember1, mockMember2]));
    mockSearch.mockResolvedValue(ok({ items: [mockRelationship], totalCount: 1 }));
  });

  it('should have correct initial state', () => {
    expect(store.trees).toEqual({});
    expect(store.loading).toEqual({});
    expect(store.error).toEqual({});
    expect(store.searchQuery).toBe('');
  });

  describe('getters', () => {
    beforeEach(() => {
      store.trees[mockFamilyId] = {
        members: [mockMember1, mockMember2],
        relationships: [mockRelationship],
      };
      store.loading[mockFamilyId] = false;
      store.error[mockFamilyId] = null;
    });

    it('getMembers should return members for a given familyId', () => {
      expect(store.getMembers(mockFamilyId)).toEqual([mockMember1, mockMember2]);
      expect(store.getMembers('non-existent-family')).toEqual([]);
    });

    it('getRelationships should return relationships for a given familyId', () => {
      expect(store.getRelationships(mockFamilyId)).toEqual([mockRelationship]);
      expect(store.getRelationships('non-existent-family')).toEqual([]);
    });

    it('isLoading should return loading state for a given familyId', () => {
      store.loading[mockFamilyId] = true;
      expect(store.isLoading(mockFamilyId)).toBe(true);
      expect(store.isLoading('non-existent-family')).toBe(false);
    });

    it('getError should return error state for a given familyId', () => {
      const testError: ApiError = { name: 'ApiError', message: 'Test Error' };
      store.error[mockFamilyId] = testError;
      expect(store.getError(mockFamilyId)).toEqual(testError);
      expect(store.getError('non-existent-family')).toBeNull();
    });

    it('getFilteredMembers should return all members if searchQuery is empty', () => {
      store.searchQuery = '';
      expect(store.getFilteredMembers(mockFamilyId)).toEqual([mockMember1, mockMember2]);
    });

    it('getFilteredMembers should return filtered members based on firstName', () => {
      store.searchQuery = 'john';
      expect(store.getFilteredMembers(mockFamilyId)).toEqual([mockMember1]);
    });

    it('getFilteredMembers should return filtered members based on lastName', () => {
      store.searchQuery = 'doe';
      expect(store.getFilteredMembers(mockFamilyId)).toEqual([mockMember1, mockMember2]);
    });

    it('getFilteredMembers should return empty array if no match', () => {
      store.searchQuery = 'xyz';
      expect(store.getFilteredMembers(mockFamilyId)).toEqual([]);
    });

    it('getFilteredRelationships should return all relationships if searchQuery is empty', () => {
      store.searchQuery = '';
      expect(store.getFilteredRelationships(mockFamilyId)).toEqual([mockRelationship]);
    });

    it('getFilteredRelationships should return filtered relationships based on member names', () => {
      store.searchQuery = 'john'; // Only member1 matches
      expect(store.getFilteredRelationships(mockFamilyId)).toEqual([]); // relationship is between member1 and member2, so both need to match
    });

    it('getFilteredRelationships should return relationships where both members match filter', () => {
      store.searchQuery = 'doe'; // Both member1 and member2 match
      expect(store.getFilteredRelationships(mockFamilyId)).toEqual([mockRelationship]);
    });

    it('getFilteredRelationships should return empty array if no match', () => {
      store.searchQuery = 'xyz';
      expect(store.getFilteredRelationships(mockFamilyId)).toEqual([]);
    });
  });

  describe('actions', () => {
    describe('fetchTreeData without memberId', () => {
      it('should fetch all members and relationships successfully', async () => {
        await store.fetchTreeData(mockFamilyId);

        expect(mockFetchMembersByFamilyId).toHaveBeenCalledWith(mockFamilyId);
        expect(mockSearch).toHaveBeenCalledWith({ page: 1, itemsPerPage: 1000 }, { familyId: mockFamilyId });
        expect(store.trees[mockFamilyId].members).toEqual([mockMember1, mockMember2]);
        expect(store.trees[mockFamilyId].relationships).toEqual([mockRelationship]);
        expect(store.isLoading(mockFamilyId)).toBe(false);
        expect(store.getError(mockFamilyId)).toBeNull();
      });

      it('should handle member fetch failure', async () => {
        const testError: ApiError = { name: 'ApiError', message: 'Failed to fetch members' };
        mockFetchMembersByFamilyId.mockResolvedValue(err(testError));

        await store.fetchTreeData(mockFamilyId);

        expect(mockFetchMembersByFamilyId).toHaveBeenCalledWith(mockFamilyId);
        expect(mockSearch).toHaveBeenCalledWith({ page: 1, itemsPerPage: 1000 }, { familyId: mockFamilyId });
        expect(store.trees[mockFamilyId].members).toEqual([]); // Members should be empty
        expect(store.trees[mockFamilyId].relationships).toEqual([mockRelationship]); // Relationships still fetched
        expect(store.isLoading(mockFamilyId)).toBe(false);
        expect(store.getError(mockFamilyId)).toEqual(testError);
      });

      it('should handle relationship fetch failure', async () => {
        const testError: ApiError = { name: 'ApiError', message: 'Failed to fetch relationships' };
        mockSearch.mockResolvedValue(err(testError));

        await store.fetchTreeData(mockFamilyId);

        expect(mockFetchMembersByFamilyId).toHaveBeenCalledWith(mockFamilyId);
        expect(mockSearch).toHaveBeenCalledWith({ page: 1, itemsPerPage: 1000 }, { familyId: mockFamilyId });
        expect(store.trees[mockFamilyId].members).toEqual([mockMember1, mockMember2]); // Members still fetched
        expect(store.trees[mockFamilyId].relationships).toEqual([]); // Relationships should be empty
        expect(store.isLoading(mockFamilyId)).toBe(false);
        expect(store.getError(mockFamilyId)).toEqual(testError);
      });

      it('should handle both member and relationship fetch failure', async () => {
        const memberError: ApiError = { name: 'ApiError', message: 'Failed to fetch members' };
        const relationshipError: ApiError = { name: 'ApiError', message: 'Failed to fetch relationships' };
        mockFetchMembersByFamilyId.mockResolvedValue(err(memberError));
        mockSearch.mockResolvedValue(err(relationshipError));

        await store.fetchTreeData(mockFamilyId);

        expect(store.trees[mockFamilyId].members).toEqual([]);
        expect(store.trees[mockFamilyId].relationships).toEqual([]);
        expect(store.isLoading(mockFamilyId)).toBe(false);
        // Pinia stores only one error, the last one set
        expect(store.getError(mockFamilyId)).toEqual(relationshipError);
      });
    });

    describe('fetchTreeData with memberId', () => {
      const targetMemberId = mockMember1.id;
      const mockRelative: Member = {
        id: 'member-3',
        firstName: 'Child',
        lastName: 'Doe',
        fullName: 'Child Doe',
        code: 'CD1',
        familyId: mockFamilyId,
        isRoot: false,
        dateOfBirth: undefined,
        dateOfDeath: undefined,
        gender: undefined,
        nickname: undefined,
        occupation: undefined,
        placeOfBirth: undefined,
        placeOfDeath: undefined,
        avatarUrl: undefined,
        biography: undefined
      };
      const mockMemberRelationship: Relationship = {
        id: 'rel-2',
        familyId: mockFamilyId,
        sourceMemberId: targetMemberId,
        targetMemberId: mockRelative.id,
        type: RelationshipType.Father
      };

      beforeEach(() => {
        mockGetById.mockResolvedValue(ok(mockMember1));
        mockGetRelatives.mockResolvedValue(ok([mockRelative]));
        mockSearch.mockResolvedValue(ok({ items: [mockMemberRelationship], totalCount: 1 }));
      });

      it('should fetch specific member, relatives and relationships successfully', async () => {
        await store.fetchTreeData(mockFamilyId, targetMemberId);

        expect(mockGetById).toHaveBeenCalledWith(targetMemberId);
        expect(mockGetRelatives).toHaveBeenCalledWith(targetMemberId);
        expect(mockSearch).toHaveBeenCalledWith({ page: 1, itemsPerPage: 1000 }, { familyId: mockFamilyId, memberIds: [targetMemberId, mockRelative.id] });
        expect(store.trees[mockFamilyId].members).toEqual([mockMember1, mockRelative]);
        expect(store.trees[mockFamilyId].relationships).toEqual([mockMemberRelationship]);
        expect(store.isLoading(mockFamilyId)).toBe(false);
        expect(store.getError(mockFamilyId)).toBeNull();
      });

      it('should handle getById failure', async () => {
        const testError: ApiError = { name: 'ApiError', message: 'Failed to get member by ID' };
        mockGetById.mockResolvedValue(err(testError));
        mockSearch.mockResolvedValue(ok({ items: [], totalCount: 0 })); // Mock loadItems to return empty

        await store.fetchTreeData(mockFamilyId, targetMemberId);

        expect(mockGetById).toHaveBeenCalledWith(targetMemberId);
        expect(mockGetRelatives).not.toHaveBeenCalled(); // Should not be called
        expect(mockSearch).toHaveBeenCalledWith({ page: 1, itemsPerPage: 1000 }, { familyId: mockFamilyId, memberIds: [] }); // Called with empty memberIds
        expect(store.trees[mockFamilyId].members).toEqual([]);
        expect(store.trees[mockFamilyId].relationships).toEqual([]);
        expect(store.isLoading(mockFamilyId)).toBe(false);
        expect(store.getError(mockFamilyId)).toEqual(testError);
      });

      it('should handle getRelatives failure', async () => {
        const testError: ApiError = { name: 'ApiError', message: 'Failed to get relatives' };
        mockGetRelatives.mockResolvedValue(err(testError));

        await store.fetchTreeData(mockFamilyId, targetMemberId);

        expect(mockGetById).toHaveBeenCalledWith(targetMemberId);
        expect(mockGetRelatives).toHaveBeenCalledWith(targetMemberId);
        expect(mockSearch).toHaveBeenCalledWith({ page: 1, itemsPerPage: 1000 }, { familyId: mockFamilyId, memberIds: [targetMemberId] }); // Only target member
        expect(store.trees[mockFamilyId].members).toEqual([mockMember1]);
        expect(store.trees[mockFamilyId].relationships).toEqual([mockMemberRelationship]); // Relationships still fetched
        expect(store.isLoading(mockFamilyId)).toBe(false);
        expect(store.getError(mockFamilyId)).toEqual(testError);
      });

      it('should handle relationship loadItems failure', async () => {
        const testError: ApiError = { name: 'ApiError', message: 'Failed to load relationships' };
        mockSearch.mockResolvedValue(err(testError));

        await store.fetchTreeData(mockFamilyId, targetMemberId);

        expect(mockGetById).toHaveBeenCalledWith(targetMemberId);
        expect(mockGetRelatives).toHaveBeenCalledWith(targetMemberId);
        expect(mockSearch).toHaveBeenCalledWith({ page: 1, itemsPerPage: 1000 }, { familyId: mockFamilyId, memberIds: [targetMemberId, mockRelative.id] });
        expect(store.trees[mockFamilyId].members).toEqual([mockMember1, mockRelative]);
        expect(store.trees[mockFamilyId].relationships).toEqual([]); // Relationships should be empty
        expect(store.isLoading(mockFamilyId)).toBe(false);
        expect(store.getError(mockFamilyId)).toEqual(testError);
      });
    });

    it('setSearchQuery should update searchQuery state', () => {
      const newQuery = 'test query';
      store.setSearchQuery(newQuery);
      expect(store.searchQuery).toBe(newQuery);
    });
  });
});
