import type { IFamilyEventService } from './family-event.service.interface';
import type { FamilyEvent } from '@/types/family-event';
import type { Paginated } from '@/types/pagination';
import { generateMockFamilyEvents, generateMockFamilyEvent } from '@/data/mock/family-event.mock';
import { simulateLatency } from '@/utils/mockUtils';

export class MockFamilyEventService implements IFamilyEventService {
  private _familyEvents: FamilyEvent[] = generateMockFamilyEvents(10);

  get familyEvents(): FamilyEvent[] {
    return [...this._familyEvents];
  }

  async fetch(): Promise<FamilyEvent[]> {
    return simulateLatency(this.familyEvents);
  }

  async getById(id: string): Promise<FamilyEvent | undefined> {
    return simulateLatency(this.familyEvents.find((event) => event.id === id));
  }

  async add(newItem: Omit<FamilyEvent, 'id'>): Promise<FamilyEvent> {
    const familyEventToAdd = { ...newItem, id: generateMockFamilyEvent().id };
    this._familyEvents.push(familyEventToAdd);
    return simulateLatency(familyEventToAdd);
  }

  async update(updatedItem: FamilyEvent): Promise<FamilyEvent> {
    const index = this._familyEvents.findIndex((event) => event.id === updatedItem.id);
    if (index !== -1) {
      this._familyEvents[index] = updatedItem;
      return simulateLatency(updatedItem);
    }
    throw new Error('Family Event not found');
  }

  async delete(id: string): Promise<void> {
    const initialLength = this._familyEvents.length;
    this._familyEvents = this._familyEvents.filter((event) => event.id !== id);
    if (this._familyEvents.length === initialLength) {
      throw new Error('Family Event not found');
    }
    return simulateLatency(undefined);
  }

  async searchFamilyEvents(
    searchQuery: string,
    familyId?: string,
    page: number = 1,
    itemsPerPage: number = 10
  ): Promise<Paginated<FamilyEvent>> {
    let filtered = this._familyEvents;

    if (familyId) {
      filtered = filtered.filter((event) => event.familyId === familyId);
    }

    if (searchQuery) {
      const lowerCaseSearchQuery = searchQuery.toLowerCase();
      filtered = filtered.filter(
        (event) =>
          event.name.toLowerCase().includes(lowerCaseSearchQuery) ||
          (event.description && event.description.toLowerCase().includes(lowerCaseSearchQuery))
      );
    }

    const totalItems = filtered.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filtered.slice(start, end);

    return simulateLatency({
      items,
      totalItems,
      totalPages,
    });
  }
}
