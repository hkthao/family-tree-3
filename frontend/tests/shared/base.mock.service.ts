
import type { ApiError } from '@/utils/api';
import { simulateLatency } from '@/utils/mockUtils';
import { err, ok, type Result, type Paginated } from '@/types';

export type ErrorType =
  | 'load'
  | 'add'
  | 'update'
  | 'delete'
  | 'getById'
  | 'getByIds'
  | null;

export class MockCrudService<T extends { id: string }> {
  public items: T[] = [];
  public shouldThrowError: boolean = false;
  public errorType: ErrorType = null;

  constructor(initialItems: T[]) {
    this.items = JSON.parse(JSON.stringify(initialItems));
  }

  reset() {
    // This should be re-implemented in the child class to reset to its specific initial data
    this.shouldThrowError = false;
    this.errorType = null;
  }

  fetch(): Promise<Result<T[], ApiError>> {
    throw new Error('Method not implemented.');
  }

  async loadItems(
    filter: any,
    page: number,
    itemsPerPage: number,
  ): Promise<Result<Paginated<T>, ApiError>> {
    if (this.shouldThrowError && this.errorType === 'load') {
      return err({ message: 'Mock loadItems error' } as ApiError);
    }

    // Basic filtering for demonstration. Can be extended in a specific service mock.
    const filtered = this.items.filter((item) => {
      if (!filter?.searchQuery) return true;
      const lowerCaseQuery = filter.searchQuery.toLowerCase();
      return Object.values(item).some((value) =>
        String(value).toLowerCase().includes(lowerCaseQuery),
      );
    });

    const totalItems = filtered.length;
    const totalPages = Math.ceil(totalItems / itemsPerPage);
    const start = (page - 1) * itemsPerPage;
    const end = start + itemsPerPage;
    const paginatedItems = filtered.slice(start, end);

    return ok(
      await simulateLatency({
        items: paginatedItems,
        totalItems,
        totalPages,
      }),
    );
  }

  async getById(id: string): Promise<Result<T | undefined, ApiError>> {
    if (this.shouldThrowError && this.errorType === 'getById') {
      return err({ message: 'Mock getById error' } as ApiError);
    }
    const item = this.items.find((i) => i.id === id);
    return ok(await simulateLatency(item));
  }

  async add(newItem: Omit<T, 'id'>): Promise<Result<T, ApiError>> {
    if (this.shouldThrowError && this.errorType === 'add') {
      return err({ message: 'Mock add error' } as ApiError);
    }
    const itemToAdd = { ...newItem, id: new Date().getTime().toString() } as T;
    this.items.push(itemToAdd);
    return ok(await simulateLatency(itemToAdd));
  }

  async update(updatedItem: T): Promise<Result<T, ApiError>> {
    if (this.shouldThrowError && this.errorType === 'update') {
      return err({ message: 'Mock update error' } as ApiError);
    }
    const index = this.items.findIndex((i) => i.id === updatedItem.id);
    if (index !== -1) {
      this.items[index] = updatedItem;
      return ok(await simulateLatency(updatedItem));
    }
    return err({ message: 'Item not found', statusCode: 404 });
  }

  async delete(id: string): Promise<Result<void, ApiError>> {
    if (this.shouldThrowError && this.errorType === 'delete') {
      return err({ message: 'Mock delete error' } as ApiError);
    }
    const initialLength = this.items.length;
    this.items = this.items.filter((i) => i.id !== id);
    if (this.items.length === initialLength) {
      return err({ message: 'Item not found', statusCode: 404 });
    }
    return ok(await simulateLatency(undefined));
  }

  async getByIds(ids: string[]): Promise<Result<T[], ApiError>> {
    if (this.shouldThrowError && this.errorType === 'getByIds') {
      return err({ message: 'Mock getByIds error' } as ApiError);
    }
    const foundItems = this.items.filter((i) => ids.includes(i.id));
    return ok(await simulateLatency(foundItems));
  }
}
