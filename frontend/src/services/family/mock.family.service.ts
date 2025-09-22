import type { IFamilyService } from './family.service.interface';
import type { Family } from '@/types/family';
import type { Paginated } from '@/types/pagination';
import { generateMockFamilies } from '@/data/mock/family.mock';
import { simulateLatency } from '@/utils/mockUtils';

export class MockFamilyService implements IFamilyService {
  private _families: Family[] = generateMockFamilies(10);

  get families(): Family[] {
    return [...this._families];
  }

  async fetch(): Promise<Family[]> {
    return simulateLatency(this.families);
  }
  async getById(id: string): Promise<Family | undefined> {
    return simulateLatency(this.families.find((f) => f.id === id));
  }
  async add(newItem: Omit<Family, 'id'>): Promise<Family> {
    const familyToAdd = { ...newItem, id: 'mock-id-' + Math.random().toString(36).substring(7) };
    this._families.push(familyToAdd);
    return simulateLatency(familyToAdd);
  }
  async update(updatedItem: Family): Promise<Family> {
    const index = this._families.findIndex((f) => f.id === updatedItem.id);
    if (index !== -1) {
      this._families[index] = updatedItem;
      return simulateLatency(updatedItem);
    }
    throw new Error('Family not found');
  }
  async delete(id: string): Promise<void> {
    const initialLength = this._families.length;
    this._families = this._families.filter((f) => f.id !== id);
    if (this._families.length === initialLength) {
      throw new Error('Family not found');
    }
    return simulateLatency(undefined);
  }
  async searchFamilies(
    searchQuery: string,
    visibility: 'all' | 'public' | 'private',
    page: number,
    itemsPerPage: number
  ): Promise<Paginated<Family>> {
    let filtered = this._families;

    if (searchQuery) {
      const lowerCaseSearchQuery = searchQuery.toLowerCase();
      filtered = filtered.filter(
        (family) =>
          family.name.toLowerCase().includes(lowerCaseSearchQuery) ||
          (family.description && family.description.toLowerCase().includes(lowerCaseSearchQuery))
      );
    }

    if (visibility !== 'all') {
      filtered = filtered.filter((family) => (family as any).visibility === visibility);
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