import type { IFamilyService } from './family.service.interface';
import type { Family } from '@/types/family';
import type { FamilySearchFilter } from '@/types/family';
import type { Paginated } from '@/types/common';
import { fixedMockFamilies } from '@/data/mock/fixed.family.mock';
import { simulateLatency } from '@/utils/mockUtils';
import type { Result } from '@/types/common';
import { ok, err } from '@/types/common';
import type { ApiError } from '@/utils/api';

export class MockFamilyService implements IFamilyService {
  private _families: Family[] = fixedMockFamilies;

  get families(): Family[] {
    return [...this._families];
  }

  async fetch(): Promise<Result<Family[], ApiError>> {
    try {
      const families = await simulateLatency(this.families);
      return ok(families);
    } catch (e) {
      return err({ message: 'Failed to fetch families from mock service.', details: e as Error });
    }
  }
  async getById(id: string): Promise<Result<Family | undefined, ApiError>> {
    try {
      const family = await simulateLatency(this.families.find((f) => f.id === id));
      return ok(family);
    } catch (e) {
      return err({ message: `Failed to get family with ID ${id} from mock service.`, details: e as Error });
    }
  }
  async add(newItem: Omit<Family, 'id'>): Promise<Result<Family, ApiError>> {
    try {
      const familyToAdd = { ...newItem, id: 'mock-id-' + Math.random().toString(36).substring(7) };
      this._families.push(familyToAdd);
      const addedFamily = await simulateLatency(familyToAdd);
      return ok(addedFamily);
    } catch (e) {
      return err({ message: 'Failed to add family to mock service.', details: e as Error });
    }
  }
  async update(updatedItem: Family): Promise<Result<Family, ApiError>> {
    try {
      const index = this._families.findIndex((f) => f.id === updatedItem.id);
      if (index !== -1) {
        this._families[index] = updatedItem;
        const updatedFamily = await simulateLatency(updatedItem);
        return ok(updatedFamily);
      }
      return err({ message: 'Family not found', statusCode: 404 });
    } catch (e) {
      return err({ message: 'Failed to update family in mock service.', details: e as Error });
    }
  }
  async delete(id: string): Promise<Result<void, ApiError>> {
    try {
      const initialLength = this._families.length;
      this._families = this._families.filter((f) => f.id !== id);
      if (this._families.length === initialLength) {
        return err({ message: 'Family not found', statusCode: 404 });
      }
      await simulateLatency(undefined);
      return ok(undefined);
    } catch (e) {
      return err({ message: 'Failed to delete family from mock service.', details: e as Error });
    }
  }
  async loadItems(
    filter: FamilySearchFilter,
    page: number,
    itemsPerPage: number
  ): Promise<Result<Paginated<Family>, ApiError>> {
    try {
      let filtered = this._families;

      if (filter.searchQuery) {
        const lowerCaseSearchQuery = filter.searchQuery.toLowerCase();
        filtered = filtered.filter(
          (family) =>
            family.name.toLowerCase().includes(lowerCaseSearchQuery) ||
            (family.description && family.description.toLowerCase().includes(lowerCaseSearchQuery))
        );
      }

      if (filter.visibility && filter.visibility !== 'all') {
        filtered = filtered.filter((family) => (family as any).visibility === filter.visibility);
      }

      if (filter.startDate) {
        filtered = filtered.filter((family) => family.createdAt && new Date(family.createdAt) >= filter.startDate!);
      }

      if (filter.endDate) {
        filtered = filtered.filter((family) => family.createdAt && new Date(family.createdAt) <= filter.endDate!);
      }

      if (filter.location) {
        const lowerCaseLocation = filter.location.toLowerCase();
        filtered = filtered.filter((family) => family.address && family.address.toLowerCase().includes(lowerCaseLocation));
      }

      if (filter.type) {
        // Assuming 'type' refers to some property in Family, adjust as needed
        // filtered = filtered.filter((family) => family.someTypeProperty === filter.type);
      }

      const totalItems = filtered.length;
      const totalPages = Math.ceil(totalItems / itemsPerPage);
      const start = (page - 1) * itemsPerPage;
      const end = start + itemsPerPage;
      const items = filtered.slice(start, end);

      const paginatedResult = await simulateLatency({
        items,
        totalItems,
        totalPages,
      });
      return ok(paginatedResult);
    } catch (e) {
      return err({ message: 'Failed to search families from mock service.', details: e as Error });
    }
  }

  async getByIds(ids: string[]): Promise<Result<Family[], ApiError>> {
    try {
      const families = await simulateLatency(this.families.filter(f => ids.includes(f.id)));
      return ok(families);
    } catch (e) {
      return err({ message: 'Failed to get families by IDs from mock service.', details: e as Error });
    }
  }
}