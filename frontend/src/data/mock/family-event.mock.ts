import type { FamilyEvent } from '@/types/family';
import { faker } from '@faker-js/faker';

// Assume member IDs from 1 to 1200 exist in mock.member.service.ts
const existingMemberIds: string[] = Array.from({ length: 1200 }, (_, i) => (i + 1).toString());

export function generateMockFamilyEvent(
  index: number,
  familyId?: string,
): FamilyEvent {
  const startDate = faker.date.past({ years: 5 });
  const endDate = faker.datatype.boolean()
    ? faker.date.soon({ refDate: startDate })
    : undefined;
  const location = faker.location.city() + ', ' + faker.location.country();

  // Select a few random members from the provided list
  const relatedMembers = faker.helpers.arrayElements(
    existingMemberIds,
    faker.number.int({ min: 0, max: Math.min(existingMemberIds.length, 3) }),
  );

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

  for (let i = 0; i < count; i++) {
    events.push(generateMockFamilyEvent(i + 1, familyId)); // Pass member IDs
  }
  return events;
}
