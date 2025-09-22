import type { Family } from '@/types/family';
import type { IFamilyService } from './family.service.interface';
import { generateMockFamilies, generateMockFamily } from '@/data/mock/family.mock';
import type { Paginated } from '@/types/pagination'; // Import generic Paginated interface

export class MockFamilyService implements IFamilyService {
  private families: Family[] = generateMockFamilies(10);

  private simulateLatency<T>(data: T): Promise<T> {
    return new Promise((resolve) => setTimeout(() => resolve(data), 300));
  }

  async fetch(): Promise<Family[]> { // Renamed from fetchFamilies
    return this.simulateLatency(this.families);
  }

  async getById(id: string): Promise<Family | undefined> { // Renamed from getFamilyById
    const family = this.families.find((f) => f.id === id);
    return this.simulateLatency(family);
  }

  async add(newItem: Omit<Family, 'id'>): Promise<Family> { // Renamed from addFamily
    const familyToAdd = generateMockFamily(); // Generate a new ID and some default data
    Object.assign(familyToAdd, newItem); // Overlay provided data
    this.families.push(familyToAdd);
    return this.simulateLatency(familyToAdd);
  }

  async update(updatedItem: Family): Promise<Family> { // Renamed from updateFamily
    const index = this.families.findIndex((f) => f.id === updatedItem.id);
    if (index !== -1) {
      this.families[index] = updatedItem;
      return this.simulateLatency(updatedItem);
    }
    throw new Error('Family not found');
  }

  async delete(id: string): Promise<void> { // Renamed from deleteFamily
    const initialLength = this.families.length;
    this.families = this.families.filter((f) => f.id !== id);
    if (this.families.length === initialLength) {
      throw new Error('Family not found');
    }
    return this.simulateLatency(undefined);
  }

  async searchFamilies(
    searchQuery: string,
    visibility: 'all' | 'public' | 'private',
    page: number,
    itemsPerPage: number
  ): Promise<Paginated<Family>> { // Use generic Paginated interface
    let filtered = this.families;

    if (searchQuery) {
      const lowerCaseSearchQuery = searchQuery.toLowerCase();
      filtered = filtered.filter(
        (family) =>
          family.name.toLowerCase().includes(lowerCaseSearchQuery) ||
          (family.description && family.description.toLowerCase().includes(lowerCaseSearchQuery))
      );
    }

    // Note: Family mock data does not currently have a 'visibility' property.
    // This filter will not have an effect unless mock data is updated.
    if (visibility !== 'all') {
      filtered = filtered.filter((family) => (family as any).visibility === visibility);
    }

    const totalItems = filtered.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const items = filtered.slice(start, end);

    return this.simulateLatency({
      items,
      totalItems,
      totalPages,
    });
  }
}
