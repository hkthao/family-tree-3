import type { FamilyEvent } from '@/types/family-event';
import { faker } from '@faker-js/faker';

export function generateMockFamilyEvent(index: number, familyId?: string): FamilyEvent {
  const startDate = faker.date.past({ years: 5 });
  const endDate = faker.datatype.boolean() ? faker.date.soon({ refDate: startDate }) : undefined;
  const location = faker.location.city() + ', ' + faker.location.country();
  const relatedMembers = Array.from({ length: faker.number.int({ min: 0, max: 3 }) }, () => faker.string.uuid()); // Generate random member IDs

  return {
    id: `event-${index}`,
    name: `Event ${index}`,
    description: `Description for Event ${index}`,
    startDate: startDate, // Renamed from date
    endDate: endDate,
    location: location,
    familyId: familyId || `family-${Math.floor(Math.random() * 5) + 1}`,
    relatedMembers: relatedMembers,
  };
}

export function generateMockFamilyEvents(count: number, familyId?: string): FamilyEvent[] {
  const events: FamilyEvent[] = [];
  for (let i = 0; i < count; i++) {
    events.push(generateMockFamilyEvent(i + 1, familyId)); // Pass index for predictable ID
  }
  return events;
}
