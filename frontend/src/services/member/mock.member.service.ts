import type { IMemberService } from './member.service.interface'; // Import MemberFilter
import type { Member, MemberFilter } from '@/types/family/member';
import type { Paginated } from '@/types/common';
import { simulateLatency } from '@/utils/mockUtils';
import type { Result } from '@/types/common';
import { ok, err } from '@/types/common';
import type { ApiError } from '@/utils/api';
import { fixedMockFamilies } from '@/data/mock/fixed.family.mock'; // Import fixed mock families
import { Gender } from '@/types';

// Helper function to transform date strings to Date objects
function transformMemberDates(member: Member): Member {
  if (member.dateOfBirth && typeof member.dateOfBirth === 'string') {
    member.dateOfBirth = new Date(member.dateOfBirth);
  }
  if (member.dateOfDeath && typeof member.dateOfDeath === 'string') {
    member.dateOfDeath = new Date(member.dateOfDeath);
  }
  return member;
}
// Mock data generation (outside the class definition)
const royalFamilyMembers: Member[] = [
  {
    id: '1',
    lastName: 'VI',
    firstName: 'George',
    fullName: 'King George VI',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Male,
    dateOfBirth: new Date(1895, 11, 14),
    dateOfDeath: new Date(1952, 1, 6),
    avatarUrl: 'https://i.pravatar.cc/150?u=1',
    fatherId: undefined,
    motherId: undefined,
    spouseId: '2',
  },
  {
    id: '2',
    lastName: 'Mother',
    firstName: 'Elizabeth',
    fullName: 'Queen Elizabeth The Queen Mother',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Female,
    dateOfBirth: new Date(1900, 7, 4),
    dateOfDeath: new Date(2002, 2, 30),
    avatarUrl: 'https://i.pravatar.cc/150?u=2',
    fatherId: undefined,
    motherId: undefined,
    spouseId: '1',
  },
  {
    id: '3',
    lastName: 'Philip',
    firstName: 'Prince',
    fullName: 'Prince Philip',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Male,
    dateOfBirth: new Date(1921, 5, 10),
    dateOfDeath: new Date(2021, 3, 9),
    avatarUrl: 'https://i.pravatar.cc/150?u=3',
    fatherId: undefined,
    motherId: undefined,
    spouseId: '4',
  },
  {
    id: '4',
    lastName: 'II',
    firstName: 'Elizabeth',
    fullName: 'Queen Elizabeth II',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Female,
    dateOfBirth: new Date(1926, 3, 21),
    dateOfDeath: new Date(2022, 8, 8),
    avatarUrl: 'https://i.pravatar.cc/150?u=4',
    fatherId: '1',
    motherId: '2',
    spouseId: '3',
  },
  {
    id: '5',
    lastName: 'Margaret',
    firstName: 'Princess',
    fullName: 'Princess Margaret',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Female,
    dateOfBirth: new Date(1930, 7, 21),
    dateOfDeath: new Date(2002, 1, 9),
    avatarUrl: 'https://i.pravatar.cc/150?u=5',
    fatherId: '1',
    motherId: '2',
    spouseId: undefined, // No spouse in this simplified data
  },
  {
    id: '6',
    lastName: 'Camila',
    firstName: '',
    fullName: 'Camila',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Female,
    dateOfBirth: new Date(1947, 6, 17),
    dateOfDeath: undefined,
    avatarUrl: 'https://i.pravatar.cc/150?u=6',
    fatherId: undefined,
    motherId: undefined,
    spouseId: '7',
  },
  {
    id: '7',
    lastName: 'III',
    firstName: 'Charles',
    fullName: 'King Charles III',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Male,
    dateOfBirth: new Date(1948, 10, 14),
    dateOfDeath: undefined,
    avatarUrl: 'https://i.pravatar.cc/150?u=7',
    fatherId: '3',
    motherId: '4',
    spouseId: '6', // Married to Camila
  },
  {
    id: '8',
    lastName: 'Diana',
    firstName: 'Princess',
    fullName: 'Princess Diana',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Female,
    dateOfBirth: new Date(1961, 6, 1),
    dateOfDeath: new Date(1997, 7, 31),
    avatarUrl: 'https://i.pravatar.cc/150?u=8',
    fatherId: undefined,
    motherId: undefined,
    spouseId: '7', // Was married to Charles
  },
  {
    id: '9',
    lastName: 'Anne',
    firstName: 'Princess',
    fullName: 'Princess Anne',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Female,
    dateOfBirth: new Date(1950, 7, 15),
    dateOfDeath: undefined,
    avatarUrl: 'https://i.pravatar.cc/150?u=9',
    fatherId: '3',
    motherId: '4',
    spouseId: undefined,
  },
  {
    id: '10',
    lastName: 'Andrew',
    firstName: 'Prince',
    fullName: 'Prince Andrew',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Male,
    dateOfBirth: new Date(1960, 1, 19),
    dateOfDeath: undefined,
    avatarUrl: 'https://i.pravatar.cc/150?u=10',
    fatherId: '3',
    motherId: '4',
    spouseId: undefined,
  },
  {
    id: '11',
    lastName: 'Edward',
    firstName: 'Prince',
    fullName: 'Prince Edward',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Male,
    dateOfBirth: new Date(1964, 2, 10),
    dateOfDeath: undefined,
    avatarUrl: 'https://i.pravatar.cc/150?u=11',
    fatherId: '3',
    motherId: '4',
    spouseId: undefined,
  },
  {
    id: '12',
    lastName: 'Middleton',
    firstName: 'Catherine',
    fullName: 'Catherine, Princess of Wales',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Female,
    dateOfBirth: new Date(1982, 0, 9),
    dateOfDeath: undefined,
    avatarUrl: 'https://i.pravatar.cc/150?u=12',
    fatherId: undefined,
    motherId: undefined,
    spouseId: '13',
  },
  {
    id: '13',
    lastName: 'William',
    firstName: 'Prince',
    fullName: 'Prince William',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Male,
    dateOfBirth: new Date(1982, 5, 21),
    dateOfDeath: undefined,
    avatarUrl: 'https://i.pravatar.cc/150?u=13',
    fatherId: '7',
    motherId: '8',
    spouseId: '12',
  },
  {
    id: '14',
    lastName: 'Harry',
    firstName: 'Prince',
    fullName: 'Prince Harry',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Male,
    dateOfBirth: new Date(1984, 8, 15),
    dateOfDeath: undefined,
    avatarUrl: 'https://i.pravatar.cc/150?u=14',
    fatherId: '7',
    motherId: '8',
    spouseId: '15',
  },
  {
    id: '15',
    lastName: 'Markle',
    firstName: 'Meghan',
    fullName: 'Meghan Markle',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Female,
    dateOfBirth: new Date(1981, 7, 4),
    dateOfDeath: undefined,
    avatarUrl: 'https://i.pravatar.cc/150?u=15',
    fatherId: undefined,
    motherId: undefined,
    spouseId: '14',
  },
  {
    id: '16',
    lastName: 'George',
    firstName: 'Prince',
    fullName: 'Prince George',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Male,
    dateOfBirth: new Date(2013, 6, 22),
    dateOfDeath: undefined,
    avatarUrl: 'https://i.pravatar.cc/150?u=16',
    fatherId: '13',
    motherId: '12',
    spouseId: undefined,
  },
  {
    id: '17',
    lastName: 'Charlotte',
    firstName: 'Princess',
    fullName: 'Princess Charlotte',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Female,
    dateOfBirth: new Date(2015, 4, 2),
    dateOfDeath: undefined,
    avatarUrl: 'https://i.pravatar.cc/150?u=17',
    fatherId: '13',
    motherId: '12',
    spouseId: undefined,
  },
  {
    id: '18',
    lastName: 'Louis',
    firstName: 'Prince',
    fullName: 'Prince Louis',
    familyId: fixedMockFamilies[0].id,
    gender: Gender.Male,
    dateOfBirth: new Date(2018, 3, 23),
    dateOfDeath: undefined,
    avatarUrl: 'https://i.pravatar.cc/150?u=18',
    fatherId: '13',
    motherId: '12',
    spouseId: undefined,
  },
];

// New family: The Taylors
let nextId = 100;
const generateMember = (
  lastName: string,
  firstName: string,
  gender: Gender,
  birthYear: number,
  familyId: string,
  fatherId?: string,
  motherId?: string,
  spouseId?: string,
) => ({
  id: (nextId++).toString(),
  lastName,
  firstName,
  fullName: `${firstName} ${lastName}`,
  familyId,
  gender,
  dateOfBirth: new Date(birthYear, 0, 1),
  dateOfDeath: undefined,
  avatarUrl: `https://i.pravatar.cc/150?u=${nextId}`,
  fatherId,
  motherId,
  spouseId,
});

const taylorFamilyMembers: Member[] = [];
const family2Id = '2';

// Generation 1
const g1_taylor_m = generateMember(
  'Taylor',
  'Robert',
  Gender.Male,
  1940,
  family2Id,
);
const g1_taylor_f = generateMember(
  'Taylor',
  'Mary',
  Gender.Female,
  1942,
  family2Id,
);
g1_taylor_m.spouseId = g1_taylor_f.id;
g1_taylor_f.spouseId = g1_taylor_m.id;
taylorFamilyMembers.push(g1_taylor_m, g1_taylor_f);

// Generation 2
const g2_taylor_m1 = generateMember(
  'Taylor',
  'David',
  Gender.Male,
  1965,
  family2Id,
  g1_taylor_m.id,
  g1_taylor_f.id,
);
const g2_taylor_f1 = generateMember(
  'Johnson',
  'Sarah',
  Gender.Female,
  1968,
  family2Id,
);
g2_taylor_m1.spouseId = g2_taylor_f1.id;
g2_taylor_f1.spouseId = g2_taylor_m1.id;
taylorFamilyMembers.push(g2_taylor_m1, g2_taylor_f1);

const g2_taylor_f2 = generateMember(
  'Taylor',
  'Lisa',
  Gender.Female,
  1970,
  family2Id,
  g1_taylor_m.id,
  g1_taylor_f.id,
);
const g2_taylor_m2 = generateMember(
  'Brown',
  'Michael',
  Gender.Male,
  1969,
  family2Id,
);
g2_taylor_f2.spouseId = g2_taylor_m2.id;
g2_taylor_m2.spouseId = g2_taylor_f2.id;
taylorFamilyMembers.push(g2_taylor_f2, g2_taylor_m2);

// Generation 3 (Children of David & Sarah) (IDs will be 104, 105)
const g3_taylor_m1_c1 = generateMember(
  'Taylor',
  'Chris',
  Gender.Male,
  1995,
  family2Id,
  g2_taylor_m1.id,
  g2_taylor_f1.id,
);
taylorFamilyMembers.push(g3_taylor_m1_c1);
const g3_taylor_f1_c2 = generateMember(
  'Taylor',
  'Emily',
  Gender.Female,
  1998,
  family2Id,
  g2_taylor_m1.id,
  g2_taylor_f1.id,
);
taylorFamilyMembers.push(g3_taylor_f1_c2);

// Generation 3 (Children of Lisa & Michael) (IDs will be 106, 107)
const g3_taylor_f2_c1 = generateMember(
  'Brown',
  'Jessica',
  Gender.Female,
  1996,
  family2Id,
  g2_taylor_m2.id,
  g2_taylor_f2.id,
);
taylorFamilyMembers.push(g3_taylor_f2_c1);
const g3_taylor_m2_c2 = generateMember(
  'Brown',
  'Daniel',
  Gender.Male,
  1999,
  family2Id,
  g2_taylor_m2.id,
  g2_taylor_f2.id,
);
taylorFamilyMembers.push(g3_taylor_m2_c2);

// Generation 4 (Children of Chris) (ID will be 108)
const g4_taylor_m1_c1_c1 = generateMember(
  'Taylor',
  'Leo',
  Gender.Male,
  2020,
  family2Id,
  g3_taylor_m1_c1.id,
  undefined,
); // Single parent
taylorFamilyMembers.push(g4_taylor_m1_c1_c1);

// Combine all mock members
const mockMembers: Member[] = [...royalFamilyMembers, ...taylorFamilyMembers];

export class MockMemberService implements IMemberService {
  private _members: Member[] = mockMembers;

  get members(): Member[] {
    return [...this._members];
  }

  async fetch(): Promise<Result<Member[], ApiError>> {
    // Renamed from fetchMembers
    try {
      const members = await simulateLatency(
        this.members.map((m) => transformMemberDates(m)),
      );
      return ok(members);
    } catch (e) {
      return err({
        message: 'Failed to fetch members from mock service.',
        details: e as Error,
      });
    }
  }

  async fetchMembersByFamilyId(
    familyId: string,
  ): Promise<Result<Member[], ApiError>> {
    try {
      const members = await simulateLatency(
        this.members
          .filter((m) => m.familyId === familyId)
          .map((m) => transformMemberDates(m)),
      );
      return ok(members);
    } catch (e) {
      return err({
        message: `Failed to fetch members for family ID ${familyId} from mock service.`,
        details: e as Error,
      });
    }
  }

  async getById(id: string): Promise<Result<Member | undefined, ApiError>> {
    // Renamed from getMemberById
    try {
      const member = await simulateLatency(
        this.members.find((m) => m.id === id),
      );
      return ok(member ? transformMemberDates(member) : undefined);
    } catch (e) {
      return err({
        message: `Failed to get member with ID ${id} from mock service.`,
        details: e as Error,
      });
    }
  }

  async add(newItem: Omit<Member, 'id'>): Promise<Result<Member, ApiError>> {
    // Renamed to add
    try {
      const newId = (this._members.length + 1).toString(); // Simple sequential ID
      const memberToAdd = { ...newItem, id: newId };
      this._members.push(memberToAdd);
      const addedMember = await simulateLatency(
        transformMemberDates(memberToAdd),
      );
      return ok(addedMember);
    } catch (e) {
      return err({
        message: 'Failed to add member to mock service.',
        details: e as Error,
      });
    }
  }

  async update(updatedItem: Member): Promise<Result<Member, ApiError>> {
    // Renamed to update
    try {
      const index = this._members.findIndex((m) => m.id === updatedItem.id);
      if (index !== -1) {
        this._members[index] = updatedItem;
        const updatedMember = await simulateLatency(
          transformMemberDates(updatedItem),
        );
        return ok(updatedMember);
      }
      return err({ message: 'Member not found', statusCode: 404 });
    } catch (e) {
      return err({
        message: 'Failed to update member in mock service.',
        details: e as Error,
      });
    }
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    // Renamed to delete
    try {
      const initialLength = this._members.length;
      this._members = this._members.filter((m) => m.id !== id);
      if (this._members.length === initialLength) {
        return err({ message: 'Member not found', statusCode: 404 });
      }
      await simulateLatency(undefined);
      return ok(undefined);
    } catch (e) {
      return err({
        message: 'Failed to delete member from mock service.',
        details: e as Error,
      });
    }
  }

  async searchItems(
    filters: MemberFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Member>, ApiError>> {
    try {
      let filteredMembers = this.members;

      if (filters.fullName) {
        const lowerCaseFullName = filters.fullName.toLowerCase();
        filteredMembers = filteredMembers.filter(
          (m) =>
            m.lastName.toLowerCase().includes(lowerCaseFullName) ||
            m.firstName.toLowerCase().includes(lowerCaseFullName) ||
            `${m.lastName} ${m.firstName}`
              .toLowerCase()
              .includes(lowerCaseFullName),
        );
      }
      if (filters.dateOfBirth) {
        // Compare Date objects
        filteredMembers = filteredMembers.filter(
          (m) =>
            m.dateOfBirth?.toISOString().split('T')[0] ===
            filters.dateOfBirth?.toISOString().split('T')[0],
        );
      }
      if (filters.dateOfDeath) {
        // Compare Date objects
        filteredMembers = filteredMembers.filter(
          (m) =>
            m.dateOfDeath?.toISOString().split('T')[0] ===
            filters.dateOfDeath?.toISOString().split('T')[0],
        );
      }
      if (filters.gender) {
        filteredMembers = filteredMembers.filter(
          (m) => m.gender === filters.gender,
        );
      }
      if (filters.placeOfBirth) {
        const lowerCasePlaceOfBirth = filters.placeOfBirth.toLowerCase();
        filteredMembers = filteredMembers.filter((m) =>
          m.placeOfBirth?.toLowerCase().includes(lowerCasePlaceOfBirth),
        );
      }
      if (filters.placeOfDeath) {
        const lowerCasePlaceOfDeath = filters.placeOfDeath.toLowerCase();
        filteredMembers = filteredMembers.filter((m) =>
          m.placeOfDeath?.toLowerCase().includes(lowerCasePlaceOfDeath),
        );
      }
      if (filters.occupation) {
        const lowerCaseOccupation = filters.occupation.toLowerCase();
        filteredMembers = filteredMembers.filter((m) =>
          m.occupation?.toLowerCase().includes(lowerCaseOccupation),
        );
      }
      if (filters.familyId) {
        filteredMembers = filteredMembers.filter(
          (m) => m.familyId === filters.familyId,
        );
      }

      const totalItems = filteredMembers.length;
      const totalPages = Math.ceil(totalItems / itemsPerPage);
      const start = (page - 1) * itemsPerPage;
      const end = start + itemsPerPage;
      const items = filteredMembers.slice(start, end);

      const paginatedResult = await simulateLatency({
        items,
        totalItems,
        totalPages,
      });
      return ok(paginatedResult);
    } catch (e) {
      return err({
        message: 'Failed to search members from mock service.',
        details: e as Error,
      });
    }
  }

  async getManyByIds(ids: string[]): Promise<Result<Member[], ApiError>> {
    try {
      const members = await simulateLatency(
        this._members
          .filter((m) => ids.includes(m.id))
          .map((m) => transformMemberDates(m)),
      );
      return ok(members);
    } catch (e) {
      return err({
        message: 'Failed to get members by IDs from mock service.',
        details: e as Error,
      });
    }
  }
}
