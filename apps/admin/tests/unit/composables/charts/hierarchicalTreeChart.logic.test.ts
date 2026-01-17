import { describe, it, expect, vi } from 'vitest';
import { transformFamilyData, determineMainChartId } from '@/composables/charts/hierarchicalTreeChart.logic';
import { Gender, RelationshipType } from '@/types';
import type { MemberDto, Relationship } from '@/types';


// Mock getAvatarUrl since it's an external utility and not part of the logic being tested directly
vi.mock('@/utils/avatar.utils', () => ({
  getAvatarUrl: vi.fn((url, gender) => url || (gender === Gender.Male ? 'male.png' : 'female.png')),
}));

describe('hierarchicalTreeChart.logic', () => {
  const mockMembers: MemberDto[] = [
    { id: '1', firstName: 'John', lastName: 'Doe', fullName: 'John Doe', gender: Gender.Male, isRoot: true, familyId: 'f1' },
    { id: '2', firstName: 'Jane', lastName: 'Doe', fullName: 'Jane Doe', gender: Gender.Female, isRoot: false, familyId: 'f1' },
    { id: '3', firstName: 'Child', lastName: 'Doe', fullName: 'Child Doe', gender: Gender.Male, isRoot: false, familyId: 'f1' },
    { id: '4', firstName: 'Partner', lastName: 'X', fullName: 'Partner X', gender: Gender.Female, isRoot: false, familyId: 'f1' },
  ];

  const mockRelationships: Relationship[] = [
    { id: 'rel1', sourceMemberId: '1', targetMemberId: '3', type: RelationshipType.Father, familyId: 'f1' },
    { id: 'rel2', sourceMemberId: '2', targetMemberId: '3', type: RelationshipType.Mother, familyId: 'f1' },
    { id: 'rel3', sourceMemberId: '1', targetMemberId: '4', type: RelationshipType.Husband, familyId: 'f1' }, // John is husband of Partner
    { id: 'rel4', sourceMemberId: '4', targetMemberId: '1', type: RelationshipType.Wife, familyId: 'f1' }, // Partner is wife of John
  ];

  describe('transformFamilyData', () => {
    it('should correctly transform members and relationships into f3 format', () => {
      const transformed = transformFamilyData(mockMembers, mockRelationships, null);

      const john = transformed.transformedData.find(p => p.id === '1');
      const jane = transformed.transformedData.find(p => p.id === '2');
      const child = transformed.transformedData.find(p => p.id === '3');
      const partner = transformed.transformedData.find(p => p.id === '4');

      expect(john).toBeDefined();
      expect(john?.data.fullName).toBe('John Doe');
      expect(john?.data.gender).toBe('M');
      expect(john?.rels.spouses).toContain('4');
      expect(john?.rels.children).toContain('3');

      expect(jane).toBeDefined();
      expect(jane?.data.fullName).toBe('Jane Doe');
      expect(jane?.data.gender).toBe('F');
      expect(jane?.rels.father).toBeUndefined(); // Jane is not a child of anyone here
      expect(jane?.rels.mother).toBeUndefined(); // Jane is not a child of anyone here
      expect(jane?.rels.children).toContain('3'); // Jane is mother of child

      expect(child).toBeDefined();
      expect(child?.data.fullName).toBe('Child Doe');
      expect(child?.rels.father).toBe('1');
      expect(child?.rels.mother).toBe('2');

      expect(partner).toBeDefined();
      expect(partner?.data.fullName).toBe('Partner X');
      expect(partner?.rels.spouses).toContain('1');
    });

    it('should mark the root member if provided', () => {
      const transformed = transformFamilyData(mockMembers, mockRelationships, '1');
      const john = transformed.transformedData.find(p => p.id === '1');
      expect(john?.data.main).toBe(true);
    });

    it('should not mark a root member if not provided', () => {
      const transformed = transformFamilyData(mockMembers, mockRelationships, null);
      const john = transformed.transformedData.find(p => p.id === '1');
      expect(john?.data.main).toBeUndefined();
    });

    it('should handle members with no relationships', () => {
      const loneMember: MemberDto[] = [
        { id: '5', firstName: 'Lone', lastName: 'Wolf', fullName: 'Lone Wolf', gender: Gender.Male, isRoot: false, familyId: 'f1' },
      ];
      const transformed = transformFamilyData(loneMember, [], null);
      expect(transformed.transformedData.length).toBe(1);
      expect(transformed.transformedData[0].rels.spouses).toEqual([]);
      expect(transformed.transformedData[0].rels.children).toEqual([]);
      expect(transformed.transformedData[0].rels.father).toBeUndefined();
      expect(transformed.transformedData[0].rels.mother).toBeUndefined();
      expect(transformed.filteredMembers.length).toBe(1);
    });

    it('should handle empty members array', () => {
      const transformed = transformFamilyData([], mockRelationships, null);
      expect(transformed.transformedData).toEqual([]);
      expect(transformed.filteredMembers).toEqual([]);
    });

    it('should handle gender undefined', () => {
      const unknownGenderMember: MemberDto[] = [
        { id: '6', firstName: 'Unknown', lastName: 'Gender', fullName: 'Unknown Gender', gender: Gender.Other, familyId: 'f1' },
      ];
      const transformed = transformFamilyData(unknownGenderMember, [], null);
      expect(transformed.transformedData[0].data.gender).toBe('F'); // Expect 'F' due to new logic
    });
  });

  describe('determineMainChartId', () => {
    const transformedData = [
      { id: '1', data: { fullName: 'John Doe', gender: 'M' }, rels: { spouses: [], children: [] } },
      { id: '2', data: { fullName: 'Jane Doe', gender: 'F' }, rels: { spouses: [], children: [] } },
      { id: 'root', data: { fullName: 'Root MemberDto', gender: 'M', main: true }, rels: { spouses: [], children: [] } },
    ] as any; // Cast to any to simplify mock type

    it('should return providedRootId if it exists in transformedData', () => {
      const result = determineMainChartId(mockMembers, transformedData, '2');
      expect(result).toBe('2');
    });

    it('should return the ID of the root member if providedRootId is null or not found', () => {
      const result = determineMainChartId(mockMembers, transformedData, null);
      expect(result).toBe('1'); // John Doe is marked as isRoot: true in mockMembers
    });

    it('should return the first transformedData ID if no root member and no providedRootId', () => {
      const noRootMembers: MemberDto[] = mockMembers.map(m => ({ ...m, isRoot: false }));
      const result = determineMainChartId(noRootMembers, transformedData, null);
      expect(result).toBe('1'); // First item in transformedData is '1'
    });

    it('should return undefined if no members and no providedRootId', () => {
      const result = determineMainChartId([], [], null);
      expect(result).toBeUndefined();
    });
  });
});