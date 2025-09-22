import type { Family } from '@/types/family';
import type { IFamilyService } from './family.service.interface';
import { generateMockFamilies, generateMockFamily } from '@/data/mock/family.mock';

export class MockFamilyService implements IFamilyService {
  private families: Family[] = generateMockFamilies(10);

  private simulateLatency<T>(data: T): Promise<T> {
    return new Promise((resolve) => setTimeout(() => resolve(data), 300));
  }

  async fetchFamilies(): Promise<Family[]> {
    return this.simulateLatency(this.families);
  }

  async getFamilyById(id: string): Promise<Family | undefined> {
    const family = this.families.find((f) => f.id === id);
    return this.simulateLatency(family);
  }

  async addFamily(newFamily: Omit<Family, 'id'>): Promise<Family> {
    const familyToAdd = generateMockFamily(); // Generate a new ID and some default data
    Object.assign(familyToAdd, newFamily); // Overlay provided data
    this.families.push(familyToAdd);
    return this.simulateLatency(familyToAdd);
  }

  async updateFamily(updatedFamily: Family): Promise<Family> {
    const index = this.families.findIndex((f) => f.id === updatedFamily.id);
    if (index !== -1) {
      this.families[index] = updatedFamily;
      return this.simulateLatency(updatedFamily);
    }
    throw new Error('Family not found');
  }

  async deleteFamily(id: string): Promise<void> {
    const initialLength = this.families.length;
    this.families = this.families.filter((f) => f.id !== id);
    if (this.families.length === initialLength) {
      throw new Error('Family not found');
    }
    return this.simulateLatency(undefined);
  }
}
