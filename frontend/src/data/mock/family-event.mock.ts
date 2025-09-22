import type { FamilyEvent } from '@/types/family-event';

export function generateMockFamilyEvent(index: number, familyId?: string): FamilyEvent {
  return {
    id: `event-${index}`,
    name: `Event ${index}`,
    description: `Description for Event ${index}`,
    date: new Date(2023, Math.floor(Math.random() * 12), Math.floor(Math.random() * 28) + 1),
    familyId: familyId || `family-${Math.floor(Math.random() * 5) + 1}`,
  };
}

export function generateMockFamilyEvents(count: number, familyId?: string): FamilyEvent[] {
  const events: FamilyEvent[] = [];
  for (let i = 0; i < count; i++) {
    events.push(generateMockFamilyEvent(i + 1, familyId)); // Pass index for predictable ID
  }
  return events;
}
