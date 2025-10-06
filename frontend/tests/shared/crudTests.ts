import { describe, it, expect, vi } from 'vitest';
import { setupStoreForTesting } from './sharedStoreTestUtils';
import { DEFAULT_ITEMS_PER_PAGE } from '@/constants/pagination';
import { ok, err } from '@/types';

/**
 * Defines a standard suite of CRUD tests for a Pinia store.
 *
 * @param storeName - The name of the store, used for test descriptions.
 * @param useStore - The store hook (e.g., useFamilyStore).
 * @param mockService - The mocked service instance.
 * @param serviceName - The name of the service property in the store.
 * @param entitySample - A sample entity used for creation and update tests.
 */
export function defineCrudTests<
  T extends { id: string; name?: string; fullName?: string },
>(
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

      expect(store.error).toBeNull();
      expect(store.totalItems).toBe(initialTotal + 1);
    });

    it('adds an item and handles failure', async () => {
      mockService.shouldThrowError = true;
      mockService.errorType = 'add';
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const initialTotal = store.totalItems;
      const { id, ...newEntity } = entitySample;
      await store.addItem(newEntity);
      expect(store.error).not.toBeNull();
      expect(store.totalItems).toBe(initialTotal);
    });

    it('updates an item successfully', async () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const itemToUpdate = { ...store.items[0], name: 'Updated Name' };

      await store.updateItem(itemToUpdate);
      await store._loadItems(); // Re-fetch to confirm the update

      const updatedItem = store.items.find((i: T) => i.id === itemToUpdate.id);
      if (updatedItem && updatedItem.name) {
        expect(updatedItem?.name).toBe('Updated Name');
      } else if (updatedItem && updatedItem.fullName) {
        expect(updatedItem?.fullName).toBe('Updated Name');
      }
      expect(store.error).toBeNull();
    });

    it('updates an item and handles failure', async () => {
      mockService.shouldThrowError = true;
      mockService.errorType = 'update';
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const itemToUpdate = { ...store.items[0], name: 'Updated Name' };

      await store.updateItem(itemToUpdate);

      expect(store.error).not.toBeNull();
    });

    it('deletes an item successfully', async () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const initialTotal = store.totalItems;
      const itemToDelete = store.items[0];

      await store.deleteItem(itemToDelete.id);

      expect(store.totalItems).toBe(initialTotal - 1);
      expect(store.error).toBeNull();
    });

    it('deletes an item and handles failure', async () => {
      mockService.shouldThrowError = true;
      mockService.errorType = 'delete';
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const initialTotal = store.totalItems;
      const itemToDelete = store.items[0];

      await store.deleteItem(itemToDelete.id);

      expect(store.error).not.toBeNull();
      expect(store.totalItems).toBe(initialTotal);
    });

    it('gets an item by ID successfully', async () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const itemToFind = mockService.items[0];

      // Assuming getById returns the item directly or sets currentItem
      const fetchedItem = await store.getById(itemToFind.id);

      if (fetchedItem) {
        expect(fetchedItem.id).toBe(itemToFind.id);
      } else {
        expect(store.currentItem?.id).toBe(itemToFind.id);
      }
      expect(store.error).toBeNull();
    });

    it('handles failure when getting item by ID', async () => {
      mockService.shouldThrowError = true;
      mockService.errorType = 'getById';
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store.getById('non-existent-id');
      expect(store.currentItem).toBeNull();
    });

    it('sets the current page and reloads items', async () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const loadSpy = vi.spyOn(store, '_loadItems');

      await store.setPage(2);

      expect(store.currentPage).toBe(2);
      expect(loadSpy).toHaveBeenCalled();
    });

    it('does not set an invalid page', async () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const initialPage = store.currentPage;
      const loadSpy = vi.spyOn(store, '_loadItems');

      await store.setPage(0); // Invalid page
      expect(store.currentPage).toBe(initialPage);
      expect(loadSpy).not.toHaveBeenCalled();
    });

    it('sets items per page and reloads items', async () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const loadSpy = vi.spyOn(store, '_loadItems');

      await store.setItemsPerPage(20);

      expect(store.itemsPerPage).toBe(20);
      expect(store.currentPage).toBe(1);
      expect(loadSpy).toHaveBeenCalled();
    });

    it('does not set invalid items per page', async () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const initialItemsPerPage = store.itemsPerPage;
      const loadSpy = vi.spyOn(store, '_loadItems');

      await store.setItemsPerPage(0); // Invalid count

      expect(store.itemsPerPage).toBe(initialItemsPerPage);
      expect(loadSpy).not.toHaveBeenCalled();
    });

    it('sets current item', () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      const item = { ...entitySample, id: 'test-id' };

      store.setCurrentItem(item);

      expect(store.currentItem).toEqual(item);
      store.setCurrentItem(null);
      expect(store.currentItem).toBeNull();
    });

    it('gets items by IDs successfully', async () => {
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store._loadItems();
      const idsToFetch = mockService.items
        .slice(0, 2)
        .map((item: T) => item.id);
      await store.getByIds(idsToFetch);
      const fetchedItems = store.items;

      expect(fetchedItems.length).toBe(2);
      expect(fetchedItems[0].id).toBe(idsToFetch[0]);
      expect(store.error).toBeNull();
    });

    it('handles failure when getting items by IDs', async () => {
      mockService.shouldThrowError = true;
      mockService.errorType = 'getByIds';
      const store = setupStoreForTesting(useStore, serviceName, mockService);
      await store.getByIds(['non-existent-id']);
      const fetchedItems = store.items;
      expect(fetchedItems).toEqual([]);
      expect(store.error).not.toBeNull();
    });
  });
}
