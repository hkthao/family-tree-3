import type { Event } from '@/types/event/event';
import { EventType } from '@/types/event/event-type'; // Import EventType enum
import { faker } from '@faker-js/faker';

// Assume member IDs from 1 to 1200 exist in mock.member.service.ts
const existingMemberIds: string[] = Array.from({ length: 1200 }, (_, i) => (i + 1).toString());

export function generateMockEvent(
  index: number,
  familyId?: string,
): Event {
  const startDate = faker.date.between({ from: '2025-08-01T00:00:00.000Z', to: '2025-09-30T23:59:59.999Z' });
  const endDate = faker.datatype.boolean()
    ? faker.date.soon({ refDate: startDate, days: 30 })
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
    familyId: familyId || `family-00${(index % 5) + 1}`,
    relatedMembers: relatedMembers,
    type: faker.helpers.arrayElement(Object.values(EventType)), // Use enum values
    color: faker.color.rgb(), // Generate a random color
  };
}

export function generateMockEvents(
  count: number,
  familyId?: string,
): Event[] {
  const events: Event[] = [];

  for (let i = 0; i < count; i++) {
    events.push(generateMockEvent(i + 1, familyId)); // Pass member IDs
  }
  return events;
}
