import { describe, it, expect, vi } from 'vitest';
import { setupStoreForTesting } from './sharedStoreTestUtils';

export function defineCrudTests<T extends { id: string; name?: string; fullName?:string }>(
  storeName: string,
  useStore: () => any,
  mockService: any,
  serviceName: string,
  entitySample: T,
) {
  describe(`${storeName} CRUD Operations`, () => {
    it('fetches items successfully', async () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      expect(store.items.length).toBeGreaterThan(0);
      expect(store.loading).toBe(false);
      expect(store.error).toBeNull();
    });

    it('adds an item successfully', async () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const initialTotal = store.totalItems;
      const { id, ...newEntity } = entitySample;

      await store.addItem(newEntity);

      expect(store.totalItems).toBe(initialTotal + 1);
    });

    it('updates an item successfully', async () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const itemToUpdate = { ...store.items[0], name: 'Updated Name' };

      await store.updateItem(itemToUpdate);
      await store._loadItems();

      const updatedItem = store.items.find((i: T) => i.id === itemToUpdate.id);
      if (updatedItem.name) {
        expect(updatedItem?.name).toBe('Updated Name');
      } else if (updatedItem.fullName) {
        expect(updatedItem?.fullName).toBe('Updated Name');
      }
    });

    it('deletes an item successfully', async () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const initialTotal = store.totalItems;
      const itemToDelete = store.items[0];

      await store.deleteItem(itemToDelete.id);

      expect(store.totalItems).toBe(initialTotal - 1);
    });

    it('handles failure when fetching items', async () => {
      mockService.shouldThrowError = true;
      mockService.errorType = 'load' as const
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      expect(store.error).not.toBeNull();
      expect(store.items).toEqual([]);
    });
  });
}