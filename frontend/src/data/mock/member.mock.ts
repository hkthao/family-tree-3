import { faker } from '@faker-js/faker';
import type { Member } from '@/types/member';
import { generateMockFamilies } from '@/data/mock/family.mock';

// Generate some mock families to link members to
const mockFamilies = generateMockFamilies(5); 

export function generateMockMember(familyId?: string, overrides?: Partial<Member>): Member {
  const selectedFamilyId = familyId || faker.helpers.arrayElement(mockFamilies).id;
  return {
    id: faker.string.uuid(),
    fullName: faker.person.fullName(),
    familyId: selectedFamilyId,
    gender: faker.helpers.arrayElement(['male', 'female', 'other']),
    dateOfBirth: faker.date.past({ years: 50 }).toISOString().split('T')[0],
    dateOfDeath: faker.helpers.arrayElement([undefined, faker.date.past({ years: 10 }).toISOString().split('T')[0]]),
    avatarUrl: faker.image.avatar(),
    nickname: faker.person.firstName(),
    placeOfBirth: faker.location.city(),
    placeOfDeath: faker.helpers.arrayElement([undefined, faker.location.city()]),
    occupation: faker.person.jobTitle(),
    fatherId: faker.helpers.arrayElement([undefined, faker.string.uuid()]),
    motherId: faker.helpers.arrayElement([undefined, faker.string.uuid()]),
    spouseId: faker.helpers.arrayElement([undefined, faker.string.uuid()]),
    biography: faker.lorem.paragraph(),
    visibility: faker.helpers.arrayElement(['public', 'private', 'shared']),
    ...overrides, // Apply overrides
  };
}

export function generateMockMembers(count: number, specificFamilyId?: string): Member[] {
  const members: Member[] = [];
  for (let i = 0; i < count; i++) {
    members.push(generateMockMember(specificFamilyId));
  }
  return members;
}

// Export mock families for external use if needed (e.g., in tests)
export { mockFamilies };
