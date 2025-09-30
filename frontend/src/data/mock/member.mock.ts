import { faker } from '@faker-js/faker';
import type { Member } from '@/types/family';
import { mockFamilies as fixedMockFamilies } from '@/data/mock/family.mock';
import { Gender } from '@/types';

// Generate some mock families to link members to
const mockFamilies = fixedMockFamilies;

export function generateMockMember(
  familyId?: string,
  overrides?: Partial<Member>,
  index?: number,
): Member {
  const selectedFamilyId = familyId || faker.helpers.arrayElement(mockFamilies).id;
  return {
    id: `member-${index || faker.string.uuid()}`,
    lastName: faker.person.lastName(), // Generate last name
    firstName: faker.person.firstName(), // Generate first name
    fullName: `${faker.person.firstName()} ${faker.person.lastName()}`.trim(), // Generate full name
    familyId: selectedFamilyId,
    gender: faker.helpers.arrayElement(Object.values(Gender)),
    dateOfBirth: faker.date.past({ years: 50 }),
    dateOfDeath: faker.helpers.arrayElement([
      undefined,
      faker.date.past({ years: 10 }),
    ]),
    avatarUrl: faker.image.avatar(),
    nickname: faker.person.firstName(),
    placeOfBirth: faker.location.city(),
    placeOfDeath: faker.helpers.arrayElement([undefined, faker.location.city()]),
    occupation: faker.person.jobTitle(),
    fatherId: faker.helpers.arrayElement([undefined, faker.string.uuid()]),
    motherId: faker.helpers.arrayElement([undefined, faker.string.uuid()]),
    spouseId: faker.helpers.arrayElement([undefined, faker.string.uuid()]),
    biography: faker.lorem.paragraph(),
    ...overrides, // Apply overrides
  };
}

export function generateMockMembers(
  count: number,
  specificFamilyId?: string,
): Member[] {
  const members: Member[] = [];
  for (let i = 0; i < count; i++) {
    members.push(generateMockMember(specificFamilyId, {}, i));
  }
  return members;
}

// Export mock families for external use if needed (e.g., in tests)
export { mockFamilies };
