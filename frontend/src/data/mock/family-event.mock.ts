import type { FamilyEvent } from '@/types/family';
import { faker } from '@faker-js/faker';
import { generateMockMembers } from './member.mock'; // Import mock members

export function generateMockFamilyEvent(
  index: number,
  familyId?: string,
  memberIds?: string[],
): FamilyEvent {
  const startDate = faker.date.past({ years: 5 });
  const endDate = faker.datatype.boolean()
    ? faker.date.soon({ refDate: startDate })
    : undefined;
  const location = faker.location.city() + ', ' + faker.location.country();

  // Select a few random members from the provided list
  const relatedMembers = memberIds
    ? faker.helpers.arrayElements(
        memberIds,
        faker.number.int({ min: 0, max: Math.min(memberIds.length, 3) }),
      )
    : [];

  return {
    id: `event-${index}`,
    name: `Event ${index}`,
    description: `Description for Event ${index}`,
    startDate: startDate,
    endDate: endDate,
    location: location,
    familyId: familyId || `family-${Math.floor(Math.random() * 5) + 1}`,
    relatedMembers: relatedMembers,
    type: faker.helpers.arrayElement([
      'Birth',
      'Marriage',
      'Death',
      'Migration',
      'Other',
    ]),
  };
}

export function generateMockFamilyEvents(
  count: number,
  familyId?: string,
): FamilyEvent[] {
  const events: FamilyEvent[] = [];
  const mockMembers = generateMockMembers(10, familyId); // Generate some members for the family
  const memberIds = mockMembers.map((m) => m.id);

  for (let i = 0; i < count; i++) {
    events.push(generateMockFamilyEvent(i + 1, familyId, memberIds)); // Pass member IDs
  }
  return events;
}
