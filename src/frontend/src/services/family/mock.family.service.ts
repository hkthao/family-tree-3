import type { IFamilyService } from './family.service.interface';
import fixedMockFamilies from '@/data/mock/families.json';
import { simulateLatency } from '@/utils/mockUtils';
import type { ApiError } from '@/plugins/axios';
import {
  err,
  type Family,
  type Result,
  ok,
  type FamilyFilter,
  type Paginated,
} from '@/types';

export class MockFamilyService implements IFamilyService {
  public families: Family[] = [...(fixedMockFamilies as unknown as Family[])];

  async fetch(): Promise<Result<Family[], ApiError>> {
    try {
      const families = await simulateLatency(this.families);
      return ok(families);
    } catch (e) {
      return err({
        name: 'ApiError',
        message: 'Failed to fetch families from mock service.',
        details: e as Error,
      });
    }
  }
  async getById(id: string): Promise<Result<Family | undefined, ApiError>> {
    try {
      const family = await simulateLatency(
        this.families.find((f) => f.id === id),
      );
      return ok(family);
    } catch (e) {
      return err({
        name: 'ApiError',
        message: `Failed to get family with ID ${id} from mock service.`,
        details: e as Error,
      });
    }
  }
  async add(newItem: Omit<Family, 'id'>): Promise<Result<Family, ApiError>> {
    try {
      const familyToAdd = {
        ...newItem,
        id: 'mock-id-' + Math.random().toString(36).substring(7),
      };
      this.families.push(familyToAdd);
      const addedFamily = await simulateLatency(familyToAdd);
      return ok(addedFamily);
    } catch (e) {
      return err({
        name: 'ApiError',
        message: 'Failed to add family to mock service.',
        details: e as Error,
      });
    }
  }
  async update(updatedItem: Family): Promise<Result<Family, ApiError>> {
    try {
      const index = this.families.findIndex((f) => f.id === updatedItem.id);
      if (index !== -1) {
        this.families[index] = updatedItem;
        const updatedFamily = await simulateLatency(updatedItem);
        return ok(updatedFamily);
      }
      return err({
        name: 'ApiError',
        message: 'Family not found',
        statusCode: 404,
      });
    } catch (e) {
      return err({
        name: 'ApiError',
        message: 'Failed to update family in mock service.',
        details: e as Error,
      });
    }
  }
  async delete(id: string): Promise<Result<void, ApiError>> {
    try {
      const initialLength = this.families.length;
      this.families = this.families.filter((f) => f.id !== id);
      if (this.families.length === initialLength) {
        return err({
          name: 'ApiError',
          message: 'Family not found',
          statusCode: 404,
        });
      }
      await simulateLatency(undefined);
      return ok(undefined);
    } catch (e) {
      return err({
        name: 'ApiError',
        message: 'Failed to delete family from mock service.',
        details: e as Error,
      });
    }
  }
  async loadItems(
    filter: FamilyFilter,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<Family>, ApiError>> {
    try {
      let filtered = this.families;

      if (filter.searchQuery) {
        const lowerCaseSearchQuery = filter.searchQuery.toLowerCase();
        filtered = filtered.filter(
          (family) =>
            family.name.toLowerCase().includes(lowerCaseSearchQuery) ||
            (family.description &&
              family.description.toLowerCase().includes(lowerCaseSearchQuery)),
        );
      }

      if (filter.visibility && filter.visibility !== 'all') {
        filtered = filtered.filter(
          (family) => (family as any).visibility === filter.visibility,
        );
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
      return err({
        name: 'ApiError',
        message: 'Failed to search families from mock service.',
        details: e as Error,
      });
    }
  }

  async getByIds(ids: string[]): Promise<Result<Family[], ApiError>> {
    try {
      const families = await simulateLatency(
        this.families.filter((f) => ids.includes(f.id)),
      );
      return ok(families);
    } catch (e) {
      return err({
        name: 'ApiError',
        message: 'Failed to get families by IDs from mock service.',
        details: e as Error,
      });
    }
  }

  async addItems(
    newItems: Omit<Family, 'id'>[],
  ): Promise<Result<string[], ApiError>> {
    try {
      const newIds: string[] = [];
      newItems.forEach((newItem) => {
        const familyToAdd = {
          ...newItem,
          id: 'mock-id-' + Math.random().toString(36).substring(7),
        };
        this.families.push(familyToAdd);
        newIds.push(familyToAdd.id);
      });
      await simulateLatency(undefined);
      return ok(newIds);
    } catch (e) {
      return err({
        name: 'ApiError',
        message: 'Failed to add multiple families to mock service.',
        details: e as Error,
      });
    }
  }
}
